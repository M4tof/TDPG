using UnityEngine;
using System.Collections.Generic;
using TDPG.Templates.Turret;

public class TurretRegistry
{
    private static TurretRegistry _instance;
    public static TurretRegistry Instance
    {
        get
        {
            if (_instance == null) { _instance = new TurretRegistry(); _instance.LoadRegistry(); }
            return _instance;
        }
    }

    private Dictionary<string, TurretData> _lookup;
    private TurretRegistry() { _lookup = new Dictionary<string, TurretData>(); }

    private void LoadRegistry()
    {
        var allTurrets = Resources.LoadAll<TurretData>("Turrets");

        foreach (var t in allTurrets)
        {
            string key = string.IsNullOrEmpty(t.TurretID) ? t.name : t.TurretID;
            if (!_lookup.ContainsKey(key))
            {
                _lookup.Add(key, t);
            }
        }
        Debug.Log($"[TurretRegistry] Loaded {_lookup.Count} turrets.");
    }

    public TurretData Get(string id)
    {
        if (_lookup.TryGetValue(id, out var data)) return data;
        Debug.LogError($"[TurretRegistry] Turret '{id}' not found!");
        return null;
    }
}
