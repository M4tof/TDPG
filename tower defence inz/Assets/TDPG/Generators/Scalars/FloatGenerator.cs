using System;
using TDPG.Generators.Interfaces;

namespace TDPG.Generators.Scalars
{
    /// <summary>
    /// A versatile generator for producing floating-point values using various statistical distributions.
    /// <br/>
    /// unlike a simple Random.Range, this allows for bell curves, weighted centers, or quantized steps.
    /// </summary>
    [Serializable]
    public class FloatGenerator : IGenerator<float>
    {
        /// <summary>
        /// Defines the mathematical distribution used to transform raw entropy into a value.
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// Every number between Min and Max has an equal chance of being chosen. 
            /// <br/>Standard "Random.Range" behavior.
            /// </summary>
            Uniform,
            
            /// <summary>
            /// Generates values clustered around a <see cref="mean"/> (Bell Curve).
            /// <br/>Useful for organic attributes (e.g., height, NPC intelligence) where extremes are rare.
            /// </summary>
            Normal,
            
            /// <summary>
            /// Values are weighted towards the center (Average of Min/Max), but unlike Normal distribution, 
            /// it never exceeds the Min/Max bounds strictly.
            /// </summary>
            Triangular,
            
            /// <summary>
            /// Returns values snapped to a specific number of distinct steps between Min and Max.
            /// <br/>Useful for "Tiered" stats (e.g., 1.0, 1.5, 2.0).
            /// </summary>
            Step,
            
            /// <summary>
            /// Uses a custom <see cref="CurveFunc"/> to map the entropy (0..1) to the output range.
            /// </summary>
            Curve
        }
        
        /// <summary>
        /// The distribution logic used to generate the value.
        /// </summary>
        public Mode mode;

        /// <summary>
        /// The absolute minimum value (Inclusive). Acts as a hard clamp for Normal distribution.
        /// </summary>
        public float min;
        
        /// <summary>
        /// The absolute maximum value (Inclusive). Acts as a hard clamp for Normal distribution.
        /// </summary>
        public float max;

        /// <summary>
        /// The center peak of the bell curve. Only used in Normal Mode.
        /// </summary>
        public float mean;
        
        /// <summary>
        /// The standard deviation (width) of the bell curve. Larger values = flatter curve. Only used in Normal Mode.
        /// </summary>
        public float stdDev;
        
        /// <summary>
        /// How many distinct values exist between Min and Max. Only used in Step Mode.
        /// </summary>
        public int steps;
        
        /// <summary>
        /// An optional mathematical function to shape the probability.
        /// <br/>Input is normalized entropy (0.0 to 1.0), Output should be normalized (0.0 to 1.0).
        /// </summary>
        public Func<float, float> CurveFunc = null;


        /// <summary>
        /// Calculates the next float value by consuming entropy from the source and applying the selected <see cref="Mode"/>.
        /// </summary>
        /// <param name="source">The entropy source.</param>
        /// <returns>A value between <see cref="min"/> and <see cref="max"/>.</returns>
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
        
        /// <summary>
        /// Generates a deterministic float based on the provided Seed object.
        /// </summary>
        public float Generate(Seed.Seed seed, string context = null)
        {
            var local = new SplitMix64Random(seed.GetBaseValue());
            return Generate(local);
        }
        
        /// <summary>
        /// Generates a quick preview float using a raw seed value.
        /// </summary>
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
            // Generate approximate normal via Box-Muller
            double u1 = source.NextFloat();
            double u2 = source.NextFloat();
            double z0 = Math.Sqrt(-2.0 * Math.Log(Math.Max(1e-12, u1))) * Math.Cos(2.0 * Math.PI * u2);
            double val = mean + z0 * stdDev;
            // Clamp
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
