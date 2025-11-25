using UnityEngine;
using System.Collections.Generic;

public class EnemyRegistry : MonoBehaviour
{
    public static EnemyRegistry Instance { get; private set; }

    // Map "Goblin" -> EnemyData Asset
    private Dictionary<string, EnemyData> _lookup = new Dictionary<string, EnemyData>();

    void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            // If another GameManager already exists, destroy this one
            Destroy(gameObject);
            Debug.LogWarning("Duplicate EnemyRegistry destroyed. Only one instance allowed.");
        }
        else
        {
            // If this is the first GameManager, make it the instance
            Instance = this;
            // Prevents the GameObject from being destroyed when reloading a scene
            DontDestroyOnLoad(gameObject);
            Debug.Log("EnemyRegistry created and set to not destroy on load.");

            LoadAssets();
        }
    }

    private void LoadAssets()
    {
        // Load all EnemyData SOs from Resources/Enemies
        // TODO: fix
        var allData = Resources.LoadAll<EnemyData>("Enemies");
        foreach (var data in allData)
        {
            if (!_lookup.ContainsKey(data.EnemyName))
            {
                _lookup.Add(data.EnemyName, data);
            }
        }
    }

    public EnemyData GetEnemyData(string id)
    {
        if (_lookup.TryGetValue(id, out var data)) return data;
        Debug.LogError($"Enemy Data not found for ID: {id}");
        return null;
    }
}
