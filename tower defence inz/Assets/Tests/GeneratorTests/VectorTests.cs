using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;
using TDPG.Generators.Seed;
using TDPG.Generators.Vectors;
using UnityEngine;

namespace Tests.GeneratorTests
{
    public class VectorTests
    {
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
        public void VectorGenerator_ImplementsIGeneratorOfList()
        {
            var scalar = new FloatGenerator();
            IGenerator<List<float>> vecGen = new VectorGenerator<float>(scalar, 3);
            var rng = new SplitMix64Random(55UL);
            var result = vecGen.Generate(rng);

            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void VectorGenerator_GeneratesFloatsNotFloat()
        {
            var seed = new Seed(12345UL, 1);
            var scalar = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = -1f, max = 1f };
    
            var vg = new VectorGenerator<float>(scalar, dimension: 10);
            var list = vg.Generate(seed);
    
            Assert.AreEqual(10, list.Count);
    
            // Check that we don't have the same value repeated for all elements
            bool allSame = list.All(x => Mathf.Approximately(x, list[0]));
            Assert.IsFalse(allSame, "All vector elements should not be identical");
    
            // Check that we have at least 2 distinct values
            var distinctCount = list.Distinct().Count();
            Assert.GreaterOrEqual(distinctCount, 2, "Should have at least 2 distinct float values");
    
            Debug.Log($"Generated {distinctCount} distinct values: {string.Join(", ", list)}");
        }
        
        [Test]
        public void VectorGenerator_GeneratesIntsNotInt()
        {
            var seed = new Seed(101010123, 1);
            var scalar = new IntGenerator { min = 5, max = 50 };
    
            var vg = new VectorGenerator<int>(scalar, dimension: 100);
            var list = vg.Generate(seed);
    
            Assert.AreEqual(100, list.Count);
            
            // Check that we don't have the same value repeated for all elements
            bool allSame = list.All(x => Mathf.Approximately(x, list[0]));
            Assert.IsFalse(allSame, "All vector elements should not be identical");
    
            // Check that we have at least 2 distinct values
            var distinctCount = list.Distinct().Count();
            Assert.GreaterOrEqual(distinctCount, 2, "Should have at least 2 distinct float values");
    
            Debug.Log($"Generated {distinctCount} distinct values: {string.Join(", ", list)}");
        }
        
    }
}