using Assets.TDPG.Generators.Interfaces;
using Assets.TDPG.Generators.Scalars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDPG.Generators.Seed;

namespace Assets.TDPG.Generators.Vectors
{
    [Serializable]
    public class VectorGenerator<TScalar> : IGenerator<List<TScalar>>
    {
        public int Dimension { get; private set; }
        public IGenerator<TScalar> ScalarGenerator { get; private set; }

        public VectorGenerator(IGenerator<TScalar> scalarGenerator, int dimension = 2)
        {
            if (scalarGenerator == null) throw new ArgumentNullException(nameof(scalarGenerator));
            if (dimension < 1) throw new ArgumentException("Dimension must be >= 1");
            ScalarGenerator = scalarGenerator;
            Dimension = dimension;
        }

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

        public List<TScalar> Generate(Seed seed, string context = null)
        {
            var local = new SplitMix64Random(seed.GetBaseValue());
            return Generate(local);
        }

        public List<TScalar> Preview(ulong pseudo)
        {
            var local = new SplitMix64Random(pseudo);
            return Generate(local);
        }
    }
}
