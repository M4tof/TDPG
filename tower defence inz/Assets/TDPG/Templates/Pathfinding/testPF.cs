using System;
using System.Collections.Generic;
using UnityEngine;

public class Script : MonoBehaviour
{
    [Header("References")]
    public GridHelper grid;
    public Transform charObj;
    public Transform destObj;

    [Header("Visualization Settings")]
    public Color pathColor = Color.red;
    public float lineWidth = 0.05f;
    public float moveSpeed = 1f;

    private bool moving = false;
    private int pathIndex = 0;
    
    private List<Vector3Int> currentPath;
    private LineRenderer lineRenderer;

    [Obsolete("Obsolete")]
    void Start()
    {
        if (grid == null)
            grid = FindObjectOfType<GridHelper>();

        if (charObj == null)
            charObj = grid.charObj;
        if (destObj == null)
            destObj = grid.destObj;

        // Add a line renderer for visualization
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.startColor = pathColor;
        lineRenderer.endColor = pathColor;
        lineRenderer.positionCount = 0;
        lineRenderer.sortingOrder = 10;
        
        Debug.Log($"Starting BFS from {charObj.position} to {destObj.position}");
        RunBfsAndDrawPath();
        pathIndex = 0;
        moving = true;
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.P))
        // {
        //     Debug.Log($"Starting BFS from {charObj.position} to {destObj.position}");
        //     RunBfsAndDrawPath();
        //     pathIndex = 0;
        //     moving = true;
        // }

        if (moving && currentPath != null && pathIndex < currentPath.Count)
        {
            Vector3 targetPos = grid.CellToWorld(currentPath[pathIndex]);
            
            charObj.position = Vector3.MoveTowards(charObj.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(charObj.position, targetPos) < 0.05f)
            {
                Debug.Log($"Reached stop at {targetPos}");
                pathIndex++;
            }

            if (pathIndex >= currentPath.Count)
            {
                // moving = false;
                Debug.Log("Reached destination!");
            }

            if (grid.destObj.position != currentPath[currentPath.Count - 1])
            {
                Debug.Log("Detected change of dest!");
                RunBfsAndDrawPath();
            }
        }
    }

    void RunBfsAndDrawPath()
    {
        Vector3Int start = grid.WorldToCell(charObj.position);
        Vector3Int goal = grid.WorldToCell(destObj.position);

        currentPath = BFS(start, goal);

        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning("No path found!");
            lineRenderer.positionCount = 0;
            return;
        }

        Vector3[] worldPositions = new Vector3[currentPath.Count];
        
        //Translate from grid to world pos
        for (int i = 0; i < currentPath.Count; i++)
            worldPositions[i] = grid.CellToWorld(currentPath[i]);

        lineRenderer.positionCount = worldPositions.Length;
        lineRenderer.SetPositions(worldPositions);
        Debug.Log($"Path found with {currentPath.Count} nodes.");
    }

    List<Vector3Int> BFS(Vector3Int start, Vector3Int goal)
    {
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        frontier.Enqueue(start);

        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current == goal)
                break;

            foreach (var next in grid.GetNeighbors(current))
            {
                if (!cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(goal))
            return null;

        // Reconstruct path
        List<Vector3Int> path = new List<Vector3Int>();
        var temp = goal;
        while (temp != start)
        {
            path.Add(temp);
            temp = cameFrom[temp];
        }
        path.Add(start);
        path.Reverse();

        return path;
    }
}
