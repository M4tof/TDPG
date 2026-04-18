using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using TDPG.Generators.Seed;
using TDPG.Generators.Scalars;
using System.IO;
using System.Text;
using System.Collections.Generic;
using TDPG.Templates.Grid;
using TDPG.Templates.Grid.MapGen;
using TDPG.VideoGeneration;
using System.Linq;
using TDPG.AudioModulation;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GlobalSeed GSeed;

    public int Slot;

    public TDPG.Templates.Grid.Grid G;

    public int PendingLoadSlot;
    public string PendingLoadPath;

    public MapGenConfig PendingMapConfig;
    public Seed CSSeed;
    public Seed ACSeed1;
    public Seed ACSeed2;

    void Start()
    {
        Slot = 1;
        GSeed = new GlobalSeed(InitializerFromDate.QuickGenerate(Slot), "main", "Main global seed for this save slot");
    }
    void Update()
    {

    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Duplicate GameManager destroyed. Only one instance allowed.");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager created and set to not destroy on load.");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (this != Instance) return;
        if (scene.name == "MainGame")
        {
            Debug.Log($"[GameManager] OnSceneLoaded fired for {scene.name}. Config: {(PendingMapConfig != null ? "Yes" : "No")}, Save: {(PendingLoadPath != null ? "Yes" : "No")}");

            if (scene.name == "MainGame")
            {
                CSSeed = GSeed.NextSubSeed(InitializerFromDate.QuickGenerate(Slot).ToString());
                List<BaseColorSwapController> ColorSwappers = FindObjectsByType<BaseColorSwapController>(FindObjectsSortMode.None).ToList();
                Debug.Log($"ColorSwappers.Count: {ColorSwappers.Count}");
                foreach (var cs in ColorSwappers)
                {
                    cs.SetSeed(CSSeed);
                }

                ACSeed1 = GSeed.NextSubSeed(InitializerFromDate.QuickGenerate(Slot).ToString());
                ACSeed2 = GSeed.NextSubSeed(InitializerFromDate.QuickGenerate(Slot).ToString());
                List<ProceduralAudioController> AudioControllers = FindObjectsByType<ProceduralAudioController>(FindObjectsSortMode.None).ToList();
                Debug.Log($"AudioControllers.Count: {AudioControllers.Count}");
                foreach (var ac in AudioControllers)
                {
                    ac.selectionSeed = ACSeed1.GetBaseValue();
                    ac.modulationSeed = ACSeed2.GetBaseValue();
                }

                // PRIORITY 1: LOADING A SAVE
                if (!string.IsNullOrEmpty(PendingLoadPath))
                {
                    Debug.Log($"[GameManager] Loading Save from: {PendingLoadPath}");
                    PendingMapConfig = null;

                    SetSlot(PendingLoadSlot);
                    LoadGame(PendingLoadPath);

                    PendingLoadPath = null;
                    PendingLoadSlot = 0;
                }
                // PRIORITY 2: NEW GAME CONFIG
                else if (PendingMapConfig != null)
                {
                    Debug.Log($"[GameManager] Generating New Map via Config.");

                    if (GridManager.Instance != null)
                    {
                        GridManager.Instance.ConfigureMap(PendingMapConfig);
                        GridManager.Instance.globalSeed = GameManager.Instance.GSeed;
                    }
                    PendingMapConfig = null;
                }
            }
        }
    }
    public void SetSlot(int s)
    {
        Slot = s;
    }

    public void GetGrid()
    {

        if (GridManager.Instance != null) G = GridManager.Instance.GetCurrentGrid();
    }
    public void SaveGame(string path)
    {
        GetGrid();

        int dX = 0, dY = 0;
        if (GridManager.Instance != null)
        {
            var d = GridManager.Instance.GetDestinationGridPosition();
            dX = d.x; dY = d.y;
        }

        List<TurretSaveData> currentTurrets = new List<TurretSaveData>();
        List<Vec3> savedSpawners = new List<Vec3>();

        if (GridManager.Instance != null && G != null)
        {
            var gridRef = GridManager.Instance;
            for (int x = 0; x < G.width; x++)
            {
                for (int y = 0; y < G.height; y++)
                {
                    GameObject building = gridRef.GetBuilding(gridRef.GridToWorld(x, y));
                    if (building != null)
                    {
                        var tb = building.GetComponent<TDPG.Templates.Turret.TurretBase>();
                        if (tb != null && tb.Data != null)
                        {
                            currentTurrets.Add(new TurretSaveData
                            {
                                TurretID = tb.Data.TurretID,
                                GridX = x,
                                GridY = y,
                                Upgrades = tb.GetPlayerCardApplied()
                            });
                        }
                    }
                }
            }

            var positions = GridManager.Instance.GetSpawnerPositions();
            if (positions != null)
            {
                foreach (var pos in positions)
                {
                    savedSpawners.Add(new Vec3(pos.x, pos.y, pos.z));
                }
            }


        }

        string regData = "";
        if (RegistryManager.Instance != null)
        {
            var reg = RegistryManager.Instance.GetRegistry();
            if (reg != null)
            {
                regData = reg.Serialize();
            }
        }

        List<EnemySaveData> currentEnemies = new List<EnemySaveData>();
        if (EnemyCompendium.Instance != null)
        {
            currentEnemies = EnemyCompendium.Instance.GetSaveData();
        }

        int CNextId = FindFirstObjectByType<CardSelectionMenu>().nextId;

        WaveSaveData waveData = new WaveSaveData();
        if (WaveManager.Instance != null)
        {
            waveData = WaveManager.Instance.GetSaveData();
        }

        MapBoundsData boundsData = new MapBoundsData();
        var mapGen = FindFirstObjectByType<TDPG.Templates.Grid.MapGen.MapGenerator>();
        if (mapGen != null)
        {
            mapGen.GetBoundsValues(out int minX, out int maxX, out int minY, out int maxY);

            boundsData.MinX = minX;
            boundsData.MaxX = maxX;
            boundsData.MinY = minY;
            boundsData.MaxY = maxY;
        }

        GameSaveData data = new GameSaveData
        {
            GS = GSeed,
            Resources = ResourceSystem.Instance.GetData(),
            Elements = new ElementSaveData { },
            SerializedRegistry = regData,
            Turrets = currentTurrets,
            Enemies = currentEnemies,
            GData = new GridSaveData
            {
                Width = G.width,
                Height = G.height,
                CellSize = G.cellSize,
                Grid = G.grid,
                TypeGrid = G.typeGrid,
                DestX = dX,
                DestY = dY,
                SpawnerPositions = savedSpawners,
                MapBounds = boundsData
            },
            CardNextId = CNextId,
            WaveState = waveData
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        path = Path.Combine(Application.persistentDataPath, path);
        File.WriteAllText(path, json, Encoding.UTF8);

        Debug.Log($"Game saved to: {path}.");
    }

    public void LoadGame(string path)
    {
        path = Path.Combine(Application.persistentDataPath, path);
        if (!File.Exists(path))
        {
            Debug.LogError($"No save file found at: {path}");
            return;
        }

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            GameSaveData data = JsonConvert.DeserializeObject<GameSaveData>(json);

            if (data != null)
            {
                GSeed = data.GS;

                if (ResourceSystem.Instance != null && data.Resources != null)
                {
                    ResourceSystem.Instance.LoadData(data.Resources);
                }
                else
                {
                    Debug.LogWarning("Skipping Resources Load: ResourceSystem or Data is null.");
                }

                // Registry
                if (!string.IsNullOrEmpty(data.SerializedRegistry))
                {
                    var loadedRegistry = TDPG.EffectSystem.ElementRegistry.Registry.Deserialize(data.SerializedRegistry);

                    if (RegistryManager.Instance != null)
                    {
                        RegistryManager.Instance.LoadRegistry(loadedRegistry);
                    }

                    if (ElementCompendium.Instance != null)
                    {
                        ElementCompendium.Instance.RefreshCache();
                    }
                }
                else
                {
                    Debug.LogWarning("Save File did not contain Registry data. Using defaults.");
                }


                if (data.GData != null)
                {
                    G = new TDPG.Templates.Grid.Grid(data.GData.Width, data.GData.Height, data.GData.CellSize);
                    G.grid = data.GData.Grid;
                    G.typeGrid = data.GData.TypeGrid;
                    if (GridManager.Instance != null)
                    {
                        GridManager.Instance.SetCurrentGrid(G);
                        GridManager.Instance.SetLoadedDestination(data.GData.DestX, data.GData.DestY);

                        if (data.GData.SpawnerPositions != null)
                        {
                            List<Vector3Int> positions = new List<Vector3Int>();
                            foreach (var vec in data.GData.SpawnerPositions)
                            {
                                positions.Add(new Vector3Int((int)vec.x, (int)vec.y, (int)vec.z));
                            }
                            GridManager.Instance.SetLoadedSpawners(positions);
                        }
                        GridManager.Instance.ClearMap();

                        for (int x = 0; x < G.width; x++)
                        {
                            for (int y = 0; y < G.height; y++)
                            {
                                GridManager.Instance.UpdateTileVisual(x, y, G.GetTileType(x, y));
                            }
                        }

                        var spawner = FindFirstObjectByType<TurretSpawner>();
                        if (spawner != null)
                        {
                            foreach (var tData in data.Turrets)
                            {
                                Vector3 worldPos = GridManager.Instance.GridToWorld(tData.GridX, tData.GridY);

                                spawner.modifiersList = tData.Upgrades;
                                spawner.ForceSpawnTurret(tData.TurretID, worldPos);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("TurretSpawner not found. Turrets will not be placed.");
                        }


                        var mapGen = FindFirstObjectByType<TDPG.Templates.Grid.MapGen.MapGenerator>();
                        if (mapGen != null)
                        {
                            var b = data.GData.MapBounds;
                            mapGen.RestoreBoundsValues(b.MinX, b.MaxX, b.MinY, b.MaxY);
                        }

                        GridManager.Instance.ForceRebuildScene();

                        GameObject virtualBase = new GameObject("Base_Destination");
                        virtualBase.transform.position = GridManager.Instance.GetDestinationWorldPosition();

                        var allSpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
                        foreach (var s in allSpawners)
                        {
                            s.SetEndPoint(virtualBase.transform);
                        }
                    }
                    else
                    {
                        Debug.LogError("GridManager.Instance is NULL!");
                    }
                }
                else
                {
                    Debug.Log("[GameManager] no grid in save data?????");
                }

                if (EnemyCompendium.Instance != null)
                {
                    EnemyCompendium.Instance.LoadFromData(data.Enemies);
                }

                FindFirstObjectByType<CardSelectionMenu>().nextId = data.CardNextId;
                Debug.Log($"Game Loaded successfully. Version: {data.SaveVersion}");

                if (WaveManager.Instance != null && data.WaveState != null)
                {
                    WaveManager.Instance.LoadFromData(data.WaveState);
                }
                GridManager.Instance.ApplyMapToGridWithTilemap(data.GData.TypeGrid);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CRITICAL LOAD ERROR: {e.Message}\nStack Trace: {e.StackTrace}");
        }

    }

    public void RegenSeed()
    {
        GSeed = new GlobalSeed(InitializerFromDate.QuickGenerate(Slot), "main", "Main global seed for this save slot");
    }

    public void StartNewGame(int slotIndex, MapGenConfig config)
    {
        PendingLoadPath = null;
        Debug.Log($"[GameManager.StartNewGame(): config]: \n{Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented)}");

        SetSlot(slotIndex);

        string path = System.IO.Path.Combine(Application.persistentDataPath, $"SaveSlot{slotIndex}.json");
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            Debug.Log($"[GameManager] Wiped save slot {slotIndex}");
        }

        RegenSeed();

        PendingMapConfig = config;
        Debug.Log($"[GameManager.StartNewGame(): PendingMapConfig]: \n{Newtonsoft.Json.JsonConvert.SerializeObject(PendingMapConfig, Newtonsoft.Json.Formatting.Indented)}");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
    }
}
