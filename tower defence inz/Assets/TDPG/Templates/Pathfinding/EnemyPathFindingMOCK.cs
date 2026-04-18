using System.Collections.Generic;
using UnityEngine;
using TDPG.Templates.Grid;

//!! MOCK used for pathfinding testing, not actually useful anymore. !!

namespace TDPG.Templates.Pathfinding
{
    public class EnemyPathFollowerMock : MonoBehaviour
    {
        public GridManager gridManager;
        [SerializeField] internal GameObject destinationObject;
        
        [Header("Movement Abilities")]
        [SerializeField] internal bool canSwim = false;
        [SerializeField] internal bool canFly = false;
        [SerializeField] internal bool canDestroyBuildings = false;
        [SerializeField] internal float speed = 2f;
        
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

        private System.Collections.IEnumerator WaitForGridThenInit()
        {
            // Wait until the GridManager has created the grid (a safety timeout could be added)
            while (gridManager != null && gridManager.GetGrid() == null)
                yield return null;

            // Small safeguard
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
                    index++; // Move to next point in path
                    
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
            
            // Signal others to recalculate
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
            // Last node
            Vector3 lastNode = (path[path.Count - 1] * gridManager.CellSize) + new Vector3(half, half, 0);
            Gizmos.DrawSphere(lastNode, 0.12f);
        }
        
        void OnDestroy()
        {
            PathfindingEvents.OnGridChanged -= OnGridChanged;
        }
        
        private void OnGridChanged()
        {
            // Recompute the path when grid changes (unless we've reached the destination)
            if (!hasReachedDestination)
            {
                ComputeNewPath();
            }
        }
        
        
    }
}