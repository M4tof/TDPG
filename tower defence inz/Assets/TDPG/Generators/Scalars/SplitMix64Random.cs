using Assets.TDPG.Generators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.TDPG.Generators.Scalars
{
    public class SplitMix64Random : IRandomSource
    {
        private ulong state;

        public SplitMix64Random(ulong seed)
        {
            state = seed;
        }

        public ulong NextUInt64()
        {
            ulong z = (state += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        public double NextFloat()
        {
            // produce double in [0,1)
            // use 53 bits of randomness
            ulong x = NextUInt64() >> 11; // keep 53 bits
            return (double)x / (double)(1UL << 53);
        }

        public IRandomSource Clone()
        {
            return new SplitMix64Random(state);
        }
    }
}
