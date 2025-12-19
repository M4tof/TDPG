using System;
using TDPG.Generators.Interfaces;

namespace TDPG.Generators.Scalars
{
    /// <summary>
    /// A configurable generator for producing integer values within a specific range.
    /// <br/>
    /// <b>Note:</b> The range is fully inclusive (Min to Max).
    /// </summary>
    [Serializable]
    public class IntGenerator : IGenerator<int>
    {
        /// <summary>
        /// The minimum possible value (inclusive).
        /// </summary>
        public int min = 0;
        
        /// <summary>
        /// The maximum possible value (inclusive).
        /// </summary>
        public int max = 10;

        /// <summary>
        /// Generates an integer between <see cref="min"/> and <see cref="max"/> using the provided random source.
        /// </summary>
        /// <param name="source">The entropy source.</param>
        /// <returns>A value where Min &lt;= Value &lt;= Max.</returns>
        /// <exception cref="ArgumentException">Thrown if Min is greater than Max.</exception>
        public int Generate(IRandomSource source)
        {
            if (min > max) throw new ArgumentException("min > max");
            double u = source.NextFloat();
            long range = (long)max - (long)min + 1;
            long val = (long)(u * range) + min;
            if (val > max) val = max;
            return (int)val;
        }

        /// <summary>
        /// Generates a deterministic integer based on the provided <see cref="Seed.Seed"/> object.
        /// </summary>
        /// <remarks>
        /// Creates a local RNG derived from the seed's base value. <br/>
        /// context goes unused here.
        /// </remarks>
        public int Generate(Seed.Seed seed, string context = null)
        {
            var local = new SplitMix64Random(seed.GetBaseValue());
            return Generate(local);
        }

        /// <summary>
        /// Generates a quick preview integer using a raw seed value.
        /// </summary>
        public int Preview(ulong pseudo)
        {
            return Generate(new SplitMix64Random(pseudo));
        }
    }
}
