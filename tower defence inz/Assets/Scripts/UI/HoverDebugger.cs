using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // <--- Required
using System.Collections.Generic;

public class HoverDebugger : MonoBehaviour
{
    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        // Check for Spacebar using New Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // Get Mouse Position using New Input System
            Vector2 mousePos = Mouse.current.position.ReadValue();
            
            Debug.Log($"--- PROBING MOUSE POSITION {mousePos} ---");
            ProbeUI(mousePos);
            ProbePhysics(mousePos);
            Debug.Log("----------------------------------------------------");
        }
    }

    private void ProbeUI(Vector2 screenPos)
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("No EventSystem found in scene.");
            return;
        }

        // Create pointer event data manually
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPos
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            foreach (var result in results)
            {
                Debug.Log($"<color=orange>[UI BLOCKER]</color> Object: <b>{result.gameObject.name}</b> | Depth: {result.depth} | SortingLayer: {result.sortingLayer}");
            }
        }
        else
        {
            Debug.Log("<color=green>[UI]</color> Clear. No UI elements under mouse.");
        }
    }

    private void ProbePhysics(Vector2 screenPos)
    {
        Vector2 worldPoint = _cam.ScreenToWorldPoint(screenPos);
        
        // Raycast against 2D Colliders
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector2.zero);

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
                string isTrigger = hit.collider.isTrigger ? "(Trigger)" : "(Solid)";
                
                Debug.Log($"<color=cyan>[PHYSICS HIT]</color> Object: <b>{hit.collider.gameObject.name}</b> | Layer: {layerName} | Type: {hit.collider.GetType().Name} {isTrigger}");
            }
        }
        else
        {
            Debug.Log($"<color=red>[PHYSICS]</color> No Colliders found at World Pos: {worldPoint}");
        }
    }
}