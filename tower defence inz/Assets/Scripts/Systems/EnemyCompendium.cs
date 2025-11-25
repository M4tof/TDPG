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

    // Call this from your SaveSystem to get the data object
    public List<Enemy> GetSaveData()
    {
        return ActiveEnemies;
    }
}