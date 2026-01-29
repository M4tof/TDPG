using System;
using static TDPG.Generators.Seed.Genetic;

namespace TDPG.Generators.Seed
{
    /// <summary>
    /// Defines the fundamental contract for any entity that carries deterministic seed data.
    /// <br/>
    /// Implementations serve as the 'DNA' for procedural generation, ensuring reproducibility across the library.
    /// </summary>
    public interface ISeed
    {
        /// <summary>
        /// Retrieves the raw 64-bit unsigned integer value used to initialize many types of generators and functions.
        /// </summary>
        /// <returns>The raw entropy value.</returns>
        ulong GetBaseValue();
        
        /// <summary>
        /// Retrieves the identifier or metadata associated with this seed.
        /// <br/>
        /// This varies by implementation: it could be a debug string, a lineage history (ParentA_ParentB), or a unique ID.
        /// </summary>
        /// <returns>The string identifier of the seed.</returns>
        string GetName();
        
        /// <summary>
        /// Combines (Breeds) two <see cref="ISeed"/> instances to create a new offspring <see cref="Seed"/>.
        /// <br/>
        /// Performs a Byte-level Crossover on the base values.
        /// </summary>
        /// <param name="a">The first parent (Source A).</param>
        /// <param name="b">The second parent (Source B).</param>
        /// <remarks>
        /// <b>Note:</b> The resulting seed is assigned ID -1, indicating it is an ad-hoc generation 
        /// not tracked by the central GlobalSeed manager.
        /// </remarks>
        /// <returns>A new Seed instance derived from the genetic combination of A and B.</returns>
        public static Seed operator *(ISeed a, ISeed b) // explicit way to make a new seed, can use Seed and GlobalSeed as parents
        {
            string parentName = "_ChildOf:" + a.GetName() + b.GetName();
            int id = -1; // -1 represents an ad-hoc seed generated outside the GlobalSeed manager's tracking system.
            
            byte[] aByte = BitConverter.GetBytes(a.GetBaseValue());
            byte[] bByte = BitConverter.GetBytes(b.GetBaseValue());
            
            ulong value = ByteCrossover(aByte, bByte);
            
            return new Seed(value,id,parentName);
        }
    }
}