using System;
using System.Collections.Generic;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;

namespace TDPG.Generators.AttackPatterns
{
    /// <summary>
    /// An attack layout strategy that arranges events in a rapid, simultaneous cluster (Burst).
    /// <br/>
    /// Typically used for Shotguns, Multi-missile launches, or rapid-fire volleys where 
    /// multiple projectiles are fired almost instantly at the start of the pattern.
    /// </summary>
    public class BurstLayout : IAttackPatternLayout
    {
        /// <summary>
        /// Generates a list of <see cref="AttackEvent"/>s clustered closely together in time.
        /// </summary>
        /// <remarks>
        /// <b>Fallback Behavior:</b>
        /// <br/>If specific generators are not provided (null), this layout uses internal defaults:
        /// <list type="bullet">
        /// <item><description><b>Time:</b> Randomly distributed within the first <b>30%</b> of the <paramref name="duration"/>.</description></item>
        /// <item><description><b>Direction:</b> Random 2D Vector (X/Y between -1 and 1).</description></item>
        /// <item><description><b>MetaTag:</b> Sets the tag to "burst".</description></item>
        /// </list>
        /// </remarks>
        /// <param name="source">The entropy source.</param>
        /// <param name="eventCount">The number of projectiles/events to spawn.</param>
        /// <param name="duration">The total window of the attack (though burst events usually occur early).</param>
        /// <param name="directionGenerator">Custom direction logic. If null, uses random 2D noise.</param>
        /// <param name="timeOffsetGenerator">Custom timing logic. If null, defaults to [0, Duration * 0.3].</param>
        /// <param name="speedGenerator">Generator for projectile speed.</param>
        /// <param name="damageGenerator">Generator for damage values.</param>
        /// <param name="spreadAngleGenerator">Generator for spread deviation.</param>
        /// <returns>A list of configured attack events.</returns>
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
