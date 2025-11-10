using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurretSpawner : MonoBehaviour
{
    [Tooltip("grid based on where turret would spawn")]
    [SerializeField] private GridManager gridManager;
    
    [Tooltip("GameObject in which turret would be spawned")]
    [SerializeField] GameObject TurretBox;
    
    [Tooltip("It would show selected turret on grid")]
    [SerializeField] TurretBase TurretVisualizer;
    
    private GameObject turretToSpawn;
    private bool canSpawnTurret = true;

    //Set Turret to Spawn
    public void SetTurretToSpawn(GameObject turretToSpawn)
    {
        //Set turret
        this.turretToSpawn = turretToSpawn;
        if (turretToSpawn == null)
        {
            TurretVisualizer.gameObject.SetActive(false);
            return;
        }
        //Set copy parameters from turret to spawn to preview
        TurretVisualizer.gameObject.SetActive(true);
        TurretBase turret = this.turretToSpawn.GetComponent<TurretBase>();
        if (turret == null)
        {
            TurretVisualizer.gameObject.SetActive(false);
            return;
        }
        TurretVisualizer.SetTileSize(turret.GetTileSize());
        TurretVisualizer.SetMultiplayer(turret.GetMultiplayer());
        TurretVisualizer.SetSprite(turret.GetSprite());
    }

    //Return turret to Spawn
    public GameObject GetTurretToSpawn()
    {
        return turretToSpawn;
    }

    //Spawn turret on given position
    public GameObject SpawnTurret(Vector3 worldPosition)
    {
        if (turretToSpawn != null && canSpawnTurret)
        {
            worldPosition.z = 0f;
            GameObject newTurret = Instantiate(turretToSpawn,gridManager.GetGridWorldTilePosition(worldPosition),Quaternion.identity);
            newTurret.transform.SetParent(TurretBox.transform);
            gridManager.PlaceTurret(worldPosition, newTurret);
            SetTurretToSpawn(null);
            return newTurret;
        }
        Debug.LogWarning("Turret is not set");
        return null;
    }

    public void UpdateVisualizerPosition(Vector3 position)
    {
        if (turretToSpawn != null && gridManager.IsOnGrid(position))
        {
            position.z = 0f;
            TurretVisualizer.transform.position = gridManager.GetGridWorldTilePosition(position);
            TurretVisualizer.gameObject.SetActive(true);
            if (gridManager.CanPlaceTurret(position,TurretVisualizer.GetTileSize()))
            {
                TurretVisualizer.GetSpriteRenderer().color = Color.blue;
                canSpawnTurret = true;
                return;
            }
            TurretVisualizer.GetSpriteRenderer().color = Color.red; 
            canSpawnTurret =  false;
            return;
            
        }
        TurretVisualizer.gameObject.SetActive(false);
        
    }

    //Validation
    void OnValidate()
    {
        if (TurretBox == null)
        {
            Debug.LogWarning("Pause Menu is not assigned", this);
        }
        if (gridManager == null)
        {
            Debug.LogWarning("Grid Manager is not assigned", this);
        }

        if (TurretVisualizer == null)
        {
            Debug.LogWarning("Turret visualizer is not assigned", this);
        }
    }
}
