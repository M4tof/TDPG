using UnityEngine;




public class GridOld
{
    /*
    public int width{get; set;}
    public int height{get; set;}
    public float cellSize {get; set;}
    public int[,] grid {get; set;}
    public TileType[,] typeGrid {get; set;}
    public GameObject[,] buildingsGrid {get; set;}
    public int[,] turretId {get; set;}
    
    public enum TileType
    {
        EMPTY,
        WALL,
        WATER,
        BUILDING,
        DONT_EXISTS        
    }

    public Grid(int width, int height,float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        
        grid = new int[width, height];
        typeGrid = new TileType[width, height];
        buildingsGrid = new GameObject[width, height];
        turretId = new int[width, height];

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                typeGrid[x,y] = TileType.EMPTY;
                buildingsGrid[x, y] = null;;
                turretId[x, y] = -1;
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
    
    //return value of tile
    public int GetValue(Vector3 worldPosition)
    {
        Vector2Int position =  GetXY(worldPosition);
        return grid[position.x, position.y];
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
    
    //return type of tile based on world position
    public TileType GetTileType(Vector3 worldPosition)
    {
        Vector2Int position =  GetXY(worldPosition);
        return GetTileType(position.x, position.y);
    }
    
    //return type of tile based of tile type
    public TileType GetTileType(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return TileType.DONT_EXISTS;
        }
        return typeGrid[x, y];
    }
    
    //Set Building based on world position
    public void SetBuilding(Vector3 worldPosition, GameObject building)
    {
        Vector2Int position;
        position = GetXY(worldPosition);
        SetBuilding(position.x, position.y, building);
    }
    
    //Set Building based on tile position
    public void SetBuilding(int x, int y, GameObject building)
    {
        buildingsGrid[x, y] = building;
    }
    
    //return building
    public GameObject GetBuilding(Vector3 worldPosition)
    {
        Vector2Int position = GetXY(worldPosition);
        return buildingsGrid[position.x, position.y];
    }
    
    //Set value for tile on grid based on world position
    public void SetTurretId(Vector3 worldPosition, int id)
    {
        Vector2Int position;
        position = GetXY(worldPosition);
        SetTurretId(position.x, position.y, id);
    }
    
    //return value of tile
    public int GetTurretId(Vector3 worldPosition)
    {
        Vector2Int position =  GetXY(worldPosition);
        return turretId[position.x, position.y];
    }
    
    //Set value of given tile 
    public void SetTurretId(int x, int y, int id)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return;
        }
        turretId[x, y] = id;
    }
    
    //Get Tile based on given position
    public Vector2Int GetXY(Vector3 worldPosition)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPosition.x/cellSize), Mathf.FloorToInt(worldPosition.y/cellSize));
    }
    
    //convert grid to position into world (real) position 
    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }
    
    //Print in console grid tile on given position
    public void PrintGridCell(Vector3 worldPosition)
    {
        Vector2Int position = GetXY(worldPosition);
        if (position.x < 0 || position.y < 0 || position.x >= width || position.y >= height)
        {
            return;
        }
        Debug.Log($"Tile: {position} Type: {typeGrid[position.x, position.y]} Building: {buildingsGrid[position.x, position.y]} Id: {turretId[position.x, position.y]}");
    }*/
}
