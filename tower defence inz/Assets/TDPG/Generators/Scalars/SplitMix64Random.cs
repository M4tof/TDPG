using TDPG.Generators.Interfaces;

namespace TDPG.Generators.Scalars
{
    /// <summary>
    /// A fast, statistically high-quality 64-bit Pseudo-Random Number Generator (PRNG) based on the SplitMix64 algorithm.
    /// <br/>
    /// It passes the BigCrush test suite and is particularly good at generating high-quality randomness from simple seeds.
    /// Frequently used to bootstrap/seed other generators.
    /// </summary>
    public class SplitMix64Random : IRandomSource
    {
        private ulong state;

        /// <summary>
        /// Initializes the generator with a specific 64-bit seed.
        /// </summary>
        /// <param name="intiValue">The initial state. Different values produce completely distinct sequences.</param>
        public SplitMix64Random(ulong intiValue)
        {
            state = intiValue;
        }

        /// <summary>
        /// advances the internal state and returns the next pseudo-random 64-bit unsigned integer.
        /// </summary>
        public ulong NextUInt64()
        {
            ulong z = (state += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        /// <summary>
        /// Generates a random floating-point number in the range [0.0, 1.0) using 53 bits of precision.
        /// </summary>
        public double NextFloat()
        {
            // produce double in [0,1)
            // use 53 bits of randomness
            ulong x = NextUInt64() >> 11; // keep 53 bits
            return (double)x / (double)(1UL << 53);
        }

        /// <summary>
        /// Creates a new generator instance initialized with the current internal state of this generator.
        /// <br/>
        /// The new instance will produce the exact same future sequence as this one.
        /// </summary>
        public IRandomSource Clone()
        {
            return new SplitMix64Random(state);
        }
    }
}
