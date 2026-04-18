namespace TDPG.Generators.Interfaces
{
    /// <summary>
    /// Defines the contract for a procedural generator that produces content of type <typeparamref name="T"/>.
    /// <br/>
    /// Implementations should be factories that convert randomness into structured data.
    /// </summary>
    /// <typeparam name="T">The type of content generated (e.g., int, float, specific Struct).</typeparam>
    public interface IGenerator<T>
    {
        /// <summary>
        /// Generates a new instance of <typeparamref name="T"/> consuming entropy from the provided random source.
        /// <br/>
        /// This method advances the state of the <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The RNG provider to consume.</param>
        /// <returns>A procedurally generated instance.</returns>
        T Generate(IRandomSource source);
        
        /// <summary>
        /// Generates a deterministic instance based on a structured <see cref="Seed.Seed"/> object.
        /// <br/>
        /// Useful for complex hierarchies where different systems (Contexts) need distinct random streams derived from the same master seed.
        /// </summary>
        /// <param name="seed">The master seed object.</param>
        /// <param name="context">
        /// Optional salt/modifier (e.g., "Loot", "Terrain"). 
        /// <br/>If provided, it alters the RNG state so that <c>Generate(seed, "A")</c> differs from <c>Generate(seed, "B")</c>.
        /// <br/> A factory doesn't necessarily need to use the context
        /// </param>
        /// <returns>A procedurally generated instance.</returns>
        T Generate(Seed.Seed seed, string context = null);
        
        /// <summary>
        /// Generates a result using a raw 64-bit seed value.
        /// <br/>
        /// Intended for lightweight "What if?" checks, UI previews, or simple one-off generations 
        /// where creating a full <see cref="Seed.Seed"/> or <see cref="IRandomSource"/> is unnecessary.
        /// </summary>
        /// <param name="pseudo">The raw seed value.</param>
        /// <returns>A procedurally generated instance.</returns>
        T Preview(ulong pseudo);
    }
}
