using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TDPG.Templates.Grid;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] Transform playerTransform;

    [Header("Dynamic camera position")] 
    [SerializeField] private bool allowDynamicMovement = true;
    [Header("Camera would stick to point")] 
    [SerializeField] private bool stickToPoint = false;
    [SerializeField] Vector2 displacementMultiplayer = new Vector2(0.15f,0.3f);
    
    [Header("Camera Point")] 
    [SerializeField] private Vector2 centerGrid;
    [SerializeField] private Vector2 staticCameraPosition;

    
    [Header("Zooming")]
    [SerializeField] private float normalZoom = 10f;
    [SerializeField] private float zoomOut = 40f;

    private bool cameraTransition = false;

    void Start()
    {
        normalZoom = Camera.main.orthographicSize;
        centerGrid = GridManager.Instance.GetCenterGrid();
    }
    
    void Update()
    {
        if (cameraTransition)
        {
            return;
        }
        if (allowDynamicMovement)
        {
            SetCameraPositionBasedOnMousePosition();
            return;
        }

        if (stickToPoint)
        {
            return;
        }
        SetInstanltyCameraPostionBasedOnPlayerPosition();
    }

    //Set Camera position based on attached transform;
    public void SetCameraPostionBasedOnPlayerPosition(float duration = 1.0f)
    {
        StartCoroutine(MoveToPlayerCoroutine(duration));
    }
    
    //Set Camera position based on attached transform;
    public void SetInstanltyCameraPostionBasedOnPlayerPosition(float duration = 1.0f)
    {
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -10f);
    }
    
    //Set Camera positon based diffrence between mouse position and attached transform (camera would move toward mouse position)
    void SetCameraPositionBasedOnMousePosition()
    {
        transform.position = GetPositionBasedOnMousePosition();
    }
    
    public Vector3 GetPositionBasedOnMousePosition()
    {
        Vector3 worldMousePosition  = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3 cameraDisplacement = (worldMousePosition - playerTransform.position) * displacementMultiplayer;

        Vector3 finalCameraPosition = playerTransform.position + cameraDisplacement;
        finalCameraPosition.z = -10f;
        return finalCameraPosition;
    }

    public void SetCameraPositionBasedOnCenterMap(float duration = 1.0f)
    {
        Debug.Log($"Center {centerGrid}");
        StartCoroutine(MoveCoroutine(new Vector3(centerGrid.x, centerGrid.y, -10f),duration));
    }
    
    public void SetCameraPosition(float duration = 1.0f)
    {
        StartCoroutine(MoveCoroutine(new Vector3(staticCameraPosition.x,staticCameraPosition.y, -10f),duration));
    }

    //Change mode of camera
    public void SetDynamicCameraMovement(bool active, bool smoothTransition = false)
    {
        allowDynamicMovement = active;
        if (allowDynamicMovement && smoothTransition)
        {
            SetCameraPostionBasedOnPlayerPosition();
        }
    }
    
    //Change mode of camera
    public void SetStaticCamera(bool active)
    {
        stickToPoint = active;
        if (stickToPoint)
        {
            SetCameraPositionBasedOnCenterMap();
        }
    }

    public void SetStaticCameraPosition(Vector2 position)
    {
        staticCameraPosition = position;
    }

    public void ZoomIn()
    {
        StartCoroutine(CameraSizeCoroutine(normalZoom, 1.0f));
    }
    
    public void ZoomOut()
    {
        StartCoroutine(CameraSizeCoroutine(zoomOut, 1.0f));
    }
    
    //Courutine 
    private IEnumerator CameraSizeCoroutine(float cameraSize, float duration)
    {
        float startSize = Camera.main.orthographicSize;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            Camera.main.orthographicSize = Mathf.Lerp(startSize, cameraSize, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.orthographicSize =  cameraSize;
    }
    
    //Smoothly changes camera position to target position (good for static points)
    private IEnumerator MoveCoroutine(Vector3 target, float duration)
    {
        cameraTransition = true;
        Vector3 startPosition = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, target, elapsed / duration);
            elapsed += Time.deltaTime;
            cameraTransition = false;
            yield return null;
        }
        
        transform.position = target;
    }
    
    //Smoothly changes camera position to point between player and mouse pointer
    private IEnumerator MoveToPlayerCoroutine(float duration)
    {
        Debug.Log("ACTIVE");
        cameraTransition = true;
        Vector3 target = GetPositionBasedOnMousePosition();
        float elapsed = 0f;
        Vector3 velocity = Vector3.zero;
        while (elapsed < duration)
        {
            target = GetPositionBasedOnMousePosition();
            transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, duration-elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        cameraTransition = false;
        transform.position = target;
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
