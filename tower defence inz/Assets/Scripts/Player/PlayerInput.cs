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
    
    private Rigidbody2D rb;
    private BasicProjectileSpawner projectileSpawner;
    
    private Vector3 moveDirection;
    private Vector3 mousePosition;
    private Vector3 projectileRotation;
    
    //Initial
    void Start()
    {
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
        if (context.performed)
        {
            mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
            projectileSpawner.Shoot(transform.position, worldMousePosition);
        }
    }

    public void onPause(InputAction.CallbackContext context)
    {
        //Debug.Log("PAUSZA");
        if (context.performed)
        {
            pauseMenu.SwitchMenu();
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
    
}
