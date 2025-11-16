using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform playerTransform;

    [Header("Dynamic camera position")] 
    [SerializeField] private bool allowDynamicMovement = true;
    [Header("Camera would stick to point")] 
    [SerializeField] private bool stickToPoint = false;
    [SerializeField] Vector2 displacementMultiplayer = new Vector2(0.15f,0.3f);
    
    [Header("Camera Point")] 
    [SerializeField] private Vector2 staticCameraPosition;
    
    [Header("Zooming")]
    [SerializeField] private float normalZoom = 10f;
    [SerializeField] private float zoomOut = 40f;

    void Start()
    {
        normalZoom = Camera.main.orthographicSize;
    }
    
    void Update()
    {
        if (allowDynamicMovement)
        {
            SetCameraPositionBasedOnMousePosition();
            return;
        }

        if (stickToPoint)
        {
            SetCameraPositionBasedOnCenterMap();
            return;
        }
        SetCameraPostionBasedOnPlayerPosition();
    }

    //Set Camera position based on attached transform;
    public void SetCameraPostionBasedOnPlayerPosition()
    {
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -10f);
    }
    //Set Camera positon based diffrence between mouse position and attached transform (camera would move toward mouse position)
    void SetCameraPositionBasedOnMousePosition()
    {
        Vector3 worldMousePosition  = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3 cameraDisplacement = (worldMousePosition - playerTransform.position) * displacementMultiplayer;

        Vector3 finalCameraPosition = playerTransform.position + cameraDisplacement;
        finalCameraPosition.z = -10f;
        transform.position = finalCameraPosition;
    }

    public void SetCameraPositionBasedOnCenterMap()
    {
        transform.position = new Vector3(staticCameraPosition.x, staticCameraPosition.y, -10f);
    }

    //Change mode of camera
    public void SetDynamicCameraMovement(bool active)
    {
        allowDynamicMovement = active;
    }
    
    //Change mode of camera
    public void SetStaticCamera(bool active)
    {
        stickToPoint = active;
    }

    public void SetStaticCameraPosition(Vector2 position)
    {
        staticCameraPosition = position;
    }

    public void ZoomIn()
    {
        Camera.main.orthographicSize = normalZoom;
    }
    
    public void ZoomOut()
    {
        Camera.main.orthographicSize = zoomOut;
    }
    
    //Validation
    void OnValidate()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Player Transforom is not set", this);
        }
    }
}
