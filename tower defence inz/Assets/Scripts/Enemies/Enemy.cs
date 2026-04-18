using System.Collections.Generic;
using UnityEngine;
using TDPG.Templates.Enemies;
using System;

[System.Serializable]
public class Enemy : EnemyBase
{
    // Saved Data
    public string EnemyID;
    public EnemyStatsOverride Overrides;

    // Runtime Only (Re-assigned on Load)
    [System.NonSerialized] public EnemyData _baseData;

    // Movement State
    private Vector2? _currentTarget;

    public Vector2? CurrentTarget => _currentTarget;
#nullable enable
    public String? GeneratedName;
    
    public Enemy(EnemyData baseData, EnemyStatsOverride overrides) : base(baseData)
    {
        _baseData = baseData;
        EnemyID = baseData.EnemyName;
        Overrides = overrides;
    
        // Initialize State
        SetCurrentHealth(baseData.MaxHealth * overrides.HealthMultiplier);
        SetCurrentSpeed(baseData.Speed * overrides.SpeedMultiplier);
        SetCurrentDamage(baseData.Damage);
        SetCurrentAttackSpeed(baseData.AttackSpeed);
        DynamicMaxHealth = baseData.MaxHealth * overrides.HealthMultiplier;
        GeneratedName = _baseData.GenName;
    }
}
