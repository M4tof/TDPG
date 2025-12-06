using System;
using System.Collections.Generic;
using UnityEngine;
using TDPG.Generators.Seed;
using TDPG.Templates.Grid.MapGen;
using TDPG.Templates.Turret;
using static TDPG.Generators.Scalars.InitializerFromDate;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace TDPG.Templates.Grid
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; set; }

        [Header("Required elements")] [SerializeField]
        private Camera mainCamera;

        [Header("Parameters")] private int width = 10;
        private int height = 10;
        [SerializeField] private float cellSize = 1;

        [Header("Map Generation")] [SerializeField]
        private MapGenerator mapGenerator;

        [Header("Tilemap")] [SerializeField] private Tilemap tilemap;
        [SerializeField] private Tilemap fogTilemap;
        [SerializeField] private UnityEngine.Grid gridComponent;
        [SerializeField] private TileBase emptyTile;
        [SerializeField] private TileBase wallTile;
        [SerializeField] private TileBase waterTile;
        [SerializeField] private TileBase nullTile;

        [Header("Spawns")] [SerializeField] private GameObject Player;
        [SerializeField] private GameObject EnemySpawnerPrefab;
        [SerializeField] private GameObject DestinationPrefab;

        [Tooltip("Game Object which would be a parent for spawned EnemySpawners")] [SerializeField]
        private GameObject SpawnerContainer;

        [Header("Debug")] [SerializeField] private GridDebugFiller debugFiller;

        private Grid grid;
        private GameObject[,] buildingsGrid;
        private bool mapGenerated = false;
        private int numOfEnemySpawners;

        private Vector3Int destpos;
        private Vector3Int[] spawnerPositions;

        private GameObject destinationObject;

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
                // DontDestroyOnLoad(gameObject);
                Debug.Log("GridManager created and set to not destroy on load.");
            }
        }

        void Start()
        {
            SetupTilemapGridAlignment();

            bool hasMapGenerator = mapGenerator != null;
            if (hasMapGenerator)
            {
                width = mapGenerator.Width;
                height = mapGenerator.Height;
                numOfEnemySpawners = mapGenerator.NumOfEnemySpawners;
            }

            // FIX: Only initialize a NEW grid if we haven't already loaded one via GameManager
            if (grid == null)
            {
                grid = new Grid(width, height, cellSize);
                buildingsGrid = new GameObject[width, height];

                // Initialize array to avoid nulls
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    buildingsGrid[x, y] = null;

                // Only run generation logic if we created a new grid
                if (hasMapGenerator && !mapGenerated)
                {
                    Debug.Log("Map generation initializing");

                    GlobalSeed globalSeed = new GlobalSeed(QuickGenerate(1));

                    const int MaxFullRegenerations = 5;   // how many reseeded attempts allowed
                    bool success = false;

                    for (int attempt = 0; attempt < MaxFullRegenerations; attempt++)
                    {
                        Debug.Log($"--- Map Generation Attempt {attempt + 1}/{MaxFullRegenerations} ---");

                        if (TryGenerateMapWithFallback(globalSeed))
                        {
                            success = true;
                            break;
                        }

                        // fallback failed → try full regeneration with new subseed
                        Debug.LogWarning("Regenerating with new seed...");
                    }

                    if (!success)
                    {
                        Debug.LogError("All map generation attempts failed. Unable to produce valid map, something went catastrophically wrong.");
                        return;
                    }

                    // If we reach here, map is valid
                    spawnerPositions = mapGenerator.SelectSpawnerPositions(numOfEnemySpawners);
                    destpos = mapGenerator.GetDestinationPosition();
                    mapGenerated = true;
                }
                else if (!hasMapGenerator && tilemap != null)
                {
                    InitializeEmptyTilemap();
                }
                else if (debugFiller != null)
                {
                    debugFiller.Initialize(this);
                }
            }
            else
            {
                Debug.Log("GridManager started with existing (loaded) grid. Skipping initialization.");
            }

            if (!hasMapGenerator && tilemap != null)
            {
                // If no map generator, initialize with empty tiles
                InitializeEmptyTilemap();
            }

            // Initialize debug filler if assigned and not using mapGenerator
            else if (debugFiller != null)
            {
                debugFiller.Initialize(this);
            }

            SetStartPlayerPosition();
            SetDestination();
            SetSpawners();
            mapGenerator.CreateMapBounds();

            //Set Camera
        }

        private void SetupTilemapGridAlignment()
        {
            // Ensure the Tilemap's grid component matches our cell size
            if (gridComponent != null)
            {
                //gridComponent.cellSize = new Vector3(cellSize, cellSize, 0);
                //gridComponent.cellSize = new Vector3(1, 1, 0);
                Debug.Log($"Set Grid component cell size to: {cellSize}");
            }
            else if (tilemap != null)
            {
                gridComponent = tilemap.layoutGrid;
                if (gridComponent != null)
                {
                    //gridComponent.cellSize = new Vector3(cellSize, cellSize, 0);
                    //gridComponent.cellSize = new Vector3(1, 1, 0);
                    Debug.Log($"Set Grid component cell size to: {cellSize}");
                }
            }

            // Position the tilemap to align with grid coordinates
            if (tilemap != null)
            {
                tilemap.transform.position = Vector3.zero;
                transform.localScale = Vector3.one;
                Debug.Log("Reset Tilemap position to origin");
            }
            
            if (fogTilemap != null)
            {
                fogTilemap.transform.position = Vector3.zero;
                Debug.Log("Reset Fog Tilemap position to origin");
            }
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

        private void ApplyMapToGridWithTilemap(Grid.TileType[,] mapData)
        {
            bool hasFog = false;
            if (tilemap == null)
            {
                Debug.LogWarning("Tilemap is not assigned in GridManager!");
                return;
            }
            
            if (fogTilemap == null)
            {
                Debug.LogWarning("FogTilemap is not assigned in GridManager!");
            }
            else
            {
                hasFog = true;
                fogTilemap.ClearAllTiles();
            }

            tilemap.ClearAllTiles();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Grid.TileType type = mapData[x, y];
                    grid.SetTileType(x, y, type);

                    TileBase tileToPlace = GetTileBase(type);
                    if (tileToPlace != null)
                    {
                        Vector3Int tilePos = new Vector3Int(x, y, 0);
                        
                        if (hasFog && tileToPlace == nullTile)
                        {
                            fogTilemap.SetTile(tilePos, tileToPlace);
                            continue;
                        }

                        tilemap.SetTile(tilePos, tileToPlace);

                        // Debug positioning for first tile
                        if (x == 0 && y == 0)
                        {
                            Vector3 worldPos = tilemap.CellToWorld(tilePos);
                            Debug.Log($"Tile [0,0] grid position: {tilePos}, world position: {worldPos}");
                        }
                    }
                }
            }

            Debug.Log($"Tilemap populated with {width}x{height} tiles");
            Debug.Log($"Expected grid bounds: (0,0) to ({width},{height})");
            Debug.Log($"Expected world bounds: (0,0) to ({width * cellSize},{height * cellSize})");
        }

        private void InitializeEmptyTilemap()
        {
            if (tilemap == null) return;

            tilemap.ClearAllTiles();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid.SetTileType(x, y, Grid.TileType.EMPTY);
                    tilemap.SetTile(new Vector3Int(x, y, 0), emptyTile);
                }
            }
        }

        private TileBase GetTileBase(Grid.TileType type)
        {
            switch (type)
            {
                case Grid.TileType.EMPTY: return emptyTile;
                case Grid.TileType.WALL: return wallTile;
                case Grid.TileType.WATER: return waterTile;
                case Grid.TileType.DONT_EXISTS: return nullTile;
                default: return emptyTile;
            }
        }

        public void UpdateTileVisual(int x, int y, Grid.TileType tileType)
        {
            if (tilemap == null) return;

            TileBase tileToPlace = GetTileBase(tileType);
            if (tileToPlace != null)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), tileToPlace);
            }
        }

        // Convert world position to grid coordinates using Tilemap
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            return grid.GetXY(worldPosition);
        }

        // Convert grid coordinates to world position using Tilemap
        public Vector3 GridToWorld(int x, int y)
        {
            return grid.GetWorldPosition(x, y) + new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0);
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
                    Vector3Int tile = new Vector3Int(firstTile.x + x, firstTile.y + y, 0);
                    if (!IsTileOnGrid(tile))
                    {
                        Debug.Log($"Tile {tile} Out of Grid");
                        return false;
                    }

                    if (grid.GetTileType(tile.x, tile.y) != Templates.Grid.Grid.TileType.EMPTY)
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
                    grid.SetTileType(tile.x, tile.y, Grid.TileType.BUILDING);
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
            grid.SetTileType(worldPosition, tileType);
        }

        public Grid GetCurrentGrid()
        {
            return grid;
        }

        public void SetCurrentGrid(Grid g)
        {
            grid = g;

            // FIX: Sync dimensions and force initialization of the buildings array
            // This ensures ClearMap has something to work with immediately
            this.width = g.width;
            this.height = g.height;
            this.cellSize = g.cellSize; // Update cell size too if needed

            if (buildingsGrid == null || buildingsGrid.GetLength(0) != width || buildingsGrid.GetLength(1) != height)
            {
                buildingsGrid = new GameObject[width, height];
                // Wipe it to be safe
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    buildingsGrid[x, y] = null;
            }
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

        private void SetStartPlayerPosition()
        {
            Vector3 newPosition = mapGenerator.GetDestinationWorldPosition();
            Player.transform.position = newPosition;
        }

        private void SetDestination()
        {
            destinationObject = Instantiate(DestinationPrefab, mapGenerator.GetDestinationWorldPosition(),
                Quaternion.identity, gameObject.transform);
        }
        
        private bool TryGenerateMapWithFallback(GlobalSeed globalSeed)
        {
            const int MaxFallbackPasses = 4;     // how many times fallback widens
            const int CarveStep = 2;             // widen radius each pass

            //Generate the initial map
            Grid.TileType[,] mapData = mapGenerator.GenerateMap(globalSeed.NextSubSeed("MAP_MAIN"));
            ApplyMapToGridWithTilemap(mapData);

            mapGenerator.setGrid(grid);
            mapGenerator.BuildValidSpawnerCandidates();

            //If OK, done
            if (mapGenerator.ReachableCandidatesCount() > 0)
                return true;

            Debug.LogWarning("No reachable spawner candidates. Starting fallback recovery.");

            //Attempt local fallback recovery
            int radius = 2;
            for (int pass = 0; pass < MaxFallbackPasses; pass++)
            {
                Debug.Log($"Fallback pass #{pass + 1}, carving radius = {radius}");

                // Carve around destination
                mapGenerator.SpawnersFallback(radius);

                // Reapply modified map
                Grid.TileType[,] fallbackMap = mapGenerator.GetCurrentMap();
                ApplyMapToGridWithTilemap(fallbackMap);
                mapGenerator.setGrid(grid);

                // Recalculate candidates
                mapGenerator.BuildValidSpawnerCandidates();

                if (mapGenerator.ReachableCandidatesCount() > 0)
                {
                    Debug.Log("Fallback succeeded!");
                    return true;
                }

                radius += CarveStep;
            }

            Debug.LogError("Fallback failed. Map is unsalvageable.");
            return false; // fallback failed
        }


        private void SetSpawners()
        {
            foreach (Vector3Int pos in spawnerPositions)
            {
                Debug.Log($"SPAWNER POSITION {GridToWorld(pos.x, pos.y)} == {pos}");
                Instantiate(EnemySpawnerPrefab, GridToWorld(pos.x, pos.y), Quaternion.identity, SpawnerContainer.transform);
            }
        }

        public GameObject GetDestinationObject()
        {
            return  destinationObject;
        }

    public void PrintGridCell(Vector3 worldPosition)
        {
            Vector2Int position = grid.GetXY(worldPosition);
            if (position.x < 0 || position.y < 0 || position.x >= width || position.y >= height)
            {
                return;
            }
            string buildingInfo = "Building: " + buildingsGrid[position.x, position.y];
            grid.PrintGridCell(worldPosition, buildingInfo);
        }

        void OnValidate()
        {
            if (mainCamera == null)
            {
                Debug.LogWarning("Main Camera is not assigned", this);
            }
            if (tilemap == null)
            {
                Debug.LogWarning("Tilemap is not assigned", this);
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
                            Gizmos.color = new Color(1, 0, 0, 0.2f);
                            break;
                        case Grid.TileType.WATER:
                            Gizmos.color = new Color(0, 0, 1, 0.2f);
                            break;
                        case Grid.TileType.BUILDING:
                            Gizmos.color = new Color(1, 1, 0, 0.2f);
                            break;
                        case Grid.TileType.DONT_EXISTS:
                            Gizmos.color = new Color(128/255, 128/255, 128/255, 0.4f);
                            break;
                        default:
                            Gizmos.color = new Color(0, 1, 0, 0.2f); // EMPTY or unknown
                            break;
                    }


                    Vector3 center = new Vector3(
                        x * tileSize + half,
                        y * tileSize + half,
                        10f);

                    Gizmos.DrawCube(center, new Vector3(tileSize, tileSize, 0.1f));
                }
            }
            Vector3 destcentered = new Vector3(destpos.x * tileSize + half, destpos.y * tileSize + half, 0);
            Gizmos.color = new Color(1, 0f, 132f / 255f, 0.7f);
            Gizmos.DrawSphere(destcentered, 2.5f);

            // Draw spawners
            if (spawnerPositions != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f); // Orange

                foreach (var sp in spawnerPositions)
                {
                    Vector3 pos = new Vector3(
                        sp.x * tileSize + half,
                        sp.y * tileSize + half,
                        0
                    );

                    Gizmos.DrawSphere(pos, 2.4f);
                }
            }
        }


        public void ClearMap()
        {
            if (tilemap != null)
            {
                tilemap.ClearAllTiles();
            }
            
            if (fogTilemap != null)
            {
                fogTilemap.ClearAllTiles();
            }

            // FIX: Defensive check (though SetCurrentGrid should have fixed this)
            if (buildingsGrid == null) return;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (buildingsGrid[x, y] != null)
                    {
                        Destroy(buildingsGrid[x, y]);
                        buildingsGrid[x, y] = null;
                    }

                    // Reset Logic Grid (Prevent ghost buildings)
                    if (grid != null && grid.GetTileType(x, y) == TDPG.Templates.Grid.Grid.TileType.BUILDING)
                    {
                        grid.SetTileType(x, y, TDPG.Templates.Grid.Grid.TileType.EMPTY);
                    }
                }
            }
        }
    }
}
