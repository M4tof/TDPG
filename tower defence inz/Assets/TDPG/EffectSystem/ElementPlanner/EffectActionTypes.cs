using TDPG.Templates.Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TDPG.EffectSystem.ElementPlanner;
using TDPG.Templates.Enemies;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace TDPG.EffectSystem.ElementPlanner
{
    // Example actions
    public class SlowDownAction : IEffectAction
    {
        private readonly float factor;

        public SlowDownAction(float factor)
        {
            this.factor = factor;
        }

        public string Name => "SlowDown";
        public float Intensity => factor;

        public void Execute(EffectContext context)
        {
            if (context.Target == null) return;

            if (context.Target.TryGetComponent<EnemyBaseBehaviour>(out var behaviour))
            {
                Debug.Log($"BEHAVIOUR {behaviour},LOGIC {behaviour.Logic}, FACTOR {factor}");
                behaviour.Logic.CurrentSpeed *= (1f - factor);
            }
            else
            {
                Debug.LogWarning("SlowDownAction: Target has no EnemyStats.");
            }
        }
    }

    public class TempSlowDownAction : IEffectAction
    {
        private readonly float factor;
        private readonly float duration;

        public TempSlowDownAction(float factor, float duration)
        {
            this.factor = factor;
            this.duration = duration;
        }

        public string Name => "TempSlowDown";
        public float Intensity => factor;

        public void Execute(EffectContext context)
        {
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

    public class HealthDrainAction : IEffectAction
    {
        private readonly float healthPerDrain;
        private readonly int drainCount;
        private readonly float idleTime;

        public HealthDrainAction(float healthPerDrain, int drainCount, float idleTime)
        {
            this.healthPerDrain = healthPerDrain;
            this.idleTime = idleTime;
            this.drainCount = drainCount;
        }

        public string Name => "HealthDrain";
        public float Intensity => healthPerDrain;

        public void Execute(EffectContext context)
        {
            Debug.Log("DOTA");
            if (context.Target == null) return;

            if (drainCount <= 0) return; //meaningful drainCount is positive only

            if (context.Target.TryGetComponent<EnemyBaseBehaviour>(out var behaviour))
            {
                behaviour.ApplyOrExtendIterableEffect("dota", 
                    (iteration) => behaviour.DealDamage((int) healthPerDrain), 
                    drainCount, 
                    idleTime);
                Debug.Log("BURN!!");
            }
            else
            {
                Debug.LogWarning("HealthDownAction: Target has no EnemyStats.");
            }
        }
    }

    public class StunAction : IEffectAction
    {
        private readonly float duration;

        public StunAction(float duration)
        {
            this.duration = duration;
        }

        public string Name => "Stun";
        public float Intensity => duration;

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

        /*private IEnumerator ApplyRoutine(EnemyBase logic)
        {
            logic.CurrentSpeed *= (1f - factor);
            yield return new WaitForSeconds(duration);
            logic.CurrentSpeed /= (1f - factor);
        }*/
    }

    public class ScaleAction : IEffectAction
    {
        private readonly float scale;

        public ScaleAction(float scale)
        {
            this.scale = scale;
        }

        public string Name => "Scale";
        public float Intensity => scale;

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

        /*private IEnumerator ApplyRoutine(EnemyBase logic)
        {
            logic.CurrentSpeed *= (1f - factor);
            yield return new WaitForSeconds(duration);
            logic.CurrentSpeed /= (1f - factor);
        }*/
    }

    public class HealthDownAction : IEffectAction
    {
        private readonly float amount;

        public HealthDownAction(float amount)
        {
            this.amount = amount; // NEGATIVE
        }

        public string Name => "HealthDown";
        public float Intensity => Mathf.Abs(amount);

        public void Execute(EffectContext context)
        {
            if (context.Target == null) return;

            if (context.Target.TryGetComponent<EnemyBaseBehaviour>(out var behaviour))
            {
                //behaviour.Logic.CurrentHealth += amount; // amount < 0
                behaviour.DealDamage((int) -amount);
                Debug.Log(behaviour.Logic.CurrentHealth);
            }
            else
            {
                Debug.LogWarning("HealthDownAction: Target has no EnemyStats.");
            }
        }
    }

    public class HealAction : IEffectAction
    {
        private readonly float amount;

        public HealAction(float amount)
        {
            this.amount = amount; // POSITIVE
        }

        public string Name => "Heal";
        public float Intensity => amount;

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
    //TODO: long-term + change initial ones

}
