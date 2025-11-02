using Assets.TDPG.Generators;
using Assets.TDPG.Generators.AttackPatterns;
using Assets.TDPG.Generators.Interfaces;
using Assets.TDPG.Generators.Scalars;
using Assets.TDPG.Generators.Vectors;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDPG.Generators.Seed;

namespace Tests.GeneratorTests
{
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

        [Test]
        public void VectorGenerator_ProducesDimensionCount()
        {
            var seed = new Seed(12345UL, 1);
            var scalar = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = -1f, max = 1f };
            var vg = new VectorGenerator<float>(scalar, dimension: 3);
            var list = vg.Generate(seed, "v");
            Assert.AreEqual(3, list.Count);
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
        public void BurstAttackPatternGenerator_GeneratesEvents()
        {
            var generator = new BurstAttackPatternGenerator();
            var rng = new SplitMix64Random(0x12345678UL);
            var pattern = generator.Generate(rng);
            Assert.IsNotNull(pattern);
            Assert.IsTrue(pattern.duration >= 0f);
            Assert.IsNotNull(pattern.events);
            Assert.IsTrue(pattern.events.Count >= 0);
            if (pattern.events.Count > 0)
            {
                var ev = pattern.events[0];
                Assert.IsTrue(ev.direction != null && ev.direction.Count >= 2);
            }
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
        public void FloatGenerator_UniformWithinRange()
        {
            var rng = new SplitMix64Random(0xBEEFUL);
            var gen = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 5f, max = 15f };

            for (int i = 0; i < 1000; i++)
            {
                float value = gen.Generate(rng);
                Assert.GreaterOrEqual(value, 5f, "Value below minimum bound");
                Assert.LessOrEqual(value, 15f, "Value above maximum bound");
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
        public void BurstAttackPatternGenerator_CreatesValidPattern()
        {
            var generator = new BurstAttackPatternGenerator();
            var seed = new Seed(0xABCDUL, 4);

            var pattern = generator.Generate(seed, "burst");

            Assert.NotNull(pattern, "Pattern should not be null");
            Assert.IsNotEmpty(pattern.id);
            Assert.That(pattern.duration, Is.GreaterThan(0f));

            Assert.NotNull(pattern.events, "Pattern events list must exist");
            Assert.That(pattern.events.Count, Is.GreaterThanOrEqualTo(1), "Should generate at least one event");

            foreach (var ev in pattern.events)
            {
                Assert.NotNull(ev.direction);
                Assert.AreEqual(2, ev.direction.Count, "Direction should have 2 components");
                Assert.That(ev.speed, Is.GreaterThan(0f));
                Assert.That(ev.damage, Is.GreaterThanOrEqualTo(0));
            }
        }

        [Test]
        public void BurstAttackPatternGenerator_UsesLayoutProperly()
        {
            var gen = new BurstAttackPatternGenerator();
            var rng = new SplitMix64Random(999UL);
            var pattern = gen.Generate(rng);

            Assert.NotNull(pattern.events);
            Assert.That(pattern.events.Count, Is.InRange(1, 6));
            var distinctTags = pattern.events.Select(e => e.metaTag).Distinct().ToList();
            Assert.Contains("burst", distinctTags);
        }

        [Test]
        public void BurstLayout_GeneratesEventsWithinDuration()
        {
            var layout = new BurstLayout();
            var rng = new SplitMix64Random(123UL);
            int eventCount = 10;
            float duration = 3.0f;

            var events = layout.GenerateEvents(rng, eventCount, duration);

            Assert.AreEqual(eventCount, events.Count);
            Assert.True(events.All(e => e.timeOffset <= duration * 0.3f + 0.01f));
        }

        [Test]
        public void BurstAttackPatternGenerator_DeterministicForSeed()
        {
            var gen1 = new BurstAttackPatternGenerator();
            var gen2 = new BurstAttackPatternGenerator();
            var seed = new Seed(12345678UL, 5);

            var p1 = gen1.Generate(seed, "A");
            var p2 = gen2.Generate(seed, "A");

            Assert.AreEqual(p1.duration, p2.duration);
            Assert.AreEqual(p1.events.Count, p2.events.Count);
        }

        [Test]
        public void FloatGenerator_ImplementsIGenerator()
        {
            IGenerator<float> gen = new FloatGenerator();
            Assert.IsNotNull(gen);
        }

        [Test]
        public void IntGenerator_ImplementsIGenerator()
        {
            IGenerator<int> gen = new IntGenerator();
            Assert.IsNotNull(gen);
        }

        [Test]
        public void VectorGenerator_ImplementsIGeneratorOfList()
        {
            var scalar = new FloatGenerator();
            IGenerator<List<float>> vecGen = new VectorGenerator<float>(scalar, 3);
            var rng = new SplitMix64Random(55UL);
            var result = vecGen.Generate(rng);

            Assert.AreEqual(3, result.Count);
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

        [Test]
        public void AttackPatternGenerator_ImplementsIGeneratorAttackPattern()
        {
            IGenerator<AttackPattern> gen = new BurstAttackPatternGenerator();
            var seed = new Seed(333UL, 7);
            var result = gen.Generate(seed, "abc");
            Assert.NotNull(result);
            Assert.NotNull(result.events);
        }

        [Test]
        public void Validate_ThrowsWhenGeneratorsMissing()
        {
            var gen = new BurstAttackPatternGenerator();
            gen.DurationGenerator = null; // force invalid config
            Assert.Throws<InvalidOperationException>(() => gen.Validate());
        }

    }
}
