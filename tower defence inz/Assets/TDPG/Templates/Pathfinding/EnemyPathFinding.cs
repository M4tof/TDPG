using System.Collections.Generic;
using UnityEngine;
using TDPG.Templates.Grid;
using static TDPG.Templates.Pathfinding.PathfindingEvents;

namespace TDPG.Templates.Pathfinding
{
    /// <summary>
    /// A component that navigates an agent across the Grid towards a specific destination.
    /// <br/>
    /// It handles path calculation, capability-based movement (Swimming/Flying), and dynamic re-pathing 
    /// when the environment changes (e.g. walls destroyed).
    /// </summary>
    public class EnemyPathFollower : MonoBehaviour
    {
        [Tooltip("Reference to the GridManager to access tile data.")] public GridManager gridManager;
        [SerializeField] [Tooltip("The target GameObject the enemy is trying to reach.")] internal GameObject destinationObject;
        
        [Header("Movement Abilities")]
        [SerializeField][Tooltip("If true, the agent can traverse Water tiles.")] internal bool canSwim = false;
        [SerializeField][Tooltip("If true, the agent ignores all terrain obstacles.")] internal bool canFly = false;
        [SerializeField][Tooltip("If true, the agent treats Buildings as traversable (stops to destroy them).")] internal bool canDestroyBuildings = false;
        [SerializeField][Tooltip("Movement speed in World Units per second.")] internal float speed = 2f;
        
        private PathFindingUtils pathfinder;
        private List<Vector3> path;
        private int index;
        private bool isDestroyingBuilding = false;
        private bool hasReachedDestination = false;

        private Vector3 Destination => destinationObject != null ? destinationObject.transform.position : transform.position;
        private float half;
        
        void Start()
        {
            if (gridManager == null)
            {
                Debug.LogError("GridManager not assigned on EnemyPathFollower", this);
                return;
            }

            half = gridManager.CellSize * 0.5f;

            // If grid is already ready, initialize immediately, otherwise wait.
            if (gridManager.GetGrid() != null)
            {
                InitPathfinderAndCompute();
            }
            else
            {
                StartCoroutine(WaitForGridThenInit());
            }
            
            PathfindingEvents.OnGridChanged += OnGridChanged;
        }

        /// <summary>
        /// Coroutine that yields until the <see cref="GridManager"/> has a valid internal Grid.
        /// </summary>
        private System.Collections.IEnumerator WaitForGridThenInit()
        {
            // Wait until the GridManager has created the grid (a safety timeout could be added)
            while (gridManager != null && gridManager.GetGrid() == null)
                yield return null;

            // small safeguard
            if (gridManager == null || gridManager.GetGrid() == null)
            {
                Debug.LogError("Grid never became available for EnemyPathFollower on " + gameObject.name);
                yield break;
            }

            InitPathfinderAndCompute();
        }

        private void InitPathfinderAndCompute()
        {
            pathfinder = new PathFindingUtils(gridManager.GetGrid());
            ComputeNewPath();
        }

        /// <summary>
        /// Invokes the A* algorithm to calculate a fresh path from current position to destination.
        /// <br/>
        /// Takes into account current capabilities (Swim/Fly/Destroy).
        /// </summary>
        public void ComputeNewPath()
        {
            if (pathfinder == null || destinationObject == null)
            {
                path = null;
                return;
            }

            path = pathfinder.FindPath(transform.position, Destination, canSwim, canFly, canDestroyBuildings);
            index = 0;
            hasReachedDestination = false; // Reset when computing new path
        }

        void Update()
        {
            return;
            if (hasReachedDestination) return;
            
            if (path == null || path.Count == 0 || index >= path.Count || gridManager == null)
                return;
    
            Vector3 target = (path[index] * gridManager.CellSize) + new Vector3(half, half, 0);
    
            // Only move if not currently destroying a building
            if (!isDestroyingBuilding)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
            }

            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                if (canDestroyBuildings && gridManager.GetTileType(target) == Grid.Grid.TileType.BUILDING && !isDestroyingBuilding)
                {
                    // Start destroying the building
                    StartCoroutine(DestroyBuilding(target));
                }
                else
                {
                    index++; //move to next point in path
                    
                    // Check if we've reached the final destination
                    if (index >= path.Count)
                    {
                        hasReachedDestination = true;
                        path = null; // Clear the path to stop drawing gizmos
                    }
                }
            }
            
            // Additional check: if we're very close to the actual destination object, consider it reached
            if (destinationObject != null && Vector3.Distance(transform.position, destinationObject.transform.position) < 0.5f)
            {
                hasReachedDestination = true;
                path = null;
            }
        }

        /// <summary>
        /// Retrieves the current waypoint the agent should move towards.
        /// <br/>
        /// Advances the internal path index if the waypoint is reached.
        /// </summary>
        public Vector3 GetTargetPosition()
        {
            if (hasReachedDestination) return transform.position;
            
            if (path == null || path.Count == 0 || index >= path.Count || gridManager == null)
                return transform.position;
            
            //If Destroying Building
            if (isDestroyingBuilding)
            {
                return transform.position;
            }

            Vector3 target = (path[index] * gridManager.CellSize) + new Vector3(half, half, 0);
            
            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                index++; //move to next point in path
                    
                // Check if we've reached the final destination
                if (index >= path.Count)
                {
                    hasReachedDestination = true;
                    path = null; 
                }
                
                return (path[index] * gridManager.CellSize) + new Vector3(half, half, 0);
            }
            
            // Additional check: if we're very close to the actual destination object, consider it reached
            if (destinationObject != null && Vector3.Distance(transform.position, destinationObject.transform.position) < 0.5f)
            {
                hasReachedDestination = true;
                path = null;
                return transform.position;
            }
            
            return target;

        }

        /// <summary>
        /// Manual initialization method for instantiating via code.
        /// </summary>
        public void Initialize(GridManager gridManager, GameObject destinationObject)
        {
            this.gridManager = gridManager;
            this.destinationObject = destinationObject;
            Start();
        }
        
        /// <summary>
        /// Coroutine that simulates the time taken to destroy an obstacle.
        /// <br/>
        /// Once complete, it modifies the Grid via <see cref="GridManager"/> and triggers a global update.
        /// </summary>
        private System.Collections.IEnumerator DestroyBuilding(Vector3 buildingPosition)
        {
            isDestroyingBuilding = true;
    
            float waitTime = 1f;
            float counter = 0f;
    
            while (counter < waitTime)
            {
                counter += Time.deltaTime;
                yield return null;
            }
    
            // Destroy the building
            gridManager.SetTileType(buildingPosition, Grid.Grid.TileType.EMPTY);
            
            //signal others to recalculate
            PathfindingEvents.TriggerGridChanged();
            
            // Recompute path now that the building is gone
            ComputeNewPath();
    
            isDestroyingBuilding = false;
        }
        
        void OnDrawGizmos()
        {
            // Don't draw gizmos if we've reached the destination or path is null/empty
            if (hasReachedDestination || gridManager == null || path == null || path.Count == 0)
                return;

            Gizmos.color = Color.magenta; // Default - only walks
            if (canSwim && canDestroyBuildings)
            {
                Gizmos.color = Color.black; // Both swim and destroy buildings
            }
            else if (canFly)
            {
                Gizmos.color = Color.green; // Can fly
            }
            else if (canDestroyBuildings)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f); // Orange - can destroy buildings
            }
            else if (canSwim)
            {
                Gizmos.color = Color.cyan; // Can swim
            }
            
            float zOffset = 2.0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 current = (path[i] * gridManager.CellSize) + new Vector3(half, half, 0);
                Vector3 next = (path[i + 1] * gridManager.CellSize) + new Vector3(half, half, 0);

                current.z += zOffset;
                next.z += zOffset;
                
                Gizmos.DrawSphere(current, 0.12f);
                Gizmos.DrawLine(current, next);
            }
            // last node
            Vector3 lastNode = (path[path.Count - 1] * gridManager.CellSize) + new Vector3(half, half, 0);
            Gizmos.DrawSphere(lastNode, 0.12f);
        }
        
        void OnDestroy()
        {
            PathfindingEvents.OnGridChanged -= OnGridChanged;
        }
        
        /// <summary>
        /// Callback for <see cref="PathfindingEvents.OnGridChanged"/>.
        /// <br/>
        /// Forces path recalculation if the environment topology has been altered.
        /// </summary>
        private void OnGridChanged()
        {
            // Recompute path when grid changes (unless we've reached destination)
            if (!hasReachedDestination)
            {
                ComputeNewPath();
            }
        }
    }
}