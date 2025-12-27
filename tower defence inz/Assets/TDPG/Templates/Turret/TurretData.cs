using System.Collections.Generic;
using TDPG.Generators.AttackPatterns;
using UnityEngine;

namespace TDPG.Templates.Turret
{
    /// <summary>
    /// A configuration asset defining the stats, visuals, and behavior of a specific Turret type.
    /// <br/>
    /// Create instances of this in the Project view to define different towers (e.g., "Archer Tower", "Cannon").
    /// </summary>
    [CreateAssetMenu(fileName = "NewTurret", menuName = "TD/Turret Data")]

    
    public class TurretData : ScriptableObject
    {
        [Tooltip("Unique string identifier used for Serialization (Save/Load).")]
        public string TurretID; // Key for Serialization/Registry

        [Header("Visuals")]
        [Tooltip("The sprite used for the static pedestal/foundation.")] public Sprite BaseSprite;    // The static base
        [Tooltip("The sprite used for the active, rotating, or color-swapped top part.")] public Sprite CrystalSprite; // The rotating/color-swapped part

        [Header("Building")]
        [Tooltip("Resource cost to place this turret.")] public float Cost = 50f;
        [Tooltip("The footprint of the turret in Grid Cells (Width x Height).")] public Vector2 TileSize = new Vector2(1, 1);
        [Tooltip("Visual Scale Multiplier (1.0 = Original Prefab Size).")] public float Multiplayer;

        [Header("Combat")]
        [Tooltip("If false, the turret logic updates but does not fire (e.g. Walls/Farms).")] public bool CanShoot =  true;
        [Tooltip("Combat radius in Grid Units.")] public float Range = 5f;

        [Tooltip("Base damage dealt per hit.")] public int Damage = 1;
        [Tooltip("Attack speed in shots per second.")] public float FireRate = 1f; // Shots per second
        [Tooltip("Maximum health of the building.")] public int MaxHP = 10;
        [Tooltip("Upgrade List for turret")] public List<CardData> modifierList;

        [Header("Projectile")]
        // We reference the GameObject containing BasicProjectile script
        [Tooltip("The bullet prefab spawned when firing.")] public GameObject ProjectilePrefab;

        [Header("Procedural Patterns")]
        [Tooltip("Assigned attack pattern used by the turret to shoot its projectiles.")]
        public AbstractAttackPatternGenerator PatternGenerator;
    }
}