using System.Collections.Generic;

namespace TDPG.Generators.Interfaces
{
    public interface IVectorGenerator<TScalar> : IGenerator<List<TScalar>>
    {
        int Dimension { get; }
        IGenerator<TScalar> ScalarGenerator { get; }
    }
}
