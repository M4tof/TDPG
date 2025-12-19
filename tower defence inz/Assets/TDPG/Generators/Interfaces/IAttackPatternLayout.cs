using System.Collections.Generic;
using TDPG.Generators.AttackPatterns;
using TDPG.Generators.Scalars;

namespace TDPG.Generators.Interfaces
{
    /// <summary>
    /// Defines a strategy for arranging <see cref="AttackEvent"/>s within a pattern.
    /// <br/>
    /// Implementations determine the "shape" or timing logic (e.g., Burst, Stream, Spiral, Shotgun)
    /// by utilizing the provided sub-generators to populate specific event data.
    /// </summary>
    public interface IAttackPatternLayout
    {
        /// <summary>
        /// Generates a list of individual attack events (projectiles, hitboxes) based on the specific layout logic.
        /// </summary>
        /// <param name="source">The RNG provider.</param>
        /// <param name="eventCount">The total number of events to generate.</param>
        /// <param name="duration">The total time window of the attack pattern.</param>
        /// <param name="directionGenerator">Generator for the direction vector. If null, layout may use defaults.</param>
        /// <param name="timeOffsetGenerator">Generator for the relative start time (0 to duration) of each event.</param>
        /// <param name="speedGenerator">Generator for projectile speed.</param>
        /// <param name="damageGenerator">Generator for damage values.</param>
        /// <param name="spreadAngleGenerator">Generator for angular deviation (spread).</param>
        /// <returns>A list of configured attack events.</returns>
        List<AttackEvent> GenerateEvents(IRandomSource source, int eventCount, float duration, IGenerator<List<float>> directionGenerator = null, FloatGenerator timeOffsetGenerator = null, FloatGenerator speedGenerator = null, IntGenerator damageGenerator = null, FloatGenerator spreadAngleGenerator = null);
    }
}
