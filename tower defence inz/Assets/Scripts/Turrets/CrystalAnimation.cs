using UnityEngine;

public class CrystalAnimation : MonoBehaviour
{
    [SerializeField] private float amplitude = 1f; 
    [SerializeField] private float frequency = 1f;
    [SerializeField] private float speedMultiplier = 1f; 
    
    private Vector3 startPosition;
    private float timeOffset;
    
    void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2f); // Random Offset
    }
    
    void Update()
    {
        // Sinus Movement
        float newY = startPosition.y + Mathf.Sin((Time.time + timeOffset) * frequency) * amplitude;
        
        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );
    }
}
