using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform playerTransform;
    [SerializeField] Vector2 displacementMultiplayer = new Vector2(0.15f,0.3f);

    void Update()
    {
        SetCameraPosition();
    }
    
    void SetCameraPosition()
    {
        Vector3 worldMousePosition  = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3 cameraDisplacement = (worldMousePosition - playerTransform.position) * displacementMultiplayer;

        Vector3 finalCameraPosition = playerTransform.position + cameraDisplacement;
        finalCameraPosition.z = -10f;
        transform.position = finalCameraPosition;
    }
    
    void OnValidate()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Player Transforom is not set", this);
        }
    }
}
