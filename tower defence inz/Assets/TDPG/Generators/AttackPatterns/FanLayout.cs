using System;
using System.Collections.Generic;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;
using UnityEngine;

namespace TDPG.Generators.AttackPatterns
{
    public class FanLayout : IAttackPatternLayout
    {
        public List<AttackEvent> GenerateEvents(
            IRandomSource source,
            int eventCount,
            float duration,
            IGenerator<List<float>> directionGenerator = null,
            FloatGenerator timeOffsetGenerator = null,
            FloatGenerator speedGenerator = null,
            IntGenerator damageGenerator = null,
            FloatGenerator spreadAngleGenerator = null)
        {
            var list = new List<AttackEvent>(eventCount);

            float totalAngle = spreadAngleGenerator?.Generate(source) ?? 60f;
            float half = totalAngle * 0.5f;
            float step = totalAngle / Math.Max(1, eventCount - 1);

            var speedGen = speedGenerator ?? new FloatGenerator { min = 2f, max = 5f };
            var dmgGen = damageGenerator ?? new IntGenerator { min = 1, max = 4 };

            for (int i = 0; i < eventCount; i++)
            {
                float angle = -half + step * i;
                float rad = angle * Mathf.Deg2Rad;

                list.Add(new AttackEvent
                {
                    timeOffset = duration * 0.1f,
                    direction = new List<float>
                    {
                        Mathf.Cos(rad),
                        Mathf.Sin(rad)
                    },
                    speed = speedGen.Generate(source),
                    damage = dmgGen.Generate(source),
                    spreadAngle = 0f,
                    metaTag = "fan"
                });
            }

            return list;
        }
    }
}
