using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform playerTransform;

    [Header("Dynamic camera position")] [SerializeField]
    private bool allowDynamicMovement = true;
    [SerializeField] Vector2 displacementMultiplayer = new Vector2(0.15f,0.3f);

    void Update()
    {
        if (allowDynamicMovement)
        {
            SetCameraPositionBasedOnMousePosition();
            return;
        }
        SetCameraPostionBasedOnPlayerPosition();
    }

    //Set Camera position based on attached transform;
    void SetCameraPostionBasedOnPlayerPosition()
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

    //Change mode of camera
    public void SetDynamicCameraMovement(bool active)
    {
        allowDynamicMovement = active;
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
