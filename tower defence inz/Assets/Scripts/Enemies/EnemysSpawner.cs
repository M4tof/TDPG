using UnityEngine;
using System.Collections.Generic;
using TDPG.Templates.Grid;
using TDPG.Templates.Pathfinding;

public class EnemysSpawner : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject EnemyPrefab;
    public Transform EndPoint;

    [Header("Runtime")]
    private EnemyFactory _factory;

    private bool spanwed = false; //TODO USUNĄĆ
    
    void Start()
    {
        EndPoint = GridManager.Instance.GetDestinationObject().transform;
        InitializeFactory();
    }

    void Update()
    {
        if (!spanwed)
        {
            SpawnEnemy("Walker",1);
            spanwed = true;
        }
    }

    private void InitializeFactory()
    {
        // 2. Initialize Factory with the Lambda Injection
        _factory = new EnemyFactory(GameManager.Instance.GSeed, GameManager.Instance.Slot, (data, overrides) =>
        {
            // This is the bridge: Factory asks for an object, we give the concrete implementation
            return new Enemy(data, overrides);
        });
    }

    // public void SpawnEnemy(string enemyID, int waveDifficulty)
    // {
    //     // 1. Get Data
    //     EnemyData data = EnemyRegistry.Instance.Get(enemyID);
    //     if (data == null) return;

    //     // 2. Generate Logic (Factory)
    //     // Returns EnemyBase, we cast to Enemy because we know our Lambda returns Enemy
    //     Enemy logicalEnemy = (Enemy)_factory.GenerateNextEnemy(data, waveDifficulty);

    //     // TODO: check if this *really* goes here. Gemini claims it does
    //     // 3. Inject Path (Placeholder for now)
    //     // logicalEnemy.SetPath(PathfindingSystem.GetPath(SpawnPoint, EndPoint));
    //     // logicalEnemy.Position = SpawnPoint; 

    //     // 4. Register
    //     EnemyCompendium.Instance.RegisterEnemy(logicalEnemy);

    //     // 5. Create Visuals
    //     GameObject go = Instantiate(EnemyPrefab, transform.position, Quaternion.identity);
    //     go.GetComponent<EnemyBehavior>().Initialize(logicalEnemy);
    // }

    // TODO: cleanup before release
    public void SpawnEnemy(string enemyID, int waveDifficulty, bool debug = false)
    {
        // Debug 1: Check Registry
        if (EnemyRegistry.Instance == null) { Debug.LogError("❌ Registry is NULL"); return; }

        EnemyData data = EnemyRegistry.Instance.Get(enemyID);
        if (data == null) { Debug.LogError($"❌ Enemy Data '{enemyID}' not found!"); return; }

        // Debug 2: Check Factory (Did Awake run?)
        if (_factory == null) { Debug.LogError("❌ Factory is NULL! InitializeFactory didn't run."); return; }

        Enemy logicalEnemy = (Enemy)_factory.GenerateNextEnemy(data, waveDifficulty);


        // Debug path

        if (debug)
        {
            // 1. Get Spawner's World Position
            Vector3 spawnerPos = transform.position;

            // 2. Snap to Grid Coordinates (int x, int y)
            // Note: GridManager might return bottom-left of cell, we want Center for movement
            Vector2Int gridStart = GridManager.Instance.WorldToGrid(spawnerPos);

            // 3. Define Path in GRID UNITS (Relative Steps)
            var gridSteps = new List<Vector2Int>
            {
                gridStart,                         // Start
                gridStart + new Vector2Int(2, 0),  // Right 2 cells
                gridStart + new Vector2Int(2, -2), // Down 2 cells
                gridStart + new Vector2Int(0, -2), // Left 2 cells
                gridStart                          // Back to Start
            };

            // 4. Convert Grid Steps -> World Positions (Centered)
            var worldPath = new List<Vector2>();
            foreach (var step in gridSteps)
            {
                // Use GridManager to get the CENTER of the tile
                // GridToWorld in your script returns center:
                // "return grid.GetWorldPosition(x, y) + new Vector3(cellSize * 0.5f, ...);"
                Vector3 worldPos = GridManager.Instance.GridToWorld(step.x, step.y);
                worldPath.Add(worldPos);
            }

            // 5. Apply to Enemy
            logicalEnemy.Position = worldPath[0]; // Snap logic to valid start
            logicalEnemy.SetPath(worldPath);
        }

        logicalEnemy.Position = transform.position;
        
        // Debug 3: Check Compendium (Is object in scene?)
        if (EnemyCompendium.Instance == null) { Debug.LogError("❌ EnemyCompendium is NULL! Missing GameObject in scene?"); return; }

        EnemyCompendium.Instance.RegisterEnemy(logicalEnemy);

        // ... Visuals ...
        Debug.Log($"ENEMY SPAWNER {transform.position}");
        GameObject go = Instantiate(EnemyPrefab, transform.position, Quaternion.identity);
        float cellSize = GridManager.Instance.CellSize;
        if (go.TryGetComponent(out BoxCollider2D col))
        {
            col.size *= cellSize;
        }
        go.GetComponent<EnemyBehavior>().Initialize(logicalEnemy);
        go.GetComponent<EnemyPathFollower>().Initialize(GridManager.Instance, EndPoint.gameObject);
    }

    public void DebugSpawn()
    {
        SpawnEnemy("Walker", 1, true);
    }

    public void ForceSpawnEnemy(EnemySaveData save)
    {
        EnemyData data = EnemyRegistry.Instance.Get(save.EnemyID);
        if (data == null) return;

        // Create Logic manually
        // Note: You might need to handle Seed/Overrides differently here for loading
        var logic = new Enemy(data, EnemyStatsOverride.Default);
        logic.CurrentHealth = save.Health;
        logic.Position = save.Position;

        // Create Visuals
        GameObject go = Instantiate(EnemyPrefab, save.Position, Quaternion.identity);
        if (go.TryGetComponent(out EnemyBehavior behavior))
        {
            behavior.Initialize(logic);
        }

        // Register
        EnemyCompendium.Instance.RegisterEnemy(logic);
    }

    public void SetEndPoint(Transform transform)
    {
        EndPoint = transform;
    }
}