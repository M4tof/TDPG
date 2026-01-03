using UnityEngine;

namespace TDPG.Templates.Enemies
{
    /// <summary>
    /// A configuration asset defining the base statistics and visuals for an Enemy unit.
    /// <br/>
    /// Create instances of this to define different enemy types (e.g., "Goblin", "Boss").
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "EnemyData", order = 0)]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Stats")]
        [Tooltip("The display name of the enemy.")]
        public string EnemyName;
        
        [Tooltip("The starting health points of the unit.")]
        public float MaxHealth;
        
        [Tooltip("The base movement speed in World Units per second.")]
        public float Speed;

        [Tooltip("The damage dealt by the enemy upon reaching the goal or tower in the way.")]
        public int Damage;
        
        [Tooltip("The attack speed of the enemy (attacks per second).")]
        public float AttackSpeed;

        [Tooltip("Reward for eleminating enemy")]
        public int Reward;

        [Header("Visuals")]
        [Tooltip("The sprite used by the SpriteRenderer.")]
        public Sprite EnemySprite;
        
        
        
    }
}