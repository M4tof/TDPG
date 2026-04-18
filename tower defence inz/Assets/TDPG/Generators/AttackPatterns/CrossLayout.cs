using System.Collections.Generic;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;

namespace TDPG.Generators.AttackPatterns
{
    public class CrossLayout : IAttackPatternLayout
    {
        private static readonly List<List<float>> BaseDirections = new()
        {
            new() { 1f, 0f },   // Right
            new() { -1f, 0f },  // Left
            new() { 0f, 1f },   // Up
            new() { 0f, -1f }   // Down
        };

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

            var timeGen = timeOffsetGenerator ?? new FloatGenerator { min = 0f, max = duration };
            var speedGen = speedGenerator ?? new FloatGenerator { min = 2f, max = 5f };
            var dmgGen = damageGenerator ?? new IntGenerator { min = 1, max = 4 };
            var spreadGen = spreadAngleGenerator ?? new FloatGenerator { min = 0f, max = 5f };

            for (int i = 0; i < eventCount; i++)
            {
                var dir = BaseDirections[i % BaseDirections.Count];

                list.Add(new AttackEvent
                {
                    timeOffset = timeGen.Generate(source),
                    direction = new List<float>(dir),
                    speed = speedGen.Generate(source),
                    damage = dmgGen.Generate(source),
                    spreadAngle = spreadGen.Generate(source),
                    metaTag = "cross"
                });
            }

            return list;
        }
    }
}
