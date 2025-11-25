using UnityEngine;

[System.Serializable]
public class Enemy : EnemyBase
{
    // Saved Data
    public string EnemyID; // "Goblin"
    public EnemyStatsOverride Overrides;

    // Runtime Only (Re-assigned on Load)
    [System.NonSerialized] private EnemyData _baseData;

    public Enemy(EnemyData baseData, EnemyStatsOverride overrides) : base(baseData)
    {
        _baseData = baseData;
        EnemyID = baseData.EnemyName;
        Overrides = overrides;
    
        // Initialize State
        CurrentHealth = baseData.MaxHealth * overrides.HealthMultiplier;
        CurrentSpeed = baseData.Speed * overrides.SpeedMultiplier;
        // Position set by Spawner...
    }

    // Called after loading from JSON to reconnect the SO
    public void RestoreReference(EnemyData data)
    {
        _baseData = data;
    }
}
