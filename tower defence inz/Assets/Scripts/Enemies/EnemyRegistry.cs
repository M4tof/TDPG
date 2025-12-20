using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TDPG.Templates.Enemies;

public class EnemyRegistry
{
    // --- Singleton Pattern (Lazy Loaded) ---
    private static EnemyRegistry _instance;
    public static EnemyRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EnemyRegistry();
                _instance.LoadRegistry();
            }
            return _instance;
        }
    }

    // --- Logic ---
    private Dictionary<string, EnemyData> _lookup;

    // Private constructor ensures only 'Instance' can create this
    private EnemyRegistry()
    {
        _lookup = new Dictionary<string, EnemyData>();
    }

    private void LoadRegistry()
    {
        // Still uses Unity API, so this file stays in your Unity Project (not the Lib)
        var allEnemies = Resources.LoadAll<EnemyData>("Enemies");

        foreach (var enemy in allEnemies)
        {
            // Prefer an explicit ID field if you have one, fallback to file name
            string key = enemy.EnemyName;

            if (!_lookup.ContainsKey(key))
            {
                _lookup.Add(key, enemy);
            }
            else
            {
                Debug.LogWarning($"[EnemyRegistry] Duplicate Enemy ID found: {key}. Ignoring {enemy.name}.");
            }
        }

        Debug.Log($"[EnemyRegistry] Initialized. Loaded {_lookup.Count} templates.");
    }

    public EnemyData Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (_lookup.TryGetValue(id, out var data))
        {
            return data;
        }

        Debug.LogError($"[EnemyRegistry] Enemy ID '{id}' not found!");
        return null;
    }
}