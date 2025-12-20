using TDPG.Templates.Pathfinding;
using UnityEngine;

namespace TDPG.Templates.Grid
{
    /// <summary>
    /// Handles the instantiation and initialization of enemy units.
    /// <br/>
    /// Responsible for injecting dependencies (Grid, Destination) and procedurally generating 
    /// unit capabilities (Swimming, Flying, Speed) upon spawn.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The enemy template to spawn. Must contain an EnemyPathFollowerMock component.")] public GameObject enemyPrefab;
        [Tooltip("Reference to the global grid manager.")] public GridManager gridManager;
        [Tooltip("The target Transform (Base/Exit) that enemies will pathfind towards.")] public Transform destination;
    
        [Header("Spawn Settings")]
        [Tooltip("The total number of enemies to spawn.")] public int spawnCount = 3;
        [Tooltip("If true, spawning begins automatically on Start.")] public bool spawnAfterStart = true;
    
        void Start()
        {
            if (spawnAfterStart)
                SpawnEnemies();
        }
    
        /// <summary>
        /// Triggers the spawning loop. 
        /// Validates references before execution.
        /// </summary>
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
        
        /// <summary>
        /// Instantiates a single enemy, calculates a valid position, and applies random procedural stats.
        /// </summary>
        /// <remarks>
        /// <b>Procedural Generation Logic:</b>
        /// <br/>Capabilities are assigned via <see cref="Random.value"/> buckets:
        /// <list type="bullet">
        /// <item><description>30% - Standard (Walk only)</description></item>
        /// <item><description>20% - Amphibious (Walk + Swim)</description></item>
        /// <item><description>20% - Heavy (Walk + Destroy Buildings)</description></item>
        /// <item><description>10% - Juggernaut (Walk + Swim + Destroy)</description></item>
        /// <item><description>20% - Airborne (Fly)</description></item>
        /// </list>
        /// </remarks>
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
            EnemyPathFollowerMock follower = enemy.GetComponent<EnemyPathFollowerMock>();
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
        /// <summary>
        /// Attempts to find a random <see cref="Grid.TileType.EMPTY"/> cell within the grid bounds.
        /// </summary>
        /// <remarks>
        /// Uses a Monte Carlo approach (random sampling). Max 200 attempts.
        /// <br/>Fallback: Returns (0,0) if no empty tile is found.
        /// </remarks>
        /// <param name="grid">The data grid to search.</param>
        /// <returns>Grid coordinates (x,y).</returns>
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
}

