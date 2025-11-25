using UnityEngine;

[System.Serializable]
public struct EnemyStatsOverride
{
    public float HealthMultiplier;
    public float SpeedMultiplier;
    
    public static EnemyStatsOverride Default => new EnemyStatsOverride { HealthMultiplier = 1f, SpeedMultiplier = 1f };
}