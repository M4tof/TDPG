using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.TDPG.Generators.Interfaces
{
    public interface IVectorGenerator<TScalar> : IGenerator<List<TScalar>>
    {
        int Dimension { get; }
        IGenerator<TScalar> ScalarGenerator { get; }
    }
}
