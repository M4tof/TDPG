using UnityEngine;




public class Grid
{
    private int width;
    private int height;
    private float cellSize;
    private int[,] grid;
    private TileType[,] typeGrid;
    //TODO dodać grid dla budynków kiedy będzie można je tworzyć
    
    public enum TileType
    {
        Empty,
        Wall,
        Water,
        Building
    }

    public Grid(int width, int height,float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        
        grid = new int[width, height];
        typeGrid = new TileType[width, height];

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                typeGrid[x,y] = TileType.Empty;
                Debug.Log(x+":"+y);
                Debug.DrawLine(GetWorldPosition(x,y),GetWorldPosition(x,y+1),Color.yellow,100f);
                Debug.DrawLine(GetWorldPosition(x,y),GetWorldPosition(x+1,y),Color.yellow,100f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0,height),GetWorldPosition(width,height),Color.yellow,100f);
        Debug.DrawLine(GetWorldPosition(width,0),GetWorldPosition(width,height),Color.yellow,100f);
    }

    
    //Set value for tile on grid based on world position
    public void SetValue(Vector3 worldPosition, int value)
    {
        Vector2Int position;
        position = GetXY(worldPosition);
        SetValue(position.x, position.y, value);
    }
    
    //Set value of given tile 
    public void SetValue(int x, int y, int value)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return;
        }
        grid[x, y] = value;
    }
    
    //Set value for tile type on grid based on world position
    public void SetTileType(Vector3 worldPosition, TileType value)
    {
        Vector2Int position;
        position = GetXY(worldPosition);
        SetTileType(position.x, position.y, value);
    }
    
    //Set tile type of given tile 
    public void SetTileType(int x, int y, TileType value)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return;
        }
        typeGrid[x, y] = value;
    }

    //Print in console grid tile on given position
    public void PrintGridCell(Vector3 worldPosition)
    {
        Debug.Log(GetXY(worldPosition));
    }
    
    //Get Tile based on given position
    private Vector2Int GetXY(Vector3 worldPosition)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPosition.x/cellSize), Mathf.FloorToInt(worldPosition.y/cellSize));
    }
    
    //convert grid to position into world (real) position 
    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }

    
}
