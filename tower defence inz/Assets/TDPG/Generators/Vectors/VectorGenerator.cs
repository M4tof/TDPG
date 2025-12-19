using System;
using System.Collections.Generic;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;

namespace TDPG.Generators.Vectors
{
    /// <summary>
    /// A composite generator that produces a list of values (a Vector) by invoking a scalar generator multiple times.
    /// <br/>
    /// Useful for generating multi-dimensional data like Coordinates (x,y,z), Colors (r,g,b,a), or Stat Blocks (str, dex, int).
    /// </summary>
    /// <typeparam name="TScalar">The type of the individual elements (e.g., float, int).</typeparam>
    [Serializable]
    public class VectorGenerator<TScalar> : IGenerator<List<TScalar>>
    {
        
        /// <summary>
        /// The number of elements in the generated list (e.g., 2 for 2D, 3 for 3D).
        /// </summary>
        public int Dimension { get; private set; }
        
        /// <summary>
        /// The underlying generator used to produce each individual component of the vector.
        /// </summary>
        public IGenerator<TScalar> ScalarGenerator { get; private set; }

        /// <summary>
        /// Creates a new VectorGenerator.
        /// </summary>
        /// <param name="scalarGenerator">The logic for generating a single element.</param>
        /// <param name="dimension">The size of the resulting list. Must be >= 1.</param>
        /// <exception cref="ArgumentNullException">Thrown if scalarGenerator is null.</exception>
        /// <exception cref="ArgumentException">Thrown if dimension is less than 1.</exception>
        public VectorGenerator(IGenerator<TScalar> scalarGenerator, int dimension = 2)
        {
            if (scalarGenerator == null) throw new ArgumentNullException(nameof(scalarGenerator));
            if (dimension < 1) throw new ArgumentException("Dimension must be >= 1");
            ScalarGenerator = scalarGenerator;
            Dimension = dimension;
        }

        /// <summary>
        /// Generates a list of <see cref="Dimension"/> elements.
        /// <br/>
        /// The <see cref="ScalarGenerator"/> is invoked sequentially for each component, consuming entropy from the source 
        /// to ensure independent, uncorrelated values.
        /// </summary>
        /// <param name="source">The entropy source.</param>
        /// <returns>A list containing the generated components.</returns>
        public List<TScalar> Generate(IRandomSource source)
        {
            var list = new List<TScalar>(Dimension);
            for (int i = 0; i < Dimension; i++)
            {
                // scalar generator may use the same source; to avoid correlation, clone the source for each component
                list.Add(ScalarGenerator.Generate(source));
            }
            return list;
        }

        /// <summary>
        /// Generates a deterministic vector based on the provided Seed.
        /// </summary>
        public List<TScalar> Generate(Seed.Seed seed, string context = null)
        {
            var local = new SplitMix64Random(seed.GetBaseValue());
            return Generate(local);
        }

        /// <summary>
        /// Generates a quick preview vector using a raw seed value.
        /// </summary>
        public List<TScalar> Preview(ulong pseudo)
        {
            var local = new SplitMix64Random(pseudo);
            return Generate(local);
        }
    }
}
