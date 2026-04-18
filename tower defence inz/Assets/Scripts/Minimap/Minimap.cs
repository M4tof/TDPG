using TDPG.Templates.Grid;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Minimap : MonoBehaviour
{
    
    [Tooltip("Size of minimap required to cover one tile when grid have 1x1")]
    [SerializeField] private float minimapTileSize;
    private GridManager gridManager;
    private Camera minimapCamera;
    
    void Start()
    {
        minimapCamera = GetComponent<Camera>();
        gridManager = GridManager.Instance;
        if (gridManager != null)
        {
            gridManager.SubscribeToEvent(SetCenterPositon);
        }
    }

    public void SetCenterPositon()
    {
        transform.position = gridManager.GetCenterGrid();
        minimapCamera.orthographicSize =  minimapTileSize * gridManager.GetCellSize() * Mathf.Max(gridManager.GetHeight(),gridManager.GetWidth());
        
    }
    
}
