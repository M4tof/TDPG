using UnityEngine;

namespace TDPG.Templates.Enemies
{
    /// <summary>
    /// A lightweight struct for applying multiplicative modifications to an enemy's base stats.
    /// <br/>
    /// Useful for creating 'Elite' versions or difficulty scaling without altering the base <see cref="EnemyData"/> asset.
    /// </summary>
    [System.Serializable]
    public struct EnemyStatsOverride
    {
        [Tooltip("Multiplier applied to MaxHealth (e.g., 2.0 = double health).")]
        public float HealthMultiplier;
        
        [Tooltip("Multiplier applied to MaxHealth (e.g., 2.0 = double health).")]
        public float SpeedMultiplier;
    
        /// <summary>
        /// Returns a default override with no modifications (all multipliers are 1.0).
        /// </summary>
        public static EnemyStatsOverride Default => new EnemyStatsOverride { HealthMultiplier = 1f, SpeedMultiplier = 1f };
    }
}