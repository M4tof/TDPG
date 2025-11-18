using System.Collections;
using System.Collections.Generic;
using TDPG.Templates.Pathfinding;
using UnityEngine;
using static TDPG.Templates.Grid.Grid;

namespace TDPG.Templates.Grid
{
    public class GridDebugFiller : MonoBehaviour
    {
        [Header("Spawn Chances (0–1)")] [Range(0f, 1f)]
        public float wallChance = 0.15f;

        [Range(0f, 1f)] public float waterChance = 0.10f;
        [Range(0f, 1f)] public float buildingChance = 0.05f;
        
        public GameObject destinationObject;
        public GameObject[] enemyObjects;
        
        private GridManager gridManager;
        private Grid grid;
        private TileType[,] cachedTiles;
        private int width;
        private int height;

        public void Initialize(GridManager gridManager)
        {
            this.gridManager = gridManager;
            this.grid = gridManager.GetGrid();
            this.width = gridManager.GetWidth();
            this.height = gridManager.GetHeight();
            this.cachedTiles = new TileType[width, height];

            FillGridRandomly();
            
            // Subscribe to grid change events
            PathfindingEvents.OnGridChanged += OnGridChanged;
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            PathfindingEvents.OnGridChanged -= OnGridChanged;
        }
        
        private void OnGridChanged()
        {
            // Update the cached tiles when grid changes
            UpdateCachedTiles();
        }
        
        private void UpdateCachedTiles()
        {
            if (grid == null) return;
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cachedTiles[x, y] = grid.GetTileType(x, y);
                }
            }
        }
        
        private HashSet<Vector2Int> GetProtectedTiles()
        {
            HashSet<Vector2Int> protectedTiles = new HashSet<Vector2Int>();

            if (destinationObject != null)
            {
                Vector2Int destCell = grid.GetXY(destinationObject.transform.position);
                protectedTiles.Add(destCell);
            }

            if (enemyObjects != null)
            {
                foreach (var enemy in enemyObjects)
                {
                    if (enemy == null) continue;
                    Vector2Int enemyCell = grid.GetXY(enemy.transform.position);
                    protectedTiles.Add(enemyCell);
                }
            }

            return protectedTiles;
        }
        
        private void FillGridRandomly()
        {
            HashSet<Vector2Int> protectedTiles = GetProtectedTiles();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int currentTile = new Vector2Int(x, y);

                    // Skip protected tiles
                    if (protectedTiles.Contains(currentTile))
                    {
                        cachedTiles[x, y] = TileType.EMPTY;
                        grid.SetTileType(x, y, TileType.EMPTY);
                        continue;
                    }

                    TileType current = grid.GetTileType(x, y);

                    if (current == TileType.BUILDING)
                    {
                        cachedTiles[x, y] = current;
                        continue;
                    }

                    float roll = Random.value;

                    TileType result = TileType.EMPTY;

                    if (roll < wallChance)
                        result = TileType.WALL;
                    else if (roll < wallChance + waterChance)
                        result = TileType.WATER;
                    else if (roll < wallChance + waterChance + buildingChance)
                        result = TileType.BUILDING;

                    grid.SetTileType(x, y, result);
                    cachedTiles[x, y] = result;
                }
            }
        }

        void OnDrawGizmos()
        {
            if (gridManager == null || cachedTiles == null)
                return;

            float size = gridManager.CellSize * 0.35f;
            float half = gridManager.CellSize * 0.5f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileType tile = cachedTiles[x, y];
                    Vector3 center = new Vector3(
                        x * gridManager.CellSize + half,
                        y * gridManager.CellSize + half,
                        3f
                    );

                    Gizmos.color = tile switch
                    {
                        TileType.WALL => Color.red,
                        TileType.WATER => Color.blue,
                        TileType.BUILDING => Color.yellow,
                        _ => Color.magenta
                    };

                    if (tile != TileType.EMPTY)
                    {
                        Gizmos.DrawLine(center + new Vector3(-size, -size, 0), center + new Vector3(size, size, 0));
                        Gizmos.DrawLine(center + new Vector3(-size, size, 0), center + new Vector3(size, -size, 0));
                    }
                }
            }
        }
    }
}