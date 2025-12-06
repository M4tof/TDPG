using TDPG.Templates.Grid;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    private GridManager gridManager;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridManager = GridManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
