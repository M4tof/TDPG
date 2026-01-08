using System.Collections.Generic;
using UnityEngine;
using TDPG.Templates.Grid;   // Lib
using TDPG.Templates.Turret; // Lib
using TDPG.AudioModulation;

public class TurretSpawner : MonoBehaviour
{
    // [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject TurretBox;

    [Header("References")]
    [Tooltip("The Scene Object used for previewing placement")]
    [SerializeField] private TurretPreview TurretVisualizer;

    [Tooltip("The Clean Prefab used for spawning real turrets")]
    [SerializeField] private GameObject GenericTurretPrefab;

    private string _selectedTurretID;
    private bool _canSpawnTurret;
    private TurretData data;
    private List<CardData> modifiersList = new List<CardData>();

    // Renamed back to Set... for consistency
    public void SetTurretToSpawn(string turretID, List<CardData> modifiers = null)
    {
        _selectedTurretID = turretID;

        if (string.IsNullOrEmpty(turretID))
        {
            if (TurretVisualizer != null) TurretVisualizer.gameObject.SetActive(false);
            return;
        }

        // Access Registry (Game Side)
        data = TurretRegistry.Instance.Get(turretID);
        if (data == null)
        {
            Debug.LogError($"Cannot find turret data: {turretID}");
            TurretVisualizer.gameObject.SetActive(false);
            modifiersList = new List<CardData>();
            return;
        }
        
        //Set Modifiers
        modifiersList = (modifiers == null || modifiers.Count == 0) ?  new List<CardData>() : modifiers;

        // Setup Visualizer
        TurretVisualizer.gameObject.SetActive(true);
        TurretVisualizer.Initialize(data);
    }

    public string GetTurretToSpawn()
    {
        return _selectedTurretID;
    }

    private Vector3 CalculateTurretPosition(Vector3 mousePos, TurretData data)
    {
        // 1. Get Anchor (Bottom-Left of the tile under mouse)
        Vector3 gridBottomLeft = GridManager.Instance.GetGridWorldTilePosition(mousePos);

        // 2. Calculate Center Offset based on Data size
        float cellSize = GridManager.Instance.CellSize;
        Vector3 centerOffset = new Vector3(
            data.TileSize.x * cellSize * 0.5f,
            data.TileSize.y * cellSize * 0.5f,
            0f
        );

        // 3. Return Center
        return gridBottomLeft + centerOffset;
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

        Vector3 spawnPos = CalculateTurretPosition(worldPosition, data);

        GameObject newTurret = Instantiate(GenericTurretPrefab, spawnPos, Quaternion.identity);
        newTurret.transform.SetParent(TurretBox.transform);

        // Initialize the new instance (It will calculate offset from its clean state)
        var logic = newTurret.GetComponent<Turret>();
        logic.Initialize(data);
        logic.ApplyModifiers(modifiersList);

        GridManager.Instance.PlaceTurret(worldPosition, newTurret);

        UpdateVisualizerPosition(worldPosition);
        return newTurret;
    }

    public void UpdateVisualizerPosition(Vector3 mousePosition)
    {
        if (!string.IsNullOrEmpty(_selectedTurretID) && GridManager.Instance.IsOnGrid(mousePosition))
        {
            Vector3 centerPos = CalculateTurretPosition(mousePosition, data);
            TurretVisualizer.transform.position = centerPos;
            TurretVisualizer.gameObject.SetActive(true);

            if (GridManager.Instance.CanPlaceTurret(mousePosition, data.TileSize))
            {
                _canSpawnTurret = true;
                TurretVisualizer.SetPlacementValid(true);
                // Visual feedback (requires sprite renderer access)
                // logic...
            }
            else
            {
                _canSpawnTurret = false;
                TurretVisualizer.SetPlacementValid(false);
                // logic...
            }
            return;
        }
        TurretVisualizer.gameObject.SetActive(false);
    }

    public void ForceSpawnTurret(string turretID, Vector3 worldPosition)
    {
        TurretData data = TurretRegistry.Instance.Get(turretID);
        if (data == null)
        {
            Debug.LogError($"[Load] Unknown Turret ID: {turretID}");
            return;
        }

        // Instantiate
        GameObject newTurret = Instantiate(GenericTurretPrefab, worldPosition, Quaternion.identity);
        
        var ac = newTurret.GetComponentInChildren<ProceduralAudioController>();
        ac.selectionSeed = GameManager.Instance.ACSeed1.GetBaseValue();
        ac.modulationSeed = GameManager.Instance.ACSeed2.GetBaseValue();
        
        newTurret.SetActive(true);
        newTurret.transform.SetParent(TurretBox.transform);

        // Init Logic
        newTurret.GetComponent<TurretBase>().Initialize(data);
        newTurret.GetComponent<TurretBase>().ApplyModifiers(modifiersList);

        // Register in Grid
        GridManager.Instance.PlaceTurret(worldPosition, newTurret);
    }
}