using System.Collections.Generic;
using TDPG.Templates.Pathfinding;
using UnityEngine;
using Grid = TDPG.Templates.Grid.Grid;

/// <summary>
/// A standard A* (A-Star) Pathfinding implementation tailored for the TDPG Grid system.
/// <br/>
/// Calculates the optimal path between nodes while accounting for specific traversal capabilities 
/// (Swimming, Flying, Destruction).
/// </summary>
public class AStar
{
    private Grid grid;

    // Persist these at the class level to reuse memory buffers
    private readonly PriorityQueue<Vector3, float> frontier = new();
    private readonly Dictionary<Vector3, Vector3?> cameFrom = new();
    private readonly Dictionary<Vector3, float> costSoFar = new();

    /// <summary>
    /// Initializes the pathfinder with a reference to the logical grid.
    /// </summary>
    /// <param name="grid">The data grid containing terrain info (Walls, Water, etc).</param>

    public AStar(Grid grid)
    {
        this.grid = grid;
    }

    /// <summary>
    /// Computes the shortest valid path from Start to Goal.
    /// </summary>
    /// <param name="start">World position of the starting node.</param>
    /// <param name="goal">World position of the target node.</param>
    /// <param name="canSwim">If true, treats Water tiles as traversable.</param>
    /// <param name="canFLy">If true, ignores terrain obstacles (Walls/Water).</param>
    /// <param name="canDestroyBuildings">If true, treats Building tiles as traversable.</param>
    /// <returns>
    /// A list of waypoints (Vector3) representing the path from Start to Goal. 
    /// Returns an empty list if the goal is unreachable.
    /// </returns>
    public List<Vector3> FindPath(Vector3 start, Vector3 goal, bool canSwim, bool canFLy, bool canDestroyBuildings)
    {
        frontier.Clear();
        cameFrom.Clear();
        costSoFar.Clear();

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

    /// <summary>
    /// Calculates the estimated cost from A to B.
    /// </summary>
    /// <remarks>
    /// Uses <b>Manhattan Distance</b> (Taxicab Geometry), which is efficient for grids 
    /// where movement is restricted to 4 cardinal directions.
    /// </remarks>
    private float Heuristic(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// <summary>
    /// Backtracks from the Goal node to the Start node using the 'cameFrom' map 
    /// to generate the final path list.
    /// </summary>
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