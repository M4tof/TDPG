using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDPG.Generators.Seed;

namespace Assets.TDPG.Generators.Interfaces
{
    public interface IGenerator<T>
    {
        T Generate(IRandomSource source);
        T Generate(Seed seed, string context = null);
        T Preview(ulong pseudo);
    }
}
