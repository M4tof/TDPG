using System.Collections.Generic;
using TDPG.Generators.Scalars;
using UnityEngine;
using static TDPG.Generators.Scalars.InitializerFromDate;

namespace TDPG.Templates.Grid
{
    public class Grid
    {
        public int width;
        public int height;
        public float cellSize;
        public int[,] grid;
        public TileType[,] typeGrid;
        public int[,] turretId {get; set;}
    
        private IntGenerator gridIntXRand;
        private IntGenerator gridIntYRand;
    
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
        
            gridIntXRand = new IntGenerator { min = 0, max = width };
            gridIntYRand = new IntGenerator { min = 0, max = height };
        
            grid = new int[width, height];
            typeGrid = new TileType[width, height];
            
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    typeGrid[x,y] = TileType.EMPTY;
                    
                    //buildingsGrid[x, y] = null;
                    // Debug.Log(x+":"+y);
                    Debug.DrawLine(GetWorldPosition(x,y),GetWorldPosition(x,y+1),Color.yellow,100f);
                    Debug.DrawLine(GetWorldPosition(x,y),GetWorldPosition(x+1,y),Color.yellow,100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0,height),GetWorldPosition(width,height),Color.yellow,100f);
            Debug.DrawLine(GetWorldPosition(width,0),GetWorldPosition(width,height),Color.yellow,100f);
        }

        internal int GetWidth()
        {
            return width;
        }
        internal int GetHeight()
        {
            return height;
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
    
        //Get Tile based on given position
        public Vector2Int GetXY(Vector3 worldPosition)
        {
            return new Vector2Int(Mathf.FloorToInt(worldPosition.x/cellSize), Mathf.FloorToInt(worldPosition.y/cellSize));
        }
    
        //convert grid to position into world (real) position 
        internal Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * cellSize;
        }
    
        //Print in console grid tile on given position
        public void PrintGridCell(Vector3 worldPosition, string buildingInfo)
        {
            Vector2Int position = GetXY(worldPosition);
            if (position.x < 0 || position.y < 0 || position.x >= width || position.y >= height)
            {
                return;
            }
            Debug.Log($"Tile: {position} Type: {typeGrid[position.x, position.y]} {buildingInfo}");
        }
    
    
        private Vector3 randomTileInGrid()
        {
            var rngX = new SplitMix64Random(QuickGenerate(1));
            var rngY = new SplitMix64Random(QuickGenerate(2));

            int x = gridIntXRand.Generate(rngX);
            int y = gridIntYRand.Generate(rngY);
            return new Vector3(x,y,0);
        }

        private bool InBounds(Vector3 worldPosition)
        {
            Vector2Int cell = GetXY(worldPosition);
            return cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height;
        }

        private bool IsWalkable(Vector3 worldPosition)
        {
            if (!InBounds(worldPosition))
                return false;

            TileType tileType = GetTileType(worldPosition);

            return tileType == TileType.EMPTY;
        }
        
        private bool IsSwimmable(Vector3 worldPosition)
        {
            if (!InBounds(worldPosition))
                return false;

            TileType tileType = GetTileType(worldPosition);

            return tileType == TileType.WATER;
        }

    
        public IEnumerable<Vector3> GetNeighbors(Vector3 cell, bool canSwim, bool canFly, bool canDestroyBuildings, bool allowDiagonals = false)
        {
            int[,] directions = allowDiagonals
                ? new int[,] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }, { 1, 1 }, { -1, 1 }, { 1, -1 }, { -1, -1 } }
                : new int[,] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

            int cx = (int)cell.x;
            int cy = (int)cell.y;

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int nx = cx + directions[i, 0];
                int ny = cy + directions[i, 1];

                if (!InBoundsCell(nx, ny))
                    continue;

                TileType tile = typeGrid[nx, ny];

                // WALKABLE logic:
                // EMPTY always OK
                // WATER only OK if canSwim
                if (tile == TileType.EMPTY ||
                    (tile == TileType.WATER && canSwim) ||
                    canFly||
                    (tile == TileType.BUILDING && canDestroyBuildings)
                    )
                {
                    yield return new Vector3(nx, ny, 0);
                }
            }
            
        }
        
        private bool InBoundsCell(int x, int y)
        {
            return x >= 0 && y >= 0 && x < width && y < height;
        }

        private bool IsWalkableCell(int x, int y)
        {
            if (!InBoundsCell(x, y))
                return false;

            return typeGrid[x, y] == TileType.EMPTY;
        }

        private Vector3 FindFirstWalkableCell()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsWalkable(new Vector3Int(x,y,0)))
                        return new Vector3Int(x, y, 0);
                }
            }
            return new Vector3Int(0, 0, 0); // fallback
        }

        private Vector3 FindLastWalkableCell()
        {
            for (int y = height-1; y >= 0; y--)
            {
                for (int x = width - 1; x >= 0; x--)
                {
                    if (IsWalkable(new Vector3Int(x,y,0)))
                        return new Vector3Int(x, y, 0);
                }
            }
            return new Vector3Int(0, 0, 0); // fallback
        }

        public float GetCellSize()
        {
            return cellSize;
        }
    
    }
}

