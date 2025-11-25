using System;
using TDPG.Generators.Seed;
using TDPG.Templates.Grid.MapGen;
using TDPG.Templates.Turret;
using static TDPG.Generators.Scalars.InitializerFromDate;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TDPG.Templates.Grid
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; set; }
        
        [Header("Required elements")]
        [SerializeField] private Camera mainCamera;
    
        [Header("Parameters")]
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private float cellSize = 8;
        
        [Header("Map Generation")]
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private Texture2D emptyTileTexture;
        [SerializeField] private Texture2D wallTileTexture;
        [SerializeField] private Texture2D watterTileTexture;
        
        [Header("Debug")]
        [SerializeField] private GridDebugFiller debugFiller;
        
        private Grid grid;
        private GameObject[,] buildingsGrid;
        
        public Grid GetGrid() => grid;
        public float CellSize => cellSize;

        void Awake()
        {
            // Singleton pattern to ensure only one instance exists
            if (Instance != null && Instance != this)
            {
                // If another GameManager already exists, destroy this one
                Destroy(gameObject);
                Debug.LogWarning("Duplicate GridManager destroyed. Only one instance allowed.");
            }
            else
            {
                // If this is the first GameManager, make it the instance
                Instance = this;
                // Prevents the GameObject from being destroyed when reloading a scene
                DontDestroyOnLoad(gameObject);
                Debug.Log("GridManager created and set to not destroy on load.");
            }
        }

        void Start()
        {
            bool hasMapGenerator = false;
            if (mapGenerator != null)
            {
                hasMapGenerator = true;
                width = mapGenerator.Width;
                height = mapGenerator.Height;
            }
            
            grid = new Grid(width, height, cellSize);
            buildingsGrid = new GameObject[width, height];
            for (int x = 0; x < buildingsGrid.GetLength(0); x++)
            {
                for (int y = 0; y < buildingsGrid.GetLength(1); y++)
                {
                    buildingsGrid[x, y] = null;
                }
            }
            
            if (hasMapGenerator)
            {
                Debug.Log("Map generation initializing");
                //tmp
                GlobalSeed globalSeed = new GlobalSeed(QuickGenerate(1));
                
                Grid.TileType[,] mapData = mapGenerator.GenerateMap(globalSeed.NextSubSeed("TMPHERE"));
                
                ApplyMapToGrid(mapData);
            }
            
            // Initialize debug filler if assigned and not using mapGenerator
            else if (debugFiller != null)
            {
                debugFiller.Initialize(this);
            }
            
            //Set Camera
        }

        public void OnMouseClick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Vector3 mousePosition;
                mousePosition = Mouse.current.position.ReadValue();
                Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
                PrintGridCell(worldMousePosition);
            }
        }
        
        private void ApplyMapToGrid(Grid.TileType[,] mapData)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Grid.TileType type = grid.GetTileType(x, y);
                    
                    grid.SetTileType(x, y, mapData[x, y]);
                }
            }
        }
        
        private Texture2D GetTileTexture(Grid.TileType type)
        {
            switch (type)
            {
                case Grid.TileType.EMPTY:
                    return emptyTileTexture;
                case Grid.TileType.WALL:
                    return wallTileTexture;
                case Grid.TileType.WATER:
                    return watterTileTexture;
                default:
                    return emptyTileTexture;
            }
        }
        
        //Return tile postition on grid
        public Vector2 GetGridTilePosition(Vector3 worldPosition)
        {
            return grid.GetXY(worldPosition);
        }
    
        //Return tile world postition on grid
        public Vector2 GetGridWorldTilePosition(Vector3 worldPosition)
        {
            Vector2 tile = grid.GetXY(worldPosition);
            tile *= cellSize;
            return tile;
        }

        //Check if world point is on Grid
        public bool IsOnGrid(Vector3 worldPosition)
        {
            Vector2 tile = grid.GetXY(worldPosition);
            if (tile.x < 0 || tile.x >= width || tile.y < 0 || tile.y >= height)
            {
                return false;
            }

            return true;
        }
    
        //Check if world point is on Grid
        public bool IsTileOnGrid(Vector3 position)
        {
            if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
            {
                return false;
            }

            return true;
        }

        public bool CanPlaceTurret(Vector3 worldPosition, Vector2 TurretSize)
        {
            Debug.Log($"turret size: {TurretSize}");
            Vector2Int firstTile = grid.GetXY(worldPosition);
            for (int x = 0; x < TurretSize.x; x++)
            {
                for (int y = 0; y < TurretSize.y; y++)
                {
                    Vector3Int tile = new Vector3Int(firstTile.x + x, firstTile.y + y,0);
                    if (!IsTileOnGrid(tile))
                    {
                        Debug.Log($"Tile {tile} Out of Grid");
                        return false;
                    }

                    if (grid.GetTileType(tile.x,tile.y) != Templates.Grid.Grid.TileType.EMPTY)
                    {
                        Debug.Log($"Tile Blocked");
                        return false;
                    }
                }
            }
            return true;
        }

        public void PlaceTurret(Vector3 worldPosition, GameObject turret)
        {
            Debug.Log($"Place Turrets {turret}");
            TurretBase turretBase = turret.GetComponent<TurretBase>();
            Debug.Log($"Turret Base {turretBase}");
            if (turretBase == null)
            {
                Debug.Log("NULL Place Turrets");
                return;
            }
            Vector2Int firstTile = grid.GetXY(worldPosition);
            Vector2 turretSize = turretBase.GetTileSize();
            //Validation
            if (!CanPlaceTurret(worldPosition, turretSize))
            {
                Debug.Log("Can't Place Turrets");
                return;
            }
            //Placing turret on grid
            for (int x = 0; x < turretSize.x; x++)
            {
                for (int y = 0; y < turretSize.y; y++)
                {
                    Vector2Int tile = new Vector2Int(firstTile.x + x, firstTile.y + y);
                    buildingsGrid[tile.x, tile.y] = turret;
                    //grid.SetBuilding(tile.x,tile.y,turret);
                    grid.SetTileType(tile.x,tile.y,Grid.TileType.BUILDING);
                }
            }
            Debug.Log("FINISH Place Turrets");
        }
    
        
        public int GetWidth()
        {
            return width;
        }
        public int GetHeight()
        {
            return height;
        }
        
        public Vector3 GetCenterGrid()
        {
            return new Vector3(width * cellSize / 2, width * cellSize / 2, -10f);
        }

        internal Grid.TileType GetTileType(Vector3 worldPosition)
        {
            return grid.GetTileType(worldPosition);
        }

        internal void SetTileType(Vector3 worldPosition, Grid.TileType tileType)
        {
            grid.SetTileType(worldPosition,tileType);
        }
        
        public Grid GetCurrentGrid()
        {
            return grid;
        }

        public void SetCurrentGrid(Grid g)
        {
            grid = g;
        }
        
        //Set Building based on world position
        public void SetBuilding(Vector3 worldPosition, GameObject building)
        {
            Vector2Int position;
            position = grid.GetXY(worldPosition);
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
            Vector2Int position = grid.GetXY(worldPosition);
            return buildingsGrid[position.x, position.y];
        }
        
        public void PrintGridCell(Vector3 worldPosition)
        {
            Vector2Int position = grid.GetXY(worldPosition);
            if (position.x < 0 || position.y < 0 || position.x >= width || position.y >= height)
            {
                return;
            }
            string buildingInfo = "Building: " + buildingsGrid[position.x, position.y];
            grid.PrintGridCell(worldPosition,buildingInfo);
        }
        
        void OnValidate()
        {
            if (mainCamera == null)
            {
                Debug.LogWarning("Main Camera is not assigned", this);
            }
        }

        private void OnDrawGizmos()
        {
            if (grid == null)
                return;

            float tileSize = cellSize;
            float half = tileSize * 0.5f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Grid.TileType tileType = grid.GetTileType(x, y);

                    // Pick a color for each tile
                    switch (tileType)
                    {
                        case Grid.TileType.WALL:
                            Gizmos.color = Color.red;
                            break;
                        case Grid.TileType.WATER:
                            Gizmos.color = Color.blue;
                            break;
                        case Grid.TileType.BUILDING:
                            Gizmos.color = Color.yellow;
                            break;
                        default:
                            Gizmos.color = Color.green; // EMPTY or unknown
                            break;
                    }
                    
                    Vector3 center = new Vector3(
                        x * tileSize + half, 
                        y * tileSize + half,
                        3f);

                    Gizmos.DrawCube(center, new Vector3(tileSize, tileSize, 0.1f));
                }
            }
        }
        
        
        
    }
}
