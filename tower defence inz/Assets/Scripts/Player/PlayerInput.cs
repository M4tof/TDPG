using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BasicProjectileSpawner))]
public class PlayerInput : MonoBehaviour
{
    
    [SerializeField] private float speed = 1;
    
    [Header("Required elements")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private BuildingMenu buildingMenu;
    
    private Rigidbody2D rb;
    private BasicProjectileSpawner projectileSpawner;
    
    private Vector3 moveDirection;
    private Vector3 mousePosition;
    private Vector3 projectileRotation;

    [SerializeField] private bool inMenu;
    
    //Initial
    void Start()
    {
        inMenu = false;
        rb = GetComponent<Rigidbody2D>();
        projectileSpawner = GetComponent<BasicProjectileSpawner>();
    }
    
    //Updates every frame
    void Update()
    {
        rb.linearVelocity = moveDirection * speed;
        RotateSprite();
    }

    //Set player direction based on input system
    public void onMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    //Shoot if player press shoot button
    public void onShoot(InputAction.CallbackContext context)
    {
        if (context.performed && !inMenu)
        {
            mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
            projectileSpawner.Shoot(transform.position, worldMousePosition);
        }
    }

    //When Player press building button it switch building Panel
    public void onBuilding(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (pauseMenu.GetMenuActive())
            {
                return;
            }
            buildingMenu.SwitchBuildingPanel();
            mainCamera.GetComponent<CameraController>().SetDynamicCameraMovement(!buildingMenu.GetIsActive());
            inMenu = !inMenu;
        }
    }
    
    public void onPause(InputAction.CallbackContext context)
    {
        
        if (context.performed)
        {
            if (buildingMenu.GetIsActive())
            {
                buildingMenu.CloseBuildingPanel();
                inMenu = false;
                return;
            }
            pauseMenu.SwitchMenu();
            inMenu = !inMenu;
        }

    }
    
    //Rotates player's sprite to correct direction
    private void RotateSprite()
    {
        if (moveDirection.x > 0)
        {
            spriteRenderer.flipX = false;
            return;
        }
        if (moveDirection.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }
    
    void OnValidate()
    {
        if (pauseMenu == null)
        {
            Debug.LogWarning("Pause Menu is not assigned", this);
        }
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera is not assigned", this);
        }
        if (buildingMenu == null)
        {
            Debug.LogWarning("Building Menu is not assigned", this);
        }
        
    }
}


