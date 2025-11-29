using UnityEngine;
using TDPG.Templates.Grid;   // Lib
using TDPG.Templates.Turret; // Lib

public class TurretSpawner : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject TurretBox;

    [Header("References")]
    [Tooltip("The Scene Object used for previewing placement")]
    [SerializeField] private TurretBase TurretVisualizer;

    [Tooltip("The Clean Prefab used for spawning real turrets")]
    [SerializeField] private GameObject GenericTurretPrefab;

    private string _selectedTurretID;
    private bool _canSpawnTurret;

    // Renamed back to Set... for consistency
    public void SetTurretToSpawn(string turretID)
    {
        Debug.Log("SetTurretToSpawn");
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
        if (string.IsNullOrEmpty(_selectedTurretID) || !_canSpawnTurret) { Debug.Log("ID not set"); return null; }

        TurretData data = TurretRegistry.Instance.Get(_selectedTurretID);

        // Access ResourceSystem (Game Side) - No errors now!
        if (ResourceSystem.Instance != null)
        {
            if (!ResourceSystem.Instance.mana.CanAfford(data.Cost))
            {
                Debug.Log("Not enough resources.");
                return null;
            }
            ResourceSystem.Instance.mana.Claim(data.Cost);
        }

        Vector3 gridBottomLeft = gridManager.GetGridWorldTilePosition(worldPosition);

        // 2. Calculate Center Offset
        // (TileDimensions * CellSize) / 2
        float cellSize = gridManager.CellSize;
        Vector3 centerOffset = new Vector3(
            data.TileSize.x * cellSize * 0.5f,
            data.TileSize.y * cellSize * 0.5f,
            0f
        );

        // 3. Instantiate at CENTER
        Vector3 spawnPos = gridBottomLeft + centerOffset;

        GameObject newTurret = Instantiate(GenericTurretPrefab, spawnPos, Quaternion.identity);
        newTurret.transform.SetParent(TurretBox.transform);

        // Initialize the new instance (It will calculate offset from its clean state)
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