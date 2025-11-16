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

        // Spawn
        GameObject enemy = Instantiate(enemyPrefab, worldPos, Quaternion.identity);

        // Set references on EnemyPathFollower
        EnemyPathFollower follower = enemy.GetComponent<EnemyPathFollower>();
        if (follower != null)
        {
            follower.gridManager = gridManager;
            follower.destinationObject = destination.gameObject;

            // Randomly decide if enemy can swim
            follower.canSwim = Random.value < 0.7f;
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

