using System;
using TDPG.Generators.Interfaces;

namespace TDPG.Generators.Scalars
{
    [Serializable]
    public class FloatGenerator : IGenerator<float>
    {
        public enum Mode { Uniform, Normal, Triangular, Step, Curve }
        public Mode mode;
        public float min, max;
        public float mean, stdDev;
        public int steps;
        public Func<float, float> CurveFunc = null;


        public float Generate(IRandomSource source)
        {
            switch (mode)
            {
                case Mode.Uniform:
                    return Uniform(source, min, max);
                case Mode.Normal:
                    return Normal(source, mean, stdDev, min, max);
                case Mode.Triangular:
                    return Triangular(source, min, max);
                case Mode.Step:
                    return Step(source, min, max, steps);
                case Mode.Curve:
                    {
                        float u = (float)source.NextFloat();
                        float mapped = CurveFunc != null ? CurveFunc(u) : u;
                        return Lerp(min, max, mapped);
                    }
                default:
                    return Uniform(source, min, max);
            }
        }
        public float Generate(Seed.Seed seed, string context = null)
        {
            var local = new SplitMix64Random(seed.GetBaseValue());
            return Generate(local);
        }
        public float Preview(ulong pseudo)
        {
            var local = new SplitMix64Random(pseudo);
            return Generate(local);
        }

        private static float Uniform(IRandomSource source, float a, float b)
        {
            return (float)(a + (b - a) * source.NextFloat());
        }

        // Box-Muller
        private static float Normal(IRandomSource source, float mean, float stdDev, float min, float max)
        {
            // generate approximate normal via Box-Muller
            double u1 = source.NextFloat();
            double u2 = source.NextFloat();
            double z0 = Math.Sqrt(-2.0 * Math.Log(Math.Max(1e-12, u1))) * Math.Cos(2.0 * Math.PI * u2);
            double val = mean + z0 * stdDev;
            // clamp
            if (val < min) val = min;
            if (val > max) val = max;
            return (float)val;
        }

        private static float Triangular(IRandomSource source, float a, float b)
        {
            float u = (float)source.NextFloat();
            float c = (a + b) / 2f;
            if (u < (c - a) / (b - a))
                return a + (float)Math.Sqrt(u * (b - a) * (c - a));
            else
                return b - (float)Math.Sqrt((1 - u) * (b - a) * (b - c));
        }

        private static float Step(IRandomSource source, float a, float b, int steps)
        {
            if (steps <= 1) return Uniform(source, a, b);
            int idx = (int)(source.NextFloat() * steps);
            if (idx >= steps) idx = steps - 1;
            float t = idx / (float)(steps - 1);
            return Lerp(a, b, t);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }

}
