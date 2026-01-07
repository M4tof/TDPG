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

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    // public ResourceSystem RSInstance;
    // public TurretCompendium TCInstance;
    // public ElementCompendium ECInstance;

    public GlobalSeed GSeed;

    public int Slot;

    public TDPG.Templates.Grid.Grid G;

    public int PendingLoadSlot;
    public string PendingLoadPath;

    public MapGenConfig PendingMapConfig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // RSInstance = ResourceSystem.Instance;
        // TCInstance = TurretCompendium.Instance;
        // ECInstance = ElementCompendium.Instance;
        Slot = 1;
        GSeed = new GlobalSeed(InitializerFromDate.QuickGenerate(Slot), "main", "Main global seed for this save slot");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            // If another GameManager already exists, destroy this one
            Destroy(gameObject);
            Debug.LogWarning("Duplicate GameManager destroyed. Only one instance allowed.");
        }
        else
        {
            // If this is the first GameManager, make it the instance
            Instance = this;
            // Prevents the GameObject from being destroyed when reloading a scene
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager created and set to not destroy on load.");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGame")
        {
            if (scene.name == "MainGame")
            {
                Seed CSSeed = GSeed.NextSubSeed(InitializerFromDate.QuickGenerate(Slot).ToString());
                List<ChoseAndApplyPalette> ColorSwappers = GetComponents<ChoseAndApplyPalette>().ToList();
                foreach (var cs in ColorSwappers)
                {
                    cs.seed = CSSeed;
                }
                // PRIORITY 1: LOADING A SAVE
                // If we have a save file pending, we MUST load it and IGNORE the new game config.
                if (!string.IsNullOrEmpty(PendingLoadPath))
                {
                    Debug.Log($"[GameManager] Loading Save from: {PendingLoadPath}");

                    // Clear any lingering New Game config so it doesn't interfere
                    PendingMapConfig = null;

                    SetSlot(PendingLoadSlot);
                    LoadGame(PendingLoadPath);

                    PendingLoadPath = null;
                    PendingLoadSlot = 0;
                }
                // PRIORITY 2: NEW GAME CONFIG
                // Only use this if we are NOT loading a save
                else if (PendingMapConfig != null)
                {
                    Debug.Log($"[GameManager] Generating New Map via Config.");

                    if (GridManager.Instance != null)
                    {
                        GridManager.Instance.ConfigureMap(PendingMapConfig);
                    }
                    PendingMapConfig = null; // Consume
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
                                GridY = y
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

        GameSaveData data = new GameSaveData
        {
            // SlotNumber = Slot,
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
                // BuildingGrid = G.turretId
                DestX = dX,
                DestY = dY,
                SpawnerPositions = savedSpawners
            },
            CardNextId = CNextId
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
            // 1. Read from File
            string json = File.ReadAllText(path, Encoding.UTF8);

            // 2. Deserialize using Newtonsoft.Json
            GameSaveData data = JsonConvert.DeserializeObject<GameSaveData>(json);

            // 3. Apply Data to all systems
            if (data != null)
            {
                // Slot = data.SlotNumber;

                // Seed
                GSeed = data.GS;

                //  ResourceSystem
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
                    // Use Registry's static method to handle the custom converters
                    var loadedRegistry = TDPG.EffectSystem.ElementRegistry.Registry.Deserialize(data.SerializedRegistry);

                    // 2. Inject into Manager
                    if (RegistryManager.Instance != null)
                    {
                        RegistryManager.Instance.LoadRegistry(loadedRegistry);
                    }

                    // 3. Refresh Cache (Important!)
                    if (ElementCompendium.Instance != null)
                    {
                        ElementCompendium.Instance.RefreshCache();
                    }
                }
                else
                {
                    Debug.LogWarning("Save File did not contain Registry data. Using defaults.");
                }


                // TODO: implement rest of the systems
                if (data.GData != null)
                {
                    G = new TDPG.Templates.Grid.Grid(data.GData.Width, data.GData.Height, data.GData.CellSize);
                    G.grid = data.GData.Grid;
                    G.typeGrid = data.GData.TypeGrid;
                }
                // G.turretId = data.GData.BuildingGrid;
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
                            // Convert Grid X,Y to World Position for the Spawner
                            // (Or update ForceSpawnTurret to take Grid Coordinates as discussed in TODO)
                            Vector3 worldPos = GridManager.Instance.GridToWorld(tData.GridX, tData.GridY);

                            // Spawner handles instantiation AND calling GridManager.PlaceTurret
                            spawner.ForceSpawnTurret(tData.TurretID, worldPos);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("TurretSpawner not found. Turrets will not be placed.");
                    }

                    GridManager.Instance.ForceRebuildScene();

                    GameObject virtualBase = new GameObject("Base_Destination");
                    // virtualBase.tag = "Base";
                    virtualBase.transform.position = GridManager.Instance.GetDestinationWorldPosition();

                    // Find ALL spawners
                    var allSpawners = FindObjectsByType<EnemysSpawner>(FindObjectsSortMode.None);
                    foreach (var s in allSpawners)
                    {
                        s.SetEndPoint(virtualBase.transform);
                    }
                }
                else
                {
                    Debug.LogError("GridManager.Instance is NULL!");
                }

                if (EnemyCompendium.Instance != null)
                {
                    EnemyCompendium.Instance.LoadFromData(data.Enemies);
                }

                FindFirstObjectByType<CardSelectionMenu>().nextId = data.CardNextId;
                Debug.Log($"Game Loaded successfully. Version: {data.SaveVersion}");
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
        Debug.Log($"[GameManager.StartNewGame(): config]: \n{Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented)}");
        // Debug.Break();
        // 1. Set Active Slot
        SetSlot(slotIndex);

        // 2. Wipe Save File
        string path = System.IO.Path.Combine(Application.persistentDataPath, $"SaveSlot{slotIndex}.json");
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            Debug.Log($"[GameManager] Wiped save slot {slotIndex}");
        }

        // 3. Regenerate Global Seed
        RegenSeed();

        // 4. Store Config for GridManager
        PendingMapConfig = config;
        Debug.Log($"[GameManager.StartNewGame(): PendingMapConfig]: \n{Newtonsoft.Json.JsonConvert.SerializeObject(PendingMapConfig, Newtonsoft.Json.Formatting.Indented)}");
        // 5. Load the Game Scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
    }
}
