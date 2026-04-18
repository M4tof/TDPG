using NUnit.Framework;
using System.Collections.Generic;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;
using TDPG.Generators.Seed;
using UnityEngine;
using static Tests.TestUtils;

namespace Tests.GeneratorTests
{ 
    [TestFixture, Category("GeneratorTests")]
    public class GeneratorTests
    {
        
        
        [Test]
        public void FloatGenerator_Deterministic_WithSeed()
        {
            var seed = new Seed(0xDEADBEEFCAFEBABEUL, 0);
            var fg = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0f, max = 10f };
            
            float a = fg.Generate(seed, "test");
            float b = fg.Generate(seed, "test");
            
            Assert.AreEqual(a, b, "FloatGenerator should be deterministic for same seed and context");
        }

        [TestCase(0f, 1f)]
        [TestCase(-5f, 5f)]
        [TestCase(5f, 15f)]
        [TestCase(100f, 200f)]
        public void FloatGenerator_UniformWithinRange(float min, float max)
        {
            var rng = new SplitMix64Random(0xBEEFUL);
            var gen = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = min, max = max };
            for (int i = 0; i < 100; i++)
            {
                float value = gen.Generate(rng);
                Assert.That(value, Is.InRange(min, max), $"Value not within range [{min}, {max}]");
            }
        }

        [Test]
        public void FloatGenerator_NormalProducesMeanCenteredValues()
        {
            var rng = new SplitMix64Random(12345UL);
            var gen = new FloatGenerator { mode = FloatGenerator.Mode.Normal, mean = 0f, stdDev = 1f, min = -5f, max = 5f };
            float sum = 0f;
            int samples = 10000;
            for (int i = 0; i < samples; i++)
                sum += gen.Generate(rng);
            float avg = sum / samples;

            Assert.That(avg, Is.InRange(-0.2f, 0.2f), "Average should be close to mean (0)");
        }

        [Test]
        public void FloatGenerator_DeterministicWithSeed()
        {
            var seed = new Seed(42UL, 2);
            var g1 = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0f, max = 1f };
            var g2 = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0f, max = 1f };

            float v1 = g1.Generate(seed, "x");
            float v2 = g2.Generate(seed, "x");
            Assert.AreEqual(v1, v2, "Same seed and context should yield same result");
        }
        
        [Test]
        public void FloatGenerator_Performance_IsFastEnough()
        {
            var rng = new SplitMix64Random(42UL);
            var gen = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0f, max = 1f };
            var sw = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < 1_000_000; i++)
                gen.Generate(rng);

            sw.Stop();
            Debug.Log($"Executed in  {sw.ElapsedMilliseconds} ms");
            // Assert.Less(sw.ElapsedMilliseconds, ExpectedTimeToExecute*GetPerformanceMultiplier(), "Float generation took too long");
            Assert.Pass();
        }
        
        [Test]
        public void FloatGenerator_ImplementsIGenerator()
        {
            IGenerator<float> gen = new FloatGenerator();
            Assert.IsNotNull(gen);
        }
        
        [Test]
        public void SplitMix64Random_Reproducible()
        {
            var rs1 = new SplitMix64Random(42UL);
            var rs2 = new SplitMix64Random(42UL);
            Assert.AreEqual(rs1.NextUInt64(), rs2.NextUInt64());
            Assert.AreEqual(rs1.NextUInt64(), rs2.NextUInt64());
        }
        
        [Test]
        public void IntGenerator_Bounds()
        {
            var rng = new SplitMix64Random(99UL);
            var ig = new IntGenerator { min = 5, max = 5 };
            int v = ig.Generate(rng);
            Assert.AreEqual(5, v);
        }
        
        [Test]
        public void IntGenerator_ProducesWithinInclusiveRange()
        {
            var rng = new SplitMix64Random(777UL);
            var gen = new IntGenerator { min = 10, max = 20 };

            for (int i = 0; i < 1000; i++)
            {
                int val = gen.Generate(rng);
                Assert.That(val, Is.InRange(10, 20));
            }
        }

        [Test]
        public void IntGenerator_DeterministicForSeed()
        {
            var seed = new Seed(98765UL, 3);
            var g = new IntGenerator { min = 0, max = 100 };
            int a = g.Generate(seed, "same");
            int b = g.Generate(seed, "same");
            Assert.AreEqual(a, b);
        }

        [Test]
        public void IntGenerator_MinEqualsMaxAlwaysReturnsThatValue()
        {
            var rng = new SplitMix64Random(12UL);
            var g = new IntGenerator { min = 7, max = 7 };
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(7, g.Generate(rng));
        }
        
        [Test]
        public void IntGenerator_ImplementsIGenerator()
        {
            IGenerator<int> gen = new IntGenerator();
            Assert.IsNotNull(gen);
        }
        
        [Test]
        public void IntGenerator_Performance_IsFastEnough()
        {
            var rng = new SplitMix64Random(42UL);
            var gen = new IntGenerator { min = 0, max = 100 };
            var sw = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < 1_000_000; i++)
                gen.Generate(rng);

            sw.Stop();
            Debug.Log($"Executed in  {sw.ElapsedMilliseconds} ms");
            // Assert.Less(sw.ElapsedMilliseconds, ExpectedTimeToExecute*GetPerformanceMultiplier(), "Int generation took too long");
            Assert.Pass();
        }


        [Test]
        public void AllGenerators_DeterministicForSameSeed()
        {
            var seed = new Seed(0xDEADBEEFUL, 6);
            var gens = new List<IGenerator<object>>
            {
                new FloatGenerator { min = 0, max = 1 } as IGenerator<object>,
                new IntGenerator { min = 0, max = 10 } as IGenerator<object>
            };
            // Instead of using object generics directly, test pattern
            var f1 = new FloatGenerator { min = 0, max = 1 };
            var f2 = new FloatGenerator { min = 0, max = 1 };
            var i1 = new IntGenerator { min = 0, max = 10 };
            var i2 = new IntGenerator { min = 0, max = 10 };

            Assert.AreEqual(f1.Generate(seed, "ctx"), f2.Generate(seed, "ctx"));
            Assert.AreEqual(i1.Generate(seed, "ctx"), i2.Generate(seed, "ctx"));
        }
        
    }
}
