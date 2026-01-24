using TDPG.Templates.Grid;
using UnityEngine;
using UnityEngine.InputSystem;
using Grid = TDPG.Templates.Grid.Grid;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(ProjectileSpawner))]
[RequireComponent(typeof(TurretSpawner))]
public class PlayerInput : MonoBehaviour
{
    
    [SerializeField] private float speed = 1;
    
    [Header("Required elements")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private BuildingMenu buildingMenu;

    [Header("Terrain movement modifiers")]
    [SerializeField] private float waterSpeedMultiplier = 0.4f;
    [SerializeField] private float wallSpeedMultiplier = 0.3f;
    [SerializeField] private float defaultSpeedMultiplier = 1.0f;
    [SerializeField] private float buildingSpeedMultiplier = 0.8f;

    private Rigidbody2D rb;
    private ProjectileSpawner projectileSpawner;
    private TurretSpawner turretSpawner;
    
    private Vector3 moveDirection;
    private Vector3 mousePosition;
    private Vector3 projectileRotation;

    private bool inMenu;
    private bool inMap;
    private bool isShooting = false;
    private GameObject buildingToBuild; 
    
    //Initial
    void Start()
    {
        inMenu = false;
        rb = GetComponent<Rigidbody2D>();
        projectileSpawner = GetComponent<ProjectileSpawner>();
        turretSpawner = GetComponent<TurretSpawner>();
        inMap = false;
    }
    
    //Updates every frame
    void Update()
    {
        float speedMultiplier = defaultSpeedMultiplier;
        if(GridManager.Instance != null)
        {
            Grid.TileType tileType = GridManager.Instance.GetCurrentGrid().GetTileType(transform.position);
            switch (tileType)
            {
                case Grid.TileType.WATER:
                    speedMultiplier = waterSpeedMultiplier;
                    break;
                case Grid.TileType.WALL:
                    speedMultiplier = wallSpeedMultiplier;
                    break;
                case Grid.TileType.BUILDING:
                    speedMultiplier = buildingSpeedMultiplier;
                    break;
                default:
                    break;
            }
        }

        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        if (isShooting)
        {
            if (!inMenu)
            {
                projectileSpawner.Shoot(transform.position, worldMousePosition);
            }
        }
        //Move player
        rb.linearVelocity = moveDirection * speed * speedMultiplier;
        RotateSprite();
        
        //Move building preview
        turretSpawner.UpdateVisualizerPosition(worldMousePosition);
    }
    
    //Set player direction based on input system
    public void onMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    //Shoot if player presses shoot button
    public void onShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
            GridManager.Instance.OnMouseClick(context);
            if (!inMenu)
            {
                return;
            }

            if (buildingMenu.GetIsActive() && turretSpawner.GetTurretToSpawn() != null)
            {
                turretSpawner.SpawnTurret(worldMousePosition);
            }
        }

        if (context.started)
        {
            if (!inMenu)
            {
                isShooting = true;
            }
        }

        if (context.canceled)
        {
            isShooting = false;
        }
        
    }

    //When Player presses building button - switch building Panel
    public void onBuilding(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (pauseMenu.GetMenuActive())
            {
                return;
            }
            buildingMenu.SwitchBuildingPanel();
            mainCamera.GetComponent<CameraController>().SetDynamicCameraMovement(!buildingMenu.GetIsActive(),false);
            inMenu = !inMenu;
        }
    }
    
    public void onPause(InputAction.CallbackContext context)
    {
        
        if (context.performed)
        {
            if (buildingMenu.GetIsActive())
            {
                if (turretSpawner.GetTurretToSpawn() != null)
                {
                    turretSpawner.SetTurretToSpawn(null);
                    return;
                }
                buildingMenu.CloseBuildingPanel();
                inMenu = false;
                return;
            }
            pauseMenu.SwitchMenu();
            inMenu = !inMenu;
        }

    }
    
    public void onMap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (pauseMenu.GetMenuActive())
            {
                return;
            }
            CameraController cameraController = mainCamera.GetComponent<CameraController>();
            if (cameraController == null)
            {
                return;
            }
            if (inMap)
            {
                cameraController.SetStaticCamera(false);
                if (!buildingMenu.GetIsActive())
                {
                    mainCamera.GetComponent<CameraController>().SetDynamicCameraMovement(true,true);
                    cameraController.ZoomIn();
                }

                inMap = false;
                return;
            }
            cameraController.SetStaticCamera(true);
            cameraController.SetDynamicCameraMovement(false);
            cameraController.ZoomOut();
            inMap = true;
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


