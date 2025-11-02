using System;
using System.Collections.Generic;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;

namespace TDPG.Generators.AttackPatterns
{
    public class BurstLayout : IAttackPatternLayout
    {
        /// <summary>
        /// Generate events clustered near the start of the duration.
        /// Uses provided sub-generators if present; otherwise falls back to defaults.
        /// </summary>
        public List<AttackEvent> GenerateEvents(IRandomSource source, int eventCount, float duration, IGenerator<List<float>> directionGenerator = null, FloatGenerator timeOffsetGenerator = null, FloatGenerator speedGenerator = null, IntGenerator damageGenerator = null, FloatGenerator spreadAngleGenerator = null)
        {
            var list = new List<AttackEvent>(eventCount);
            // Fallbacks:
            var localTimeGen = timeOffsetGenerator ?? new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0f, max = Math.Max(0.01f, duration * 0.3f) };
            var localDirGen = directionGenerator;
            var localSpeedGen = speedGenerator ?? new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 1f, max = 5f };
            var localDmgGen = damageGenerator ?? new IntGenerator { min = 1, max = 5 };
            var localSpreadGen = spreadAngleGenerator ?? new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0f, max = 30f };

            for (int i = 0; i < eventCount; i++)
            {
                var ev = new AttackEvent
                {
                    timeOffset = localTimeGen.Generate(source),
                    direction = localDirGen != null ? localDirGen.Generate(source) : new List<float> { (float)(source.NextFloat() * 2 - 1), (float)(source.NextFloat() * 2 - 1) },
                    speed = localSpeedGen.Generate(source),
                    damage = localDmgGen.Generate(source),
                    spreadAngle = localSpreadGen.Generate(source),
                    metaTag = "burst"
                };
                list.Add(ev);
            }
            return list;
        }
    }
}
