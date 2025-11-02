using UnityEngine;

public class DestLogic : MonoBehaviour
{
    [Header("References")]
    public GridHelper grid;
    
    private float timer = 0f;
    private float updateInterval = 0.8f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log($"DESTINATION IS AT {grid.destObj.position}");
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        
        
        if (timer >= updateInterval)
        {
            grid.changeDestPosition();
                
            // Reset timer
            timer = 0f;
        }
    }
    
    
}
