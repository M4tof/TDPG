using System.Collections.Generic;
using UnityEngine;

public class EnemyCompendium : MonoBehaviour
{
    public static EnemyCompendium Instance { get; private set; }

    // This list is the "Source of Truth" for the Save System
    public List<Enemy> ActiveEnemies = new List<Enemy>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (!ActiveEnemies.Contains(enemy))
        {
            ActiveEnemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (ActiveEnemies.Contains(enemy))
        {
            ActiveEnemies.Remove(enemy);
        }
    }

    public List<EnemySaveData> GetSaveData()
    {
        var list = new List<EnemySaveData>();
        foreach (var enemy in ActiveEnemies)
        {
            if (enemy.CurrentHealth > 0)
            {
                list.Add(new EnemySaveData
                {
                    EnemyID = enemy.EnemyID,
                    Health = enemy.CurrentHealth,
                    Damage = enemy.CurrentDamage,
                    AttackSpeed = enemy.CurrentAttackSpeed,
                    Position = enemy.Position,
                    Ov = enemy.Overrides
                });
            }
        }
        return list;
    }

    public void LoadFromData(List<EnemySaveData> data)
    {
        // 1. Clear existing
        foreach (var enemy in ActiveEnemies)
        {
            // Destroy View/GameObject linked to this enemy? 
            // We need a reference to the View to destroy it, or clear the scene beforehand.
        }
        ActiveEnemies.Clear();

        // 2. Find Spawner
        var spawner = FindFirstObjectByType<EnemysSpawner>(); 
        
        if (spawner == null)
        {
            Debug.LogError("CRITICAL: EnemySpawner not found in scene! Cannot load enemies.");
            return;
        }

        // 3. Respawn
        if (data != null)
        {
            foreach (var save in data)
            {
                try 
                {
                    spawner.ForceSpawnEnemy(save);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to spawn saved enemy {save.EnemyID}: {e.Message}");
                }
            }
        }
    }
}