using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDPG.EffectSystem.ElementPlanner;
using UnityEngine;

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

            if (context.Target.TryGetComponent<EnemyStats>(out var stats))
            {
                stats.Speed *= (1f - factor);
            }
            else
            {
                Debug.LogWarning("SlowDownAction: Target has no EnemyStats.");
            }
        }
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

            if (context.Target.TryGetComponent<EnemyStats>(out var stats))
            {
                stats.Health += amount; // amount < 0
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

    public class DurationAction : IEffectAction
    {
        private readonly float duration;

        public DurationAction(float duration)
        {
            this.duration = duration;
        }

        public string Name => "Duration";
        public float Intensity => duration;

        public void Execute(EffectContext context)
        {
            if (context.Target == null) return;

            if (context.Target.TryGetComponent<EnemyStatusEffects>(out var status))
            {
                status.ApplyDuration(duration);
            }
            else
            {
                Debug.Log($"DurationAction executed (duration: {duration}), " +
                          $"but target has no status effect handler.");
            }
            
        }
    }

}
