using System.Collections.Generic;
using UnityEngine;
using TDPG.Templates.Grid; 
using TDPG.Templates.Turret;
using TDPG.AudioModulation;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TurretSpawner : MonoBehaviour
{
    [SerializeField] private GameObject TurretBox;

    [Header("References")]
    [Tooltip("The Scene Object used for previewing placement")]
    [SerializeField] private TurretPreview TurretVisualizer;

    [Tooltip("The Clean Prefab used for spawning real turrets")]
    [SerializeField] private GameObject GenericTurretPrefab;

    private string _selectedTurretID;
    private bool _canSpawnTurret;
    private TurretData data;
    public List<CardData> modifiersList = new List<CardData>();
    private bool blockSpawnTurret = false;

    public void SetTurretToSpawn(string turretID, List<CardData> modifiers = null)
    {
        _selectedTurretID = turretID;

        if (string.IsNullOrEmpty(turretID))
        {
            if (TurretVisualizer != null) TurretVisualizer.gameObject.SetActive(false);
            return;
        }

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
        Debug.Log("Turret Visualizer");
        TurretVisualizer.Initialize(data);
    }

    public string GetTurretToSpawn()
    {
        return _selectedTurretID;
    }

    private Vector3 CalculateTurretPosition(Vector3 mousePos)
    {
        //Get Anchor (Bottom-Left of the tile under mouse)
        Vector3 gridBottomLeft = GridManager.Instance.GetGridWorldTilePosition(mousePos);
        
        return gridBottomLeft;
    }

    public GameObject SpawnTurret(Vector3 worldPosition)
    {
        if (blockSpawnTurret)
        {
            Debug.Log("Spawning Blocked!!!");
            return null;
        }
        if (string.IsNullOrEmpty(_selectedTurretID) || !_canSpawnTurret) { Debug.Log("ID not set"); return null; }
        if (IsMouseOverUIToIgnore()) { 
            Debug.Log("Mouse is over UI");
            return null;
        }

        TurretData data = TurretRegistry.Instance.Get(_selectedTurretID);

        // Access ResourceSystem
        if (ResourceSystem.Instance != null)
        {
            if (!ResourceSystem.Instance.mana.CanAfford(data.Cost))
            {
                Debug.Log("Not enough resources.");
                return null;
            }
            ResourceSystem.Instance.mana.Claim(data.Cost);
        }

        Vector3 spawnPos = CalculateTurretPosition(worldPosition);

        GameObject newTurret = Instantiate(GenericTurretPrefab, spawnPos, Quaternion.identity);
        newTurret.transform.SetParent(TurretBox.transform);

        var logic = newTurret.GetComponent<Turret>();
        logic.Initialize(data);
        logic.ApplyModifiers(modifiersList);

        GridManager.Instance.PlaceTurret(worldPosition, newTurret);

        UpdateVisualizerPosition(worldPosition);
        return newTurret;
    }

    public void UpdateVisualizerPosition(Vector3 mousePosition)
    {
        if (!string.IsNullOrEmpty(_selectedTurretID) && GridManager.Instance.IsOnGrid(mousePosition) && !IsMouseOverUIToIgnore() && !blockSpawnTurret)
        {
            Vector3 centerPos = CalculateTurretPosition(mousePosition);
            TurretVisualizer.transform.position = centerPos;
            TurretVisualizer.gameObject.SetActive(true);

            if (GridManager.Instance.CanPlaceTurret(mousePosition, data.TileSize))
            {
                _canSpawnTurret = true;
                TurretVisualizer.SetPlacementValid(true);
            }
            else
            {
                _canSpawnTurret = false;
                TurretVisualizer.SetPlacementValid(false);
            }
            return;
        }
        TurretVisualizer.gameObject.SetActive(false);
    }

    public void ForceSpawnTurret(string turretID, Vector3 worldPosition)
    {
        Debug.Log($"[ForceSpawnTurret] {turretID}, {worldPosition}, {GenericTurretPrefab}");
        // Validate Dependencies
        if (GenericTurretPrefab == null)
        {
            Debug.LogError("[TurretSpawner] CRITICAL: GenericTurretPrefab is NULL! Assign it in the Inspector.");
            return;
        }
        if (TurretBox == null)
        {
            Debug.LogError("[TurretSpawner] TurretBox is NULL! Assign it in the Inspector.");
        }

        // Look up Data
        TurretData data = TurretRegistry.Instance.Get(turretID);
        if (data == null)
        {
            Debug.LogError($"[TurretSpawner] Unknown Turret ID: {turretID}");
            return;
        }

        // Instantiate
        GameObject newTurret = Instantiate(GenericTurretPrefab, worldPosition, Quaternion.identity);
        
        newTurret.SetActive(true);
        newTurret.transform.SetParent(TurretBox.transform);

        // Init Logic
        newTurret.GetComponent<TurretBase>().Initialize(data);
        newTurret.GetComponent<TurretBase>().ApplyModifiers(modifiersList);

        // Register in Grid
        GridManager.Instance.PlaceTurret(worldPosition, newTurret);
    }

    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    
    private bool IsMouseOverUIToIgnore()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = mousePosition;

        List<RaycastResult> raycastList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastList);
        for (int i = 0; i <raycastList.Count; i++)
        {
            if (raycastList[i].gameObject.GetComponent<MouseIgnore>() != null)
            {
                raycastList.RemoveAt(i);
                i -= 1;
            }
        }
        return  raycastList.Count > 0;
    }

    public void SetBlockSpawnTurret(bool block)
    {
        blockSpawnTurret = block;
    }
}