namespace TDPG.Generators.Seed
{
    /// <summary>
    /// Defines the strategy used to alter the entropy of a <see cref="Seed"/>.
    /// </summary>
    public enum MutateTypes
    {
        /// <summary>
        /// Applies a fixed, reproducible transformation using a specific crossover mask.
        /// <br/>
        /// The same input Seed will always produce the same mutated output.
        /// </summary>
        Deterministic, 
        
        /// <summary>
        /// Applies non-deterministic noise (using System.Random).
        /// <br/>
        /// <b>Warning:</b> This breaks reproducibility. Use this only for "Chaos" injection or one-off runtime events.
        /// </summary>
        Random, 
        
        /// <summary>
        /// Returns the original seed unmodified.
        /// </summary>
        None
    }
}