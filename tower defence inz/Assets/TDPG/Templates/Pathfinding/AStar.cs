using System.Collections.Generic;
using UnityEngine;

// frontier = PriorityQueue()
// frontier.put(start, 0)
// came_from = dict()
// cost_so_far = dict()
// came_from[start] = None
// cost_so_far[start] = 0
//
// while not frontier.empty():
//      current = frontier.get()
//
//      if current == goal:
//          break
//
//      for next in graph.neighbors(current):
//          new_cost = cost_so_far[current] + graph.cost(current, next)
//          if next not in cost_so_far or new_cost < cost_so_far[next]:
//              cost_so_far[next] = new_cost
//              priority = new_cost + heuristic(goal, next)
//              frontier.put(next, priority)
//              came_from[next] = current

namespace TDPG.Templates.Pathfinding
{
    public class AStar
    {
        private Grid worldGrid;
        private Vector2Int start;
        private Vector2Int destination;

        private PriorityQueue<Vector2Int, float> frontier;
        private Dictionary<Vector2Int, Vector2Int?> cameFrom;
        private Dictionary<Vector2Int, float> costSoFar;

        public AStar(Grid grid, int startX, int startY, int endX, int endY)
        {
            worldGrid = grid;
            start = new Vector2Int(startX, startY);
            destination = new Vector2Int(endX, endY);
        }

        public List<Vector2Int> FindPath()
        {
            frontier = new PriorityQueue<Vector2Int, float>();
            cameFrom = new Dictionary<Vector2Int, Vector2Int?>();
            costSoFar = new Dictionary<Vector2Int, float>();

            frontier.Enqueue(start, 0);
            cameFrom[start] = null;
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();

                if (current == destination)
                    break;

                foreach (Vector2Int next in GetNeighbors(current))
                {
                    float newCost = costSoFar[current] + GetCost(current, next);

                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        float priority = newCost + Heuristic(destination, next);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }

            return ReconstructPath();
        }

        private List<Vector2Int> ReconstructPath()
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int? current = destination;

            while (current != null && cameFrom.ContainsKey(current.Value))
            {
                path.Add(current.Value);
                current = cameFrom[current.Value];
            }

            path.Reverse();
            return path;
        }

        // TODO: Replace with your own logic
        private IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            Vector2Int[] directions = {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };

            foreach (var dir in directions)
            {
                Vector2Int next = pos + dir;

                if (IsWalkable(next))
                    yield return next;
            }
        }

        private bool IsWalkable(Vector2Int pos)
        {
            // TODO: replace with worldGrid walkability check
            // e.g. return worldGrid.IsWalkable(pos.x, pos.y);
            return true;
        }

        private float GetCost(Vector2Int from, Vector2Int to)
        {
            // TODO: replace with grid-specific movement cost logic
            // e.g. 1 for normal tiles, 5 for difficult terrain
            return 1f;
        }

        private float Heuristic(Vector2Int a, Vector2Int b)
        {
            // Manhattan distance for grid-based maps
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        public void ChangeDestination(int endX, int endY)
        {
            destination = new Vector2Int(endX, endY);
        }

        public void UpdateMap(Grid grid)
        {
            worldGrid = grid;
        }

        public void UpdatePosition(int x, int y)
        {
            start = new Vector2Int(x, y);
        }

        // EXAMPLE USAGE:
        // Grid grid = FindObjectOfType<Grid>();
        // AStar astar = new AStar(grid, 0, 0, 5, 5);
        // List<Vector2Int> path = astar.FindPath();
        //
        // foreach (var p in path)
        // {
        //     Debug.Log($"Path step: {p}");
        // }
    }
}