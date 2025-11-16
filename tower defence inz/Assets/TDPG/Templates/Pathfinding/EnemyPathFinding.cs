using System.Collections.Generic;
using UnityEngine;
using TDPG.Templates.Grid;

namespace TDPG.Templates.Pathfinding
{
    public class EnemyPathFollower : MonoBehaviour
    {
        public GridManager gridManager;
        [SerializeField] internal GameObject destinationObject;
        
        [Header("Movement Abilities")]
        [SerializeField]
        internal bool canSwim = false;
        
        private PathFindingUtils pathfinder;
        private List<Vector3> path;
        private int index;

        

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
        }

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

        public void ComputeNewPath()
        {
            if (pathfinder == null || destinationObject == null)
            {
                path = null;
                return;
            }

            path = pathfinder.FindPath(transform.position, Destination, canSwim);
            index = 0;
        }

        void Update()
        {
            if (path == null || path.Count == 0 || index >= path.Count || gridManager == null)
                return;
            
            Vector3 target = (path[index] * gridManager.CellSize) + new Vector3(half, half, 0);

            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * 2f);

            if (Vector3.Distance(transform.position, target) < 0.1f)
                index++;
        }

        void OnDrawGizmos()
        {
            if (gridManager == null || path == null || path.Count == 0)
                return;

            Gizmos.color = Color.magenta;
            if (canSwim)
            {
                Gizmos.color = Color.cyan;
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
    }
}
