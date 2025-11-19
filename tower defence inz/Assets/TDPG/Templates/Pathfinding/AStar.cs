using System.Collections.Generic;
using TDPG.Templates.Pathfinding;
using UnityEngine;
using Grid = TDPG.Templates.Grid.Grid;

public class AStar
{
    private Grid grid;

    private PriorityQueue<Vector3, float> frontier;
    private Dictionary<Vector3, Vector3?> cameFrom;
    private Dictionary<Vector3, float> costSoFar;

    public AStar(Grid grid)
    {
        this.grid = grid;
    }

    public List<Vector3> FindPath(Vector3 start, Vector3 goal, bool canSwim, bool canFLy, bool canDestroyBuildings)
    {
        frontier = new PriorityQueue<Vector3, float>();
        cameFrom = new Dictionary<Vector3, Vector3?>();
        costSoFar = new Dictionary<Vector3, float>();

        frontier.Enqueue(start, 0);
        cameFrom[start] = null;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Vector3 current = frontier.Dequeue();

            if (current == goal)
                break;

            foreach (Vector3 next in grid.GetNeighbors(new Vector3Int((int)current.x, (int)current.y, 0), canSwim, canFLy, canDestroyBuildings))
            {
                float newCost = costSoFar[current] + 1;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    float priority = newCost + Heuristic(next, goal);

                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
            
        }

        return ReconstructPath(goal);
    }

    private float Heuristic(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector3> ReconstructPath(Vector3 goal)
    {
        List<Vector3> path = new List<Vector3>();

        if (!cameFrom.ContainsKey(goal))
            return path;

        Vector3? current = goal;

        while (current != null)
        {
            path.Add(current.Value);
            current = cameFrom[current.Value];
        }

        path.Reverse();
        return path;
    }
    
    
}