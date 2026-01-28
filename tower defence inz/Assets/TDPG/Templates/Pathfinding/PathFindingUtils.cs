using UnityEngine;
using System.Collections.Generic;

namespace TDPG.Templates.Pathfinding
{
    /// <summary>
    /// A high-level helper class that simplifies interaction with the A* system.
    /// <br/>
    /// It handles the conversion between Unity World Coordinates (used by GameObjects) 
    /// and Grid Coordinates (used by the A* algorithm).
    /// </summary>
    public class PathFindingUtils
    {
        private AStar astar;
        private Grid.Grid grid;

        /// <summary>
        /// Initializes the utility with a reference to the active grid.
        /// </summary>
        /// <param name="grid">The logical data grid to perform searches on.</param>
        public PathFindingUtils(Grid.Grid grid)
        {
            if (grid == null)
            {
                Debug.LogError("PathFindingUtils constructed with null grid!");
                return;
            }
            this.grid = grid;
            astar = new AStar(grid);
        }

        /// <summary>
        /// Calculates a path from Start to End using World Coordinates.
        /// <br/>
        /// Automatically snaps input positions to the nearest grid cell indices before calculating.
        /// </summary>
        /// <param name="startWorld">The starting position in World Space.</param>
        /// <param name="endWorld">The target position in World Space.</param>
        /// <param name="canSwim">If true, allows traversal of Water tiles.</param>
        /// <param name="canFLy">If true, ignores all terrain obstacles.</param>
        /// <param name="canDestroyBuildings">If true, allows traversal of Building tiles (implying destruction).</param>
        /// <returns>
        /// A list of Grid Coordinates (x,y,0) representing the path. 
        /// <br/><b>Note:</b> The returned vectors are in Grid Space (Indices), not World Space.
        /// </returns>
        public List<Vector3> FindPath(Vector3 startWorld, Vector3 endWorld, bool canSwim, bool canFLy, bool canDestroyBuildings)
        {
            Vector2Int startCell = grid.GetXY(startWorld);
            Vector2Int endCell   = grid.GetXY(endWorld);

            Vector3 start = new Vector3(startCell.x, startCell.y, 0);
            Vector3 goal  = new Vector3(endCell.x, endCell.y, 0);

            // Pass the ability to A*
            List<Vector3> pathCells = astar.FindPath(start, goal, canSwim, canFLy, canDestroyBuildings);

            List<Vector3> worldPath = new List<Vector3>();
            foreach (var c in pathCells)
                worldPath.Add(c);

            return worldPath;
        }

    }

    
}