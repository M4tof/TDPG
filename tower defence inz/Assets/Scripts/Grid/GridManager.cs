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
    
    void OnValidate()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera is not assigned", this);
        }
    }
}
