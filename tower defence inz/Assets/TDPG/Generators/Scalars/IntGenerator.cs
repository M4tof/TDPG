using System;
using TDPG.Generators.Interfaces;

namespace TDPG.Generators.Scalars
{
    [Serializable]
    public class IntGenerator : IGenerator<int>
    {
        public int min = 0;
        public int max = 10;

        public int Generate(IRandomSource source)
        {
            if (min > max) throw new ArgumentException("min > max");
            double u = source.NextFloat();
            long range = (long)max - (long)min + 1;
            long val = (long)(u * range) + min;
            if (val > max) val = max;
            return (int)val;
        }

        public int Generate(Seed.Seed seed, string context = null)
        {
            var local = new SplitMix64Random(seed.GetBaseValue());
            return Generate(local);
        }

        public int Preview(ulong pseudo)
        {
            return Generate(new SplitMix64Random(pseudo));
        }
    }
}
