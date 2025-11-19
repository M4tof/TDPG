using UnityEngine;
using System.Collections.Generic;
using TDPG.Templates.Grid;

namespace TDPG.Templates.Pathfinding
{
    public class PathFindingUtils
    {
        private AStar astar;
        private Grid.Grid grid;

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