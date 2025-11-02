using System.Collections.Generic;
using UnityEngine;

public class GridHelper : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    
    [Header("Map Settings")]
    public MapData mapData;
    
    [Header("References")]
    public Transform charObj;
    public Transform destObj;

    // 0 = walkable, 1 = wall
    private int[,] map;

    private void Awake()
    {
        if (mapData != null)
        {
            LoadMapFromData();
        }
        else
        {
            GenerateTestMap();
        }
        PlaceObjects();
    }

    private void LoadMapFromData()
    {
        if (mapData.mapBitmap == null || mapData.mapBitmap.Length == 0)
        {
            Debug.LogError("[GridHelper] MapData bitmap is empty!");
            GenerateTestMap();
            return;
        }

        height = mapData.mapBitmap.Length;
        width = mapData.mapBitmap[0].row.Length;
        map = new int[width, height];

        // Copy data from ScriptableObject into map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                map[x, y] = mapData.mapBitmap[y].row[x];
            }
        }

        Debug.Log($"[GridHelper] Loaded map from ScriptableObject {width}x{height}.");
    }

    private void GenerateTestMap()
    {
        // Example of a custom layout: 0 = walkable, 1 = wall
        int[,] layout = new int[,]
        {
            {0,0,0,0,0,0,1,1,1,1},
            {1,1,0,0,0,0,0,0,1,1},
            {1,1,1,1,0,1,0,0,1,0},
            {0,0,0,1,0,1,0,0,0,0},
            {0,1,0,1,0,1,1,1,1,0},
            {0,1,0,0,0,0,0,0,0,0},
            {0,1,1,1,1,1,0,1,1,0},
            {1,0,0,0,0,0,0,0,0,1}
        };

        height = layout.GetLength(0);
        width = layout.GetLength(1);
        map = new int[width, height];

        // Copy data into map (convert layout[y,x] to map[x,y])
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                map[x, y] = layout[y, x];
            }
        }

        Debug.Log($"[GridHelper] Loaded custom map {width}x{height}.");
    }

    private void PlaceObjects()
    {
        Vector3Int startCell = new Vector3Int(1, 1, 0);
        Vector3Int destCell = new Vector3Int(width - 2, height - 2, 0);

        // Find first walkable cell from top-left if default is blocked
        if (!IsWalkable(startCell))
            startCell = FindFirstWalkableCell();

        // Find first walkable cell from bottom-right if default is blocked or same as start
        if (!IsWalkable(destCell) || destCell == startCell)
            destCell = FindLastWalkableCell();

        if (charObj != null)
            charObj.position = CellToWorld(startCell);

        if (destObj != null)
            destObj.position = CellToWorld(destCell);

        Debug.Log($"[GridHelper] Placed char at {startCell}, dest at {destCell}");
    }
    
    private Vector3Int FindFirstWalkableCell()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (map[x, y] == 0)
                    return new Vector3Int(x, y, 0);
            }
        }
        return new Vector3Int(0, 0, 0); // fallback
    }
    
    private Vector3Int FindLastWalkableCell()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = width - 1; x >= 0; x--)
            {
                if (map[x, y] == 0)
                    return new Vector3Int(x, y, 0);
            }
        }
        return new Vector3Int(width - 1, height - 1, 0); // fallback
    }
    
    public bool IsWalkable(Vector3Int cell)
    {
        if (!InBounds(cell))
            return false;
        return map[cell.x, cell.y] == 0;
    }

    public bool InBounds(Vector3Int cell)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height;
    }

    public List<Vector3Int> GetNeighbors(Vector3Int cell, bool allowDiagonals = false)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        int[,] directions = allowDiagonals
            ? new int[,] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }, { 1, 1 }, { -1, 1 }, { 1, -1 }, { -1, -1 } }
            : new int[,] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            Vector3Int neighbor = new Vector3Int(cell.x + directions[i, 0], cell.y + directions[i, 1], 0);
            if (InBounds(neighbor) && IsWalkable(neighbor))
                result.Add(neighbor);
        }

        return result;
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int y = Mathf.FloorToInt(worldPos.y / cellSize);
        return new Vector3Int(x, y, 0);
    }

    public Vector3 CellToWorld(Vector3Int cell)
    {
        return new Vector3((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize, 0f);
    }

    private void OnDrawGizmos()
    {
        if (map == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = CellToWorld(new Vector3Int(x, y, 0));
                Gizmos.color = (map[x, y] == 1) ? Color.red : Color.white;
                Gizmos.DrawWireCube(pos, Vector3.one * (cellSize * 0.9f));
            }
        }

        if (charObj != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(charObj.position, 0.2f);
        }

        if (destObj != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(destObj.position, 0.2f);
        }
    }

    private Vector3 randomCell()
    {
        int X = Random.Range(0, width);
        int Y = Random.Range(0, height);
        
        Vector3 randomcell = CellToWorld(new Vector3Int(X, Y, 0));
        
        return randomcell; 
    }
    
    public void changeDestPosition()
    {
        Vector3 pos = randomCell();
        Vector3Int cell = WorldToCell(pos);
        
        if (InBounds(cell) && IsWalkable(cell))
        {
            destObj.position = pos;
            Debug.Log($"[GridHelper] Changed destination at {cell}");
        }
    }
    
    // Public method to reload map from ScriptableObject at runtime
    public void ReloadMap()
    {
        if (mapData != null)
        {
            LoadMapFromData();
            PlaceObjects();
        }
    }
    
}