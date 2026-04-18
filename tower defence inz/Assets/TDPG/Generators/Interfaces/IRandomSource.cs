namespace TDPG.Generators.Interfaces
{
    /// <summary>
    /// Interface defining a generic source of randomness for TDPG generators.
    /// <br/>Implement this to provide custom algorithms (e.g., PCG, Xorshift) or wrap <see cref="System.Random"/>.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>
        /// Generates the next random 64-bit unsigned integer in the sequence.
        /// </summary>
        /// <returns>A random value covering the full range of <see cref="ulong"/>.</returns>
        ulong NextUInt64();
        
        /// <summary>
        /// Generates a random normalized floating-point number.
        /// </summary>
        /// <returns>A double between 0.0 (inclusive) and 1.0 (exclusive).</returns>
        double NextFloat();
        
        /// <summary>
        /// Creates a distinct copy of the RNG in its <b>current state</b>.
        /// <br/>
        /// Both the original and the clone will produce the exact same sequence of numbers from this point forward.
        /// Useful for forking deterministic generation paths.
        /// </summary>
        /// <returns>A deep copy of the random source.</returns>
        IRandomSource Clone();
    }
}
