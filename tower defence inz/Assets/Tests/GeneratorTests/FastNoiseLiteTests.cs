using System;
using NUnit.Framework;
using TDPG.Generators.FastNoiseLite;
using static Tests.TestUtils;

namespace Tests.GeneratorTests
{
    [TestFixture]
    public class NoiseGeneratorsTests
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
            var noise = new FastNoiseLite();
            noise.SetSeed(42);
            noise.SetFrequency(0.05f);

            // Act
            float value1 = noise.GetNoise(10.0f, 20.0f);
            float value2 = noise.GetNoise(15.0f, 25.0f);

            // Assert
            Assert.That(value1, Is.Not.EqualTo(value2).Within(0.0001f),
                "FastNoiseLite should produce different values for different input coordinates.");
        }
        
        [Test]
    public void FastNoiseLite_DifferentSeedsProduceDifferentResults()
    {
        // Arrange
        var noise1 = new FastNoiseLite();
        noise1.SetSeed(42);
        noise1.SetFrequency(0.05f);

        var noise2 = new FastNoiseLite();
        noise2.SetSeed(99);
        noise2.SetFrequency(0.05f);

        // Act
        float value1 = noise1.GetNoise(10.0f, 20.0f);
        float value2 = noise2.GetNoise(10.0f, 20.0f);

        // Assert
        Assert.That(value1, Is.Not.EqualTo(value2).Within(0.0001f),
            "Changing the seed should change the noise output for the same coordinates.");
    }

    [Test]
    public void FastNoiseLite_HigherFrequencyChangesNoisePattern()
    {
        // Arrange
        var lowFreq = new FastNoiseLite();
        lowFreq.SetSeed(42);
        lowFreq.SetFrequency(0.01f);

        var highFreq = new FastNoiseLite();
        highFreq.SetSeed(42);
        highFreq.SetFrequency(0.2f);

        // Act
        float lowValue = lowFreq.GetNoise(10.0f, 20.0f);
        float highValue = highFreq.GetNoise(10.0f, 20.0f);

        // Assert
        Assert.That(lowValue, Is.Not.EqualTo(highValue).Within(0.0001f),
            "Frequency should affect the output pattern of the noise function.");
    }

    [Test]
    public void FastNoiseLite_OutputValue_IsWithinExpectedRange()
    {
        // Arrange
        var noise = new FastNoiseLite();
        noise.SetSeed(42);
        noise.SetFrequency(0.05f);

        // Act
        float value = noise.GetNoise(10.0f, 20.0f);

        // Assert
        Assert.That(value, Is.InRange(-1.0f, 1.0f),
            "Noise output should be normalized within the expected range (-1, 1).");
    }

    [Test]
    public void FastNoiseLite_ProducesContinuousOutput()
    {
        // Arrange
        var noise = new FastNoiseLite();
        noise.SetSeed(42);
        noise.SetFrequency(0.05f);

        // Act
        float value1 = noise.GetNoise(10.0f, 20.0f);
        float value2 = noise.GetNoise(10.1f, 20.1f);

        // Assert
        Assert.That(Math.Abs(value1 - value2), Is.LessThan(0.5f),
            "Noise should vary smoothly between nearby points (no abrupt jumps).");
    }

    [Test]
    public void FastNoiseLite_3DNoise_GeneratesConsistentValues()
    {
        // Arrange
        var noise = new FastNoiseLite();
        noise.SetSeed(42);
        noise.SetFrequency(0.05f);

        // Act
        float v1 = noise.GetNoise(10.0f, 20.0f, 30.0f);
        float v2 = noise.GetNoise(10.0f, 20.0f, 30.0f);

        // Assert
        Assert.That(v1, Is.EqualTo(v2).Within(0.0001f),
            "3D noise should be consistent for the same inputs and seed.");
    }
    
    [Test]
    public void FastNoiseLite_ShouldRespectAmplitude_WhenAdjusted()
    {
        var noise = new FastNoiseLite();
        noise.SetSeed(123);
        noise.SetFrequency(0.05f);
        noise.SetFractalGain(0.5f); // Example if using fractal params

        float value = noise.GetNoise(5f, 10f);
        Assert.That(value, Is.InRange(-1f, 1f));
    }

    [Test]
    public void FastNoiseLite_Performance_IsAcceptable()
    {
        var noise = new FastNoiseLite();
        noise.SetSeed(42);
        noise.SetFrequency(0.1f);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < 10000; i++)
            noise.GetNoise(i * 0.01f, i * 0.02f);

        stopwatch.Stop();

        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(ExpectedTimeToExecute*GetPerformanceMultiplier()),
            "Noise generation should be performant enough for real-time use.");
    }
    
    [Test]
    public void FastNoiseLite_ValueWithinRange_ForGivenInputs(
        [Range(10, 1000, 250)] float x,
        [Range(10, 1000, 250)] float y)
    {
        // Arrange
        var noise = new FastNoiseLite();
        noise.SetSeed(42);
        noise.SetFrequency(0.05f);

        // Act
        float value = noise.GetNoise(x, y);

        // Assert
        Assert.That(value, Is.InRange(-1.0f, 1.0f),
            $"Noise value at ({x}, {y}) should be between -1 and 1, but was {value}.");
    }
        
    
    
    }
}