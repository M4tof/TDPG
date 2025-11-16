using UnityEngine;
using Newtonsoft.Json;
using TDPG.Generators.Seed;
using TDPG.Generators.Scalars;
using System.IO;
using System.Text;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    public ResourceSystem RSInstance;
    public TurretCompendium TCInstance;
    public ElementCompendium ECInstance;

    public GlobalSeed GSeed;

    public int Slot;

    public Grid G;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
            RSInstance = ResourceSystem.Instance;
            TCInstance = TurretCompendium.Instance;
            ECInstance = ElementCompendium.Instance;
            Slot = 1;
            GSeed = new GlobalSeed(InitializerFromDate.QuickGenerate(Slot), "main", "Main global seed for this save slot");
        }
    }

    public void SetSlot(int s)
    {
        Slot = s;
    }

    public void GetGrid()
    {
        
        G = FindObjectOfType<GridManager>().GetCurrentGrid();
    }
    public void SaveGame(string path)
    {
        GetGrid();
        GameSaveData data = new GameSaveData
        {
            // SlotNumber = Slot,
            GS = GSeed,
            Resources = RSInstance.GetData(),
            Elements = new ElementSaveData{},
            Turrets = new List<TurretSaveData>(),
            GData = new GridSaveData
            {
                Width = G.width,
                Height = G.height,
                CellSize = G.CellSize,
                Grid = G.grid,
                TypeGrid = G.typeGrid,
                BuildingGrid = G.turretId
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
                RSInstance.LoadData(data.Resources);
                // TODO: implement rest of the systems
                G = new Grid(data.GData.Width, data.GData.Height, data.GData.CellSize);
                G.grid = data.GData.Grid;
                G.typeGrid = data.GData.TypeGrid;
                G.turretId = data.GData.BuildingGrid;
                Debug.Log($"Game Loaded successfully. Version: {data.SaveVersion}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
        }

    }

    public void RegenSeed()
    {
        GSeed = new GlobalSeed(InitializerFromDate.QuickGenerate(Slot), "main", "Main global seed for this save slot");
    }
}
