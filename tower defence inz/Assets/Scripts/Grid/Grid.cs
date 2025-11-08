using UnityEngine;

public class Grid
{
    private int width;
    private int height;
    private float cellSize;
    private int[,] grid;

    public Grid(int width, int height,float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        
        grid = new int[width, height];

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Debug.Log(x+":"+y);
                Debug.DrawLine(GetWorldPosition(x,y),GetWorldPosition(x,y+1),Color.yellow,100f);
                Debug.DrawLine(GetWorldPosition(x,y),GetWorldPosition(x+1,y),Color.yellow,100f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0,height),GetWorldPosition(width,height),Color.yellow,100f);
        Debug.DrawLine(GetWorldPosition(width,0),GetWorldPosition(width,height),Color.yellow,100f);
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }
    
    public void SetValue(int x, int y, int value)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return;
        }
        grid[x, y] = value;
    }

    private Vector2Int GetXY(Vector3 worldPosition)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPosition.x/cellSize), Mathf.FloorToInt(worldPosition.y/cellSize));
    }

    public void SetValue(Vector3 worldPosition, int value)
    {
        Vector2Int position;
        position = GetXY(worldPosition);
        SetValue(position.x, position.y, value);
    }
    
    public void PrintGridCell(Vector3 worldPosition)
    {
        Debug.Log(GetXY(worldPosition));
    }
}
