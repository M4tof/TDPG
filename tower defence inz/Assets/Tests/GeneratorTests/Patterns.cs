using System;
using System.Linq;
using NUnit.Framework;
using TDPG.Generators.AttackPatterns;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;
using TDPG.Generators.Seed;

namespace Tests.GeneratorTests
{
    public class Patterns
    {
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
        public void BurstAttackPatternGenerator_CreatesValidPattern()
        {
            var generator = new BurstAttackPatternGenerator();
            var seed = new Seed(0xABCDUL, -1);

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