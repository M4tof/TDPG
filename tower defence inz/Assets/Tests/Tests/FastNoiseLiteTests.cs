using NUnit.Framework;
using NUnit.Framework.Internal;
using TowerDefenseProceduralGeneration.Generators.FastNoiseLite;

namespace Tests
{
    [TestFixture]
    public class GeneratorsTests
    {
        [Test]
        public void FastNoiseLite_GeneratesConsistentValue()
        {
            // Arrange 
            
            var noise = new FastNoiseLite();
            noise.SetSeed(42);
            noise.SetFrequency(0.05f);

            // Act
            float value1 = noise.GetNoise(10.0f, 20.0f);
            float value2 = noise.GetNoise(10.0f, 20.0f);

            // Assert
            Assert.That(value1, Is.EqualTo(value2).Within(0.0001f),
                "FastNoiseLite should return consistent results for the same input and seed.");
        }

        [Test]
        public void FastNoiseLite_GeneratesDifferentValuesForDifferentInputs()
        {
            // Arrange
            var noise = new FastNoiseLite();;
            noise.SetSeed(42);
            noise.SetFrequency(0.05f);

            // Act
            float value1 = noise.GetNoise(10.0f, 20.0f);
            float value2 = noise.GetNoise(15.0f, 25.0f);

            // Assert
            Assert.That(value1, Is.Not.EqualTo(value2).Within(0.0001f),
                "FastNoiseLite should produce different values for different input coordinates.");
        }
    }
}