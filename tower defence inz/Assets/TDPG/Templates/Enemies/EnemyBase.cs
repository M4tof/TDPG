using UnityEngine;

namespace TDPG.Templates.Enemies
{
    /// <summary>
    /// The abstract logical core of an enemy unit.
    /// <br/>
    /// This class acts as the "Model" in the pattern, holding the raw runtime state (Health, Speed, Position) 
    /// separate from the Unity GameObject/Transform logic.
    /// </summary>
    public abstract class EnemyBase
    {
        /// <summary>
        /// The immutable configuration asset defining this enemy's base stats and visuals.
        /// </summary>
        public EnemyData Data { get; private set; }

        /// <summary>
        /// The current hit points of the unit.
        /// </summary>
        public float CurrentHealth { get; set; }
        
        /// <summary>
        /// The current movement speed in world units per second.
        /// <br/>modified by buffs/debuffs (e.g. Slows).
        /// </summary>
        public float CurrentSpeed { get; set; }
        
        /// <summary>
        /// The current damage this enemy can deal.
        /// </summary>
        public int CurrentDamage { get; set; }
        
        /// <summary>
        /// The current attack speed (attacks per second).
        /// </summary>
        public float CurrentAttackSpeed { get; set; }

        /// <summary>
        /// The logical position of the enemy (Grid coordinates or 2D World coords).
        /// </summary>
        public Vector2 Position { get; set; }

        public float DynamicMaxHealth { get; set;}
        
        /// <summary>
        /// Initializes the logical model with base stats from the provided data asset.
        /// </summary>
        /// <param name="data">The configuration template.</param>
        public EnemyBase(EnemyData data)
        {
            Data = data;
        }

        public virtual void OnCreation()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnDeath()
        {
        }

        public void ApplyStatus(StatusComponent status)
        {

        }

        /// <summary>
        /// Reduces the current health by the specified amount.
        /// </summary>
        /// <remarks>
        /// <b>Note:</b> This method only modifies the value. It does not automatically trigger <see cref="OnDeath"/>.
        /// The controller is responsible for checking if Health &lt;= 0.
        /// </remarks>
        /// <param name="damage">Amount of health to subtract.</param>
        public void DealDamage(int damage)
        {
            CurrentHealth -= damage;
            //Debug.Log($"DEAL {damage} DMG {CurrentHealth} HP");
        }

        /// <summary>
        /// Updates the movement speed.
        /// </summary>
        public void SetCurrentSpeed(float speed)
        {
             CurrentSpeed = speed;
        }
        /// <summary>
        /// Retrieves the current movement speed.
        /// </summary>
        public float GetCurrentSpeed()
        {
            return CurrentSpeed;
        }
        /// <summary>
        /// Retrieves the current health points.
        /// </summary>
        public float GetCurrentHealth()
        {
            return CurrentHealth;
        }

        /// <summary>
        /// Retrieves the current damage value.
        /// </summary>
        /// <returns>Int value representing the current damage value the enemy can deal.</returns>
        public int GetCurrentDamage()
        {
            return CurrentDamage;
        }

        /// <summary>
        /// Sets the current damage value.
        /// </summary>
        public void SetCurrentDamage(int damage)
        {
            CurrentDamage = damage;
        }

        /// <summary>
        /// Retrieves the current attack speed.
        /// </summary>  
        /// <returns>Float value representing the current attack speed (attacks per second).</returns>
        public float GetCurrentAttackSpeed()
        {
            return CurrentAttackSpeed;
        }
        
        /// <summary>
        /// Sets the current attack speed.
        /// </summary>
        public void SetCurrentAttackSpeed(float attackSpeed)
        {
            CurrentAttackSpeed = attackSpeed;
        }
        
        /// <summary>
        /// Forcefully sets the current health (e.g. for Heals or initialization).
        /// </summary>
        public void SetCurrentHealth(float health)
        {
            CurrentHealth = health;
        }

        public int GetReward()
        {
            return Data.Reward;
        }

    }
}