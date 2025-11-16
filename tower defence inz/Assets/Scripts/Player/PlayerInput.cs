using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BasicProjectileSpawner))]
[RequireComponent(typeof(TurretSpawner))]
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
    private TurretSpawner turretSpawner;
    
    private Vector3 moveDirection;
    private Vector3 mousePosition;
    private Vector3 projectileRotation;

    private bool inMenu;
    private bool inMap;
    private GameObject buildingToBuild; 
    
    //Initial
    void Start()
    {
        inMenu = false;
        rb = GetComponent<Rigidbody2D>();
        projectileSpawner = GetComponent<BasicProjectileSpawner>();
        turretSpawner = GetComponent<TurretSpawner>();
        inMap = false;
    }
    
    //Updates every frame
    void Update()
    {
        //Move player
        rb.linearVelocity = moveDirection * speed;
        RotateSprite();
        
        //Move building preview
        turretSpawner.UpdateVisualizerPosition(mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
    }
    
    //Set player direction based on input system
    public void onMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    //Shoot if player press shoot button
    public void onShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
            if (!inMenu)
            {
                projectileSpawner.Shoot(transform.position, worldMousePosition);
                return;
            }

            if (buildingMenu.GetIsActive() && turretSpawner.GetTurretToSpawn() != null)
            {
                turretSpawner.SpawnTurret(worldMousePosition);
            }
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
            Debug.Log("MMMMMMM");
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


