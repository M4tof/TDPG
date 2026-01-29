using TDPG.Templates.Enemies;
using UnityEngine;

namespace TDPG.EffectSystem.ElementPlanner
{ 
    /// <summary>
    /// Implements a permanent reduction of movement speed.
    /// <br/>
    /// Modifies <see cref="EnemyBaseBehaviour.Logic"/> directly.
    /// </summary>
    public class SlowDownAction : IEffectAction
    {
        private float factor;

        /// <summary>
        /// Creates a permanent slow action.
        /// </summary>
        /// <param name="factor">Percentage to reduce speed (0.0 to 1.0).</param>
        public SlowDownAction(float factor)
        {
            this.factor = factor;
        }

        public string Name => "SlowDown";
        /// <summary>
        /// Returns the slowdown factor.
        /// </summary>
        public float Intensity => factor;

        /// <summary>
        /// Multiplies the target's <see cref="EnemyBaseBehaviour.Logic.CurrentSpeed"/> by <c>(1 - factor)</c>.
        /// </summary>
        /// <param name="context">Must contain a Target with <see cref="EnemyBaseBehaviour"/>.</param>
        public void Execute(EffectContext context)
        {
            if (factor > 1)
            {
                factor = 1 / factor;
            }
            
            if (context.Target == null) return;

            if (context.Target.TryGetComponent<EnemyBaseBehaviour>(out var behaviour))
            {
                behaviour.Logic.CurrentSpeed *= (1f - factor);
            }
            else
            {
                Debug.LogWarning("SlowDownAction: Target has no EnemyStats.");
            }
        }
    }

    /// <summary>
    /// Implements a temporary reduction of movement speed that automatically reverts after a duration.
    /// <br/>
    /// Uses the enemy's internal status effect system (<see cref="EnemyBaseBehaviour.ApplyOrExtendEffect"/>).
    /// </summary>
    public class TempSlowDownAction : IEffectAction
    {
        private float factor;
        private readonly float duration;

        /// <summary>
        /// Creates a temporary slow action.
        /// </summary>
        /// <param name="factor">Percentage to reduce speed.</param>
        /// <param name="duration">Duration in seconds.</param>
        public TempSlowDownAction(float factor, float duration)
        {
            this.factor = factor;
            this.duration = duration;
        }

        public string Name => "TempSlowDown";
        public float Intensity => factor;

        /// <summary>
        /// Applies the slow if it is stronger than the current slow, and registers a callback to restore speed.
        /// </summary>
        public void Execute(EffectContext context)
        {
            if (factor > 1)
            {
                factor = 1 / factor;
            }
            
            if (context.Target == null) return;

            if (context.Target.TryGetComponent<EnemyBaseBehaviour>(out var behaviour))
            {
                float newSpeed = behaviour.Logic.Data.Speed * (1f - factor);

                if (newSpeed < behaviour.Logic.CurrentSpeed)
                {
                    behaviour.SetCurrentSpeed(newSpeed);
                }
                behaviour.ApplyOrExtendEffect("slow", () => behaviour.SetCurrentSpeed(behaviour.Logic.Data.Speed), 1f);
            }
            else
            {
                Debug.LogWarning("TempSlowDownAction: Target has no CurrentSpeed.");
            }
        }
    }

    /// <summary>
    /// Implements a Damage over Time (DoT) effect using an interval ticker.
    /// </summary>
    public class HealthDrainAction : IEffectAction
    {
        private readonly float healthPerDrain;
        private readonly int drainCount;
        private readonly float idleTime;

        /// <summary>
        /// Creates a DoT action.
        /// </summary>
        /// <param name="healthPerDrain">Damage per tick.</param>
        /// <param name="drainCount">Total number of ticks.</param>
        /// <param name="idleTime">Seconds between ticks.</param>
        public HealthDrainAction(float healthPerDrain, int drainCount, float idleTime)
        {
            this.healthPerDrain = healthPerDrain;
            this.idleTime = idleTime;
            this.drainCount = drainCount;
        }

        public string Name => "HealthDrain";
        public float Intensity => healthPerDrain;

        /// <summary>
        /// Registers an iterable effect on the <see cref="EnemyBaseBehaviour"/> that deals damage periodically.
        /// </summary>
        public void Execute(EffectContext context)
        {
            if (context.Target == null) return;

            if (drainCount <= 0) return; //meaningful drainCount is positive only

            if (context.Target.TryGetComponent<EnemyBaseBehaviour>(out var behaviour))
            {
                behaviour.ApplyOrExtendIterableEffect("dota", 
                    (iteration) => behaviour.DealDamage(Mathf.CeilToInt(healthPerDrain)), 
                    drainCount, 
                    idleTime);
            }
            else
            {
                Debug.LogWarning("HealthDownAction: Target has no EnemyStats.");
            }
        }
    }

    /// <summary>
    /// Incapacitates the target by halting movement and logic execution.
    /// </summary>
    public class StunAction : IEffectAction
    {
        private readonly float duration;

        public StunAction(float duration)
        {
            this.duration = duration;
        }

        public string Name => "Stun";
        public float Intensity => duration;

        /// <summary>
        /// Sets speed to 0 and calls <see cref="EnemyBaseBehaviour.ApplyWait"/> to pause the behavior tree.
        /// </summary>
        public void Execute(EffectContext context)
        {
            if (context.Target == null) return;

            if (context.Target.TryGetComponent<EnemyBaseBehaviour>(out var behaviour))
            {
                float temp = behaviour.Logic.CurrentSpeed;
                behaviour.Logic.CurrentSpeed = 0.0f;
                behaviour.ApplyWait(duration);
                behaviour.Logic.CurrentSpeed = temp;
            }
            else
            {
                Debug.LogWarning("StunAction: Target has no CurrentSpeed.");
            }
        }
    }

    /// <summary>
    /// Modifies the local scale of the target's Transform.
    /// </summary>
    public class ScaleAction : IEffectAction
    {
        private readonly float scale;

        /// <summary>
        /// Creates a scaling action.
        /// </summary>
        /// <param name="scale">The uniform scale factor for X and Y axes.</param>
        public ScaleAction(float scale)
        {
            this.scale = scale;
        }

        public string Name => "Scale";
        public float Intensity => scale;

        /// <summary>
        /// Sets <see cref="Transform.localScale"/> to (scale, scale, 1). Assumes 2D/Sprite context.
        /// </summary>
        public void Execute(EffectContext context)
        {
            if (context.Target == null) return;
            if (context.Target.TryGetComponent<Transform>(out var transform))
            {
                transform.localScale = new UnityEngine.Vector3(scale, scale, 1f);
            }
            else
            {
                Debug.LogWarning("ScaleAction: Target has no transform.");
            }
        }

    }

    /// <summary>
    /// Applies immediate damage to the target.
    /// </summary>
    public class HealthDownAction : IEffectAction
    {
        private readonly float amount;

        /// <summary>
        /// Creates a damage action.
        /// </summary>
        /// <param name="amount">
        /// The raw value from the factory. 
        /// <br/><b>Note:</b> This is typically negative coming from the Factory logic.
        /// </param>
        public HealthDownAction(float amount)
        {
            this.amount = amount; // NEGATIVE
        }

        public string Name => "HealthDown";
        /// <summary>
        /// Absolute value of the damage.
        /// </summary>
        public float Intensity => Mathf.Abs(amount);

        /// <summary>
        /// Inverts the negative amount to positive and calls <see cref="EnemyBaseBehaviour.DealDamage"/>.
        /// </summary>
        public void Execute(EffectContext context)
        {
            if (context.Target == null) return;
            if (context.Target.TryGetComponent<EnemyBaseBehaviour>(out var behaviour))
            {
                behaviour.DealDamage(-Mathf.CeilToInt(amount));
            }
            else
            {
                Debug.LogWarning("HealthDownAction: Target has no EnemyStats.");
            }
        }
    }

    /// <summary>
    /// Applies immediate healing to the target. Supports both Ally and Enemy components.
    /// </summary>
    public class HealAction : IEffectAction
    {
        private readonly float amount;

        public HealAction(float amount)
        {
            this.amount = amount; // POSITIVE
        }

        public string Name => "Heal";
        public float Intensity => amount;

        /// <summary>
        /// Adds health to <see cref="AllyStats"/> OR <see cref="EnemyStats"/>.
        /// </summary>
        public void Execute(EffectContext context)
        {
            if (context.Target == null) return;

            if (context.Target.TryGetComponent<AllyStats>(out var stats))
            {
                stats.Health += amount;
            }
            else if (context.Target.TryGetComponent<EnemyStats>(out var enemyStats))
            {
                enemyStats.Health += amount;
            }
            else
            {
                Debug.LogWarning("HealAction: Target has no EnemyStats or AllyStats.");
            }
        }
    }

}
