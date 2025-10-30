using System;
using NUnit.Framework;
using TDPG.Generators.Seed;
using UnityEngine;

namespace Tests.SeedTests
{
    
    [TestFixture]
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
         
        
    }
}