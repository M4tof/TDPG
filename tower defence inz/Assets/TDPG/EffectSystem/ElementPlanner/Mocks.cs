using System.Collections.Generic;
using UnityEngine;

namespace TDPG.EffectSystem.ElementPlanner
{
    public class EnemyStats : MonoBehaviour
    {
        public float Health = 100f;
        public float Speed = 10f;

        public List<float> AppliedDurations = new();
    }

    public class AllyStats : MonoBehaviour
    {
        public float Health = 50f;
    }

    public class EnemyStatusEffects : MonoBehaviour
    {
        public List<float> Durations = new();

        public void ApplyDuration(float d)
        {
            Durations.Add(d);
        }
    }
}
