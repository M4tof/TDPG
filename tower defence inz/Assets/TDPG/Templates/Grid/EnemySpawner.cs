using UnityEngine;
using TDPG.Templates.Pathfinding;
using TDPG.Templates.Grid;
using Grid = TDPG.Templates.Grid.Grid;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab;
    public GridManager gridManager;
    public Transform destination;
    
    [Header("Spawn Settings")]
    public int spawnCount = 3;
    public bool spawnAfterStart = true;
    
    void Start()
    {
        if (spawnAfterStart)
            SpawnEnemies();
    }
    
    void SpawnEnemies()
    {
        if (enemyPrefab == null || gridManager == null || destination == null)
        {
            Debug.LogError("Spawner missing references!");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
            SpawnSingleEnemy();
    }
    private void SpawnSingleEnemy()
    {
        float half = 0.50f;
        
        Grid grid = gridManager.GetGrid();
        if (grid == null)
        {
            Debug.LogError("Grid not ready yet!");
            return;
        }

        // --- Pick a random valid empty tile ---
        Vector2Int spawnCell = FindValidSpawnCell(grid);

        // Convert tile => world position
        Vector3 worldPos = grid.GetWorldPosition(spawnCell.x, spawnCell.y);
        worldPos.z = 10;

        // Spawn
        GameObject enemy = Instantiate(enemyPrefab, worldPos, Quaternion.identity);

        // Set references on EnemyPathFollower
        EnemyPathFollower follower = enemy.GetComponent<EnemyPathFollower>();
        if (follower != null)
        {
            follower.gridManager = gridManager;
            follower.destinationObject = destination.gameObject;
            
            // Randomly decide enemy capabilities
            float randomValue = Random.value;

            switch (randomValue)
            {
                case < 0.3f: // 30% chance - Cannot swim
                    follower.canSwim = false;
                    follower.canDestroyBuildings = false;
                    follower.canFly = false;
                    break;
        
                case < 0.5f: // 20% chance - Can swim only
                    follower.canSwim = true;
                    follower.canDestroyBuildings = false;
                    follower.canFly = false;
                    break;
        
                case < 0.7f: // 20% chance - Can destroy buildings only
                    follower.canSwim = false;
                    follower.canDestroyBuildings = true;
                    follower.canFly = false;
                    break;
        
                case < 0.8f: // 10% chance - Can swim and destroy buildings
                    follower.canSwim = true;
                    follower.canDestroyBuildings = true;
                    follower.canFly = false;
                    break;
        
                default: // 20% chance - Can fly
                    follower.canSwim = false;
                    follower.canDestroyBuildings = false;
                    follower.canFly = true;
                    break;
            }
            
            float speed = Random.Range(0.5f, 4.0f);
            follower.speed = speed;
        }
        
    }

    // -------------------------------------------------------
    //  Finds a random EMPTY tile to spawn enemy on
    // -------------------------------------------------------
    private Vector2Int FindValidSpawnCell(Grid grid)
    {
        int width = grid.GetWidth();
        int height = grid.GetHeight();

        for (int attempts = 0; attempts < 200; attempts++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            var tile = grid.GetTileType(x, y);

            if (tile == Grid.TileType.EMPTY)
                return new Vector2Int(x, y);
        }

        // fallback if somehow no empty spaces
        return new Vector2Int(0, 0);
    }
}

