using System.Collections.Generic;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;
using UnityEngine;

namespace TDPG.Generators.AttackPatterns
{
    public class SpiralLayout : IAttackPatternLayout
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

            var speedGen = speedGenerator ?? new FloatGenerator { min = 2f, max = 4f };
            var dmgGen = damageGenerator ?? new IntGenerator { min = 1, max = 3 };

            float angleStep = 360f / eventCount;

            for (int i = 0; i < eventCount; i++)
            {
                float angleDeg = i * angleStep;
                float rad = angleDeg * Mathf.Deg2Rad;

                list.Add(new AttackEvent
                {
                    timeOffset = duration * ((float)i / eventCount),
                    direction = new List<float>
                    {
                        Mathf.Cos(rad),
                        Mathf.Sin(rad)
                    },
                    speed = speedGen.Generate(source),
                    damage = dmgGen.Generate(source),
                    spreadAngle = 0f,
                    metaTag = "spiral"
                });
            }

            return list;
        }
    }
}
