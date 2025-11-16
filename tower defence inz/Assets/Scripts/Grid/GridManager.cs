using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [Header("Required elements")]
    [SerializeField] private Camera mainCamera;
    
    [Header("Parameters")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 8;
    
    private Grid grid;

    void Start()
    {
        grid = new Grid(width, height,cellSize);
        mainCamera.GetComponent<CameraController>().SetStaticCameraPosition(new Vector2(cellSize*width/2,cellSize*height/2));
    }

    public void OnMouseClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector3 mousePosition;
            mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
            grid.PrintGridCell(worldMousePosition);
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

                if (grid.GetTileType(tile.x,tile.y) != Grid.TileType.EMPTY)
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
        TurretBase turretBase = turret.GetComponent<TurretBase>();
        if (turretBase == null)
        {
            return;
        }
        Vector2Int firstTile = grid.GetXY(worldPosition);
        Vector2 turretSize = turretBase.GetTileSize();
        //Validation
        if (!CanPlaceTurret(worldPosition, turretSize))
        {
            return;
        }
        //Placing turret on grid
        if (GameManager.Instance.RSInstance.mana.Claim(200))
        {
            for (int x = 0; x < turretSize.x; x++)
            {
                for (int y = 0; y < turretSize.y; y++)
                {
                    Vector2Int tile = new Vector2Int(firstTile.x + x, firstTile.y + y);
                    grid.SetBuilding(tile.x,tile.y,turret);
                    grid.SetTileType(tile.x,tile.y,Grid.TileType.BUILDING);
                }
            }
        }
    }
    
    void OnValidate()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera is not assigned", this);
        }
    }

    public Grid GetCurrentGrid()
    {
        return grid;
    }
}
