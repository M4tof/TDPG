using System.Collections.Generic;

namespace TDPG.Generators.Interfaces
{
    /// <summary>
    /// A specialized generator that produces a sequence (List) of values, effectively creating a Vector of dimension <see cref="Dimension"/>.
    /// <br/>
    /// Useful for generating coordinate tuples (x,y,z), color channels (r,g,b,a), or batched items.
    /// </summary>
    /// <typeparam name="TScalar">The type of the individual elements within the generated list.</typeparam>
    public interface IVectorGenerator<TScalar> : IGenerator<List<TScalar>>
    {
        /// <summary>
        /// The fixed number of elements to be generated in the output list.
        /// </summary>
        int Dimension { get; }
        
        /// <summary>
        /// The underlying generator responsible for producing each individual element within the vector.
        /// </summary>
        IGenerator<TScalar> ScalarGenerator { get; }
    }
}
