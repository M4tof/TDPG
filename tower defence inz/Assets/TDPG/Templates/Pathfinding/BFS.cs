using System.Collections.Generic;
using UnityEngine;

// frontier = Queue()
// frontier.put(start )
// came_from = dict() # path A->B is stored as came_from[B] == A
// came_from[start] = None
//
// while not frontier.empty():
//      current = frontier.get()
//      for next in graph.neighbors(current):
//          if next not in came_from:
//              frontier.put(next)
//              came_from[next] = current

namespace TDPG.Templates.Pathfinding
{
    public class BfsTilemap
    {
        private GridHelper grid;
        private Vector3Int start;
        private Vector3Int destination;

        private Queue<Vector3Int> frontier;
        private Dictionary<Vector3Int, Vector3Int?> cameFrom;

        public BfsTilemap(GridHelper gridHelper, Vector3Int startCell, Vector3Int destCell)
        {
            grid = gridHelper;
            start = startCell;
            destination = destCell;
        }

        public List<Vector3Int> FindPath()
        {
            frontier = new Queue<Vector3Int>();
            cameFrom = new Dictionary<Vector3Int, Vector3Int?>();

            frontier.Enqueue(start);
            cameFrom[start] = null;

            while (frontier.Count > 0)
            {
                Vector3Int current = frontier.Dequeue();

                // Stop if we reached the destination
                if (current == destination)
                    break;

                foreach (Vector3Int next in grid.GetNeighbors(current))
                {
                    if (!cameFrom.ContainsKey(next) && grid.IsWalkable(next))
                    {
                        frontier.Enqueue(next);
                        cameFrom[next] = current;
                    }
                }
            }

            return ReconstructPath();
        }

        private List<Vector3Int> ReconstructPath()
        {
            List<Vector3Int> path = new List<Vector3Int>();
            Vector3Int? current = destination;

            while (current != null && cameFrom.ContainsKey(current.Value))
            {
                path.Add(current.Value);
                current = cameFrom[current.Value];
            }

            path.Reverse();
            return path;
        }

        // Utility setters if you want to reuse the BFS object
        public void UpdateStart(Vector3Int newStart) => start = newStart;
        public void UpdateDestination(Vector3Int newDest) => destination = newDest;
    }
}
