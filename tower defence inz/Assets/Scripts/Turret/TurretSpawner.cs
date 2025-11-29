using UnityEngine;
using TDPG.Templates.Grid;   // Lib
using TDPG.Templates.Turret; // Lib

public class TurretSpawner : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject TurretBox;
    [SerializeField] private TurretBase TurretVisualizer;
    
    private string _selectedTurretID;
    private bool _canSpawnTurret;

    // Renamed back to Set... for consistency
    public void SetTurretToSpawn(string turretID)
    {
        _selectedTurretID = turretID;
        
        if (string.IsNullOrEmpty(turretID))
        {
            TurretVisualizer.gameObject.SetActive(false);
            return;
        }

        // Access Registry (Game Side)
        TurretData data = TurretRegistry.Instance.Get(turretID);
        if (data == null)
        {
            Debug.LogError($"Cannot find turret data: {turretID}");
            TurretVisualizer.gameObject.SetActive(false);
            return;
        }

        // Setup Visualizer
        TurretVisualizer.gameObject.SetActive(true);
        TurretVisualizer.Initialize(data); 
    }

    public string GetTurretToSpawn()
    {
        return _selectedTurretID;
    }

    public GameObject SpawnTurret(Vector3 worldPosition)
    {
        if (string.IsNullOrEmpty(_selectedTurretID) || !_canSpawnTurret) return null;

        TurretData data = TurretRegistry.Instance.Get(_selectedTurretID);
        
        // Access ResourceSystem (Game Side) - No errors now!
        if (ResourceSystem.Instance != null)
        {
            if (!ResourceSystem.Instance.money.CanAfford(data.Cost))
            {
                Debug.Log("Not enough resources.");
                return null;
            }
            ResourceSystem.Instance.money.Claim(data.Cost);
        }

        // Instantiate Visualizer (Assuming Visualizer is the Generic Prefab)
        GameObject newTurret = Instantiate(TurretVisualizer.gameObject, gridManager.GetGridWorldTilePosition(worldPosition), Quaternion.identity);
        newTurret.transform.SetParent(TurretBox.transform);
        
        // Init Logic
        var logic = newTurret.GetComponent<TurretBase>();
        logic.Initialize(data);
        
        gridManager.PlaceTurret(worldPosition, newTurret);
        
        return newTurret;
    }

    public void UpdateVisualizerPosition(Vector3 position)
    {
        if (!string.IsNullOrEmpty(_selectedTurretID) && gridManager.IsOnGrid(position))
        {
            position.z = 0f;
            TurretVisualizer.transform.position = gridManager.GetGridWorldTilePosition(position);
            TurretVisualizer.gameObject.SetActive(true);

            if (gridManager.CanPlaceTurret(position, TurretVisualizer.GetTileSize()))
            {
                _canSpawnTurret = true;
                // Visual feedback (requires sprite renderer access)
                // logic...
            }
            else
            {
                _canSpawnTurret = false;
                // logic...
            }
            return;
        }
        TurretVisualizer.gameObject.SetActive(false);
    }
}