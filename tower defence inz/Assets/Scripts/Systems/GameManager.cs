using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using TDPG.Generators.Seed;
using TDPG.Generators.Scalars;
using System.IO;
using System.Text;
using System.Collections.Generic;
using TDPG.Templates.Grid;

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
        // if (RSInstance == null) RSInstance = ResourceSystem.Instance;
        if (scene.name == "MainGame" && !string.IsNullOrEmpty(PendingLoadPath))
        {
            // Now that the scene systems (like GridManager) exist, load the data.
            SetSlot(PendingLoadSlot);
            LoadGame(PendingLoadPath);

            // Clear pending data
            PendingLoadPath = null;
            PendingLoadSlot = 0;
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

        List<TurretSaveData> currentTurrets = new List<TurretSaveData>();

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
        }

        List<EnemySaveData> currentEnemies = new List<EnemySaveData>();
        if (EnemyCompendium.Instance != null)
        {
            currentEnemies = EnemyCompendium.Instance.GetSaveData();
        }

        GameSaveData data = new GameSaveData
        {
            // SlotNumber = Slot,
            GS = GSeed,
            Resources = ResourceSystem.Instance.GetData(),
            Elements = new ElementSaveData { },
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
            }
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
                GSeed = data.GS;
                if (ResourceSystem.Instance != null && data.Resources != null)
                {
                    ResourceSystem.Instance.LoadData(data.Resources);
                }
                else
                {
                    Debug.LogWarning("Skipping Resources Load: ResourceSystem or Data is null.");
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
                }
                else
                {
                    Debug.LogError("GridManager.Instance is NULL!");
                }

                if (EnemyCompendium.Instance != null)
                {
                    EnemyCompendium.Instance.LoadFromData(data.Enemies);
                }
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
}
