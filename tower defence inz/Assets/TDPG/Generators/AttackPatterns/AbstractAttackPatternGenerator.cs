using System;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;
using UnityEngine;

namespace TDPG.Generators.AttackPatterns
{
    /// <summary>
    /// Base implementation for <see cref="IAttackPatternGenerator"/>.
    /// <br/>
    /// Handles the common boilerplate logic for deterministic seeding and validation of mandatory properties
    /// (Duration and Event Count).
    /// </summary>
    [Serializable]
    public abstract class AbstractAttackPatternGenerator : ScriptableObject, IAttackPatternGenerator
    {
        /// <summary>
        /// Generator responsible for determining the total duration (in seconds) of the attack pattern.
        /// </summary>
        public FloatGenerator DurationGenerator { get; set; }
        
        /// <summary>
        /// Generator responsible for determining how many individual events (projectiles/hitboxes) spawn.
        /// </summary>
        public IntGenerator EventCountGenerator { get; set; }


        /// <summary>
        /// A unique name for this specific type of pattern.
        /// <br/>Useful for displaying information concerning said pattern type during gameplay.
        /// </summary>
        public string patternName;

        /// <summary>
        /// Core generation logic to be implemented by concrete classes (e.g., Burst, Stream).
        /// </summary>
        /// <param name="source">The entropy source.</param>
        /// <returns>A fully configured AttackPattern.</returns>
        public abstract AttackPattern Generate(IRandomSource source);

        /// <summary>
        /// Standard implementation of the Seed-based generation.
        /// <br/>
        /// Creates a temporary <see cref="SplitMix64Random"/> from the seed and invokes the abstract <see cref="Generate(IRandomSource)"/>.
        /// </summary>
        public virtual AttackPattern Generate(Seed.Seed seed, string context = null)
        {
            var local = new SplitMix64Random(seed.GetBaseValue());
            return Generate(local);
        }

        public virtual AttackPattern Preview(ulong pseudo)
        {
            return Generate(new SplitMix64Random(pseudo));
        }

        public virtual void Validate()
        {
            if (DurationGenerator == null) throw new InvalidOperationException("DurationGenerator is null");
            if (EventCountGenerator == null) throw new InvalidOperationException("EventCountGenerator is null");
        }
    }
}
