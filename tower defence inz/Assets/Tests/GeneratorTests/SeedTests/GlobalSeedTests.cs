using System;
using System.Threading;
using NUnit.Framework;
using TDPG.Generators.Scalars;
using TDPG.Generators.Seed;
using TDPG.Generators.Vectors;
using UnityEngine;
using static TDPG.Generators.Scalars.InitializerFromDate;

namespace Tests.GeneratorTests.SeedTests
{
    
    [TestFixture, Category("SeedTests")]
    public class GlobalSeedTests
    {
        
        [Test]
        public void GlobalSeed_Creation()
         {
             // Arrange
              var gs = new GlobalSeed(1234,"testGS","testDescription");
             
             // Assert
             Assert.That(gs.GetBaseValue(),Is.EqualTo(1234),"Global seed created properly");
         }
         
         [Test]
         public void GlobalSeed_NextSeedAndGetSubSeed()
         {
             
             // Arrange
             GlobalSeed gs = new GlobalSeed(1234,"testGS","testDescription");
             string key = DateTime.Now.Ticks.ToString();
             gs.NextSubSeed(key);
             
             // Assert
             Assert.IsInstanceOf(typeof(Seed),gs.GetSubSeed(0));
             
             Debug.Log($"{gs.GetSubSeed(0).GetName()} , {gs.GetSubSeed(0).Id} , {gs.GetSubSeed(0).GetBaseValue()} ");
         }
         
         [Test]
         public void GlobalSeed_ToShortHash()
         {
             // Helpers
             double CalculateSimilarity(string hash1, string hash2)
             {
                 // Assuming hexadecimal hashes
                 if (hash1.Length != hash2.Length) return 0;
    
                 int differentBits = 0;
                 int totalBits = hash1.Length * 4; // Each hex char represents 4 bits
    
                 for (int i = 0; i < hash1.Length; i++)
                 {
                     byte b1 = Convert.ToByte(hash1[i].ToString(), 16);
                     byte b2 = Convert.ToByte(hash2[i].ToString(), 16);
        
                     // Count different bits
                     differentBits += CountBits((byte)(b1 ^ b2));
                 }
    
                 return 1.0 - (double)differentBits / totalBits;
             }

             int CountBits(byte value)
             {
                 int count = 0;
                 while (value != 0)
                 {
                     count++;
                     value &= (byte)(value - 1);
                 }
                 return count;
             }
             
             // Arrange
             GlobalSeed gs = new GlobalSeed(1234,"testGS","testDescription");
             GlobalSeed gs2 = new GlobalSeed(1234,"testGS","testDescription");
             GlobalSeed gs3 = new GlobalSeed(1234,"testGS","testDescription");
             string key = DateTime.Now.Ticks.ToString();
             gs.NextSubSeed(key);
             gs.NextSubSeed(key);
             
             gs2.NextSubSeed(key);
             
             gs3.NextSubSeed(key);
             gs3.NextSubSeed(key);
             
             string hashedData = gs.ToShortHash();
             string hashedData2 = gs2.ToShortHash();
             string hashedData3 = gs3.ToShortHash();
             
             double similarity = CalculateSimilarity(hashedData, hashedData2);
             Debug.Log(hashedData+" "+hashedData2+" "+similarity);
             
             // Assert
             Assert.AreNotEqual("testGS1234testDescription2",hashedData);
             
             Assert.That(hashedData, Is.Not.EqualTo(hashedData2));
            
             // Then assert they differ by at least 20%
             Assert.That(similarity, Is.LessThanOrEqualTo(0.8)); // At least 20% different means max 80% similar
             
             Assert.That(hashedData, Is.EqualTo(hashedData3));
         }
         
         [Test]
         public void GlobalSeed_Serialization()
         {
             // Arrange
             GlobalSeed gs = new GlobalSeed(123456789UL, "TestSeed", "A test seed for serialization");
    
             // Add some sub-seeds to test full serialization
             gs.NextSubSeed("firstKey");
             gs.NextSubSeed("secondKey");
             gs.NextSubSeed("thirdKey");

             // Act
             string jsonSave = gs.Serialize();
    
             // Assert
             Assert.IsNotNull(jsonSave);
             Assert.IsFalse(string.IsNullOrEmpty(jsonSave));
    
             // Verify JSON contains expected fields
             StringAssert.Contains("base", jsonSave);
             StringAssert.Contains("name", jsonSave);
             StringAssert.Contains("description", jsonSave);
             StringAssert.Contains("subSeeds", jsonSave);
             StringAssert.Contains("currIndex", jsonSave);
    
             // Verify specific values are in JSON
             StringAssert.Contains("123456789", jsonSave);
             StringAssert.Contains("TestSeed", jsonSave);
             StringAssert.Contains("A test seed for serialization", jsonSave);
             
             // Verify subSeed in JSOn
             StringAssert.Contains("Child of global seed", jsonSave);
    
             Debug.Log($"Serialized JSON: {jsonSave}");
         }
         
         [Test]
         public void GlobalSeed_Deserialization()
         {
             // Arrange
             GlobalSeed gs = new GlobalSeed(123456789UL, "TestSeed", "A test seed for serialization");
    
             // Add some sub-seeds to test full serialization
             gs.NextSubSeed("firstKey");
             gs.NextSubSeed("secondKey");
             gs.NextSubSeed("thirdKey");

             // Act
             string jsonSave = gs.Serialize();

             GlobalSeed newGs = GlobalSeed.Deserialize(jsonSave);
             
             //Assert
             Assert.That(newGs.Name, Is.EqualTo("TestSeed"),"Should hold same name as pre-serialized");
             
             Assert.That(newGs.Description, Is.EqualTo("A test seed for serialization"),"Should hold same description as pre-serialized");
             
             Assert.That(newGs.Base, Is.EqualTo(123456789UL),"Should hold same base as pre-serialized");
             
             Debug.Log($"thirdSubSeed: {newGs.GetSubSeed(2).Value} {newGs.GetSubSeed(2).Id}");
             Assert.NotNull(newGs.GetSubSeed(2),"Should be some subSeed 3");
         }

         [Test]
         public void FakeGamePipelineTest(       
             [NUnit.Framework.Range(10, 1000, 500)] int x,
             [NUnit.Framework.Range(1, 10, 9)] int y,
             [NUnit.Framework.Range(10, 1000, 500)] int z
             ) {
             
             // Make test parameters configurable
             const int EXPECTED_MIN_DIFFERENCE_PERCENT = 80;
             int VECTOR_DIMENSION = x;
             int DELAY_MS = y;
             int SUBSNUM = z;
             
             // Arrange
                //Save game 1
             var initVal1 = QuickGenerate(1);
             var gs1 = new GlobalSeed(initVal1,"SaveSlot1GlobalSeed","Global Seed created for slot1, from initVal1");
             var g1 = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0f, max = 10f };
             var vg1 = new VectorGenerator<float>(g1, dimension: VECTOR_DIMENSION);
             
             for (int i = 0; i < SUBSNUM; i++)
             {
                 string key1 = DateTime.Now.Ticks.ToString();
                 gs1.NextSubSeed(key1);
             }
             string hash1 = gs1.ToShortHash();
             string serialGs1 = gs1.Serialize();
             var list1 = vg1.Generate(gs1.GetSubSeed(0));
             
             // Assume at least some second between game creations
             Thread.Sleep(DELAY_MS);
             
                //Save game 2
             var initVal2 = QuickGenerate(2); 
             var gs2 = new GlobalSeed(initVal2,"SaveSlot2GlobalSeed","Global Seed created for slot2, from initVal2"); 
             var g2 = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0f, max = 10f };
             var vg2 = new VectorGenerator<float>(g2, dimension: VECTOR_DIMENSION); 
             
             for (int i = 0; i < SUBSNUM; i++) 
             { 
                 string key2 = DateTime.Now.Ticks.ToString(); 
                 gs2.NextSubSeed(key2);
             }
             string hash2 = gs2.ToShortHash(); 
             string serialGs2 = gs2.Serialize(); 
             var list2 = vg2.Generate(gs2.GetSubSeed(0));       
             
             // Assert
             
             Debug.Log($"Initial values: {initVal1} vs {initVal2}");
             // Verify that initial values are different
             Assert.That(initVal2, Is.Not.EqualTo(initVal1), "Initial values should be different");
             
             Debug.Log($"Hash1: {hash1}, Hash2: {hash2}");
             // Verify that the global seed hashes are different
             Assert.That(hash2, Is.Not.EqualTo(hash1), "Global seed hashes should be different");
        
             // Verify that the serialized global seeds are different
             Assert.That(serialGs2, Is.Not.EqualTo(serialGs1), "Serialized global seeds should be different");
             
             // Check that at least 40% of the values are different
             int differentCount = 0;
             for (int i = 0; i < list1.Count; i++)
             {
                 if (!Mathf.Approximately(list2[i], list1[i]))
                 {
                     differentCount++;
                 }
             }
             double differencePercentage = (double)differentCount / list1.Count * 100;
             Debug.Log($"Difference percentage: {differencePercentage:F1}%");
             Assert.That(differencePercentage, Is.GreaterThanOrEqualTo(EXPECTED_MIN_DIFFERENCE_PERCENT), 
                 $"At least {EXPECTED_MIN_DIFFERENCE_PERCENT}% of values should be different, but only {differencePercentage:F1}% were different");
             
             // Verify that the generated lists are different
             Assert.That(list2, Is.Not.EqualTo(list1), "Generated value lists should be different");
         }
         
         
    }
}