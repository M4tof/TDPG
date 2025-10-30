using System;
using NUnit.Framework;
using TDPG.Generators.Seed;
using UnityEngine;

namespace Tests.SeedTests
{
        [TestFixture]
        public class SeedGeneticsTests
        {
            //Makes new child from 2 Seeds
            [Test]
            public void TwoParentsTest()
            {
                Seed seed1 = new Seed(122,1);
                Seed seed2 = new Seed(123,2);
                
                Seed child = seed1 + seed2;
                
                Assert.AreNotEqual(seed1.Value, child.Value);
                Assert.AreNotEqual(seed2.Value, child.Value);
            }
            
            //Makes new child from 5 Seeds
            [Test]
            public void FiveParentsTest()
            {
                Seed seed1 = new Seed(122,1);
                Seed seed2 = new Seed(123,2);
                Seed seed3 = new Seed(321,3);
                Seed seed4 = new Seed(2401,4);
                Seed seed5 = new Seed(08976,5);
                
                var child = Genetic.CreateChildSeed(new ISeed[]{seed1, seed2, seed3, seed4, seed5});
                
                Assert.That(child, Is.TypeOf<Seed>(),"Should be a Seed");
                
                var parentValues = new[] { seed1.Value, seed2.Value, seed3.Value, seed4.Value, seed5.Value };
                Assert.That(parentValues, Does.Not.Contain(child.Value.ToString()),"Should be different");
                
                var child2 = Genetic.CreateChildSeed(new ISeed[]{seed1, seed2, seed3, seed4, seed5});
                Assert.That(child2.Value, Is.EqualTo(child.Value),"Should be deterministic");
                
                Debug.Log(child.Value);
                Debug.Log(child2.Value);
            }
            
            //Makes new child from Seed and GlobalSeed
            [Test]
            public void Seed_and_GlobalSeed_asParents_Test()
            {
                Seed seed1 = new Seed(122,1);
                ISeed gs1 = new GlobalSeed(12222, "papa");

                Seed childExplicit = seed1 + gs1;
                Assert.AreNotEqual(seed1.Value, childExplicit.Value);
                Assert.AreNotEqual(gs1.GetBaseValue(), childExplicit.Value);
                
                gs1 = (GlobalSeed)gs1;
                
                var childImplicit = Genetic.CreateChildSeed(new[]{seed1, gs1});
                Assert.AreNotEqual(seed1.Value, childImplicit.Value);
                Assert.AreNotEqual(gs1.GetBaseValue(), childImplicit.Value);
            }
            
            //Makes new child from 5 Seeds with given ID
            [Test]
            public void FiveParentsTestWithId()
            {
                Seed seed1 = new Seed(122,1);
                Seed seed2 = new Seed(123,2);
                Seed seed3 = new Seed(321,3);
                Seed seed4 = new Seed(2401,4);
                Seed seed5 = new Seed(08976,5);
                
                var child = Genetic.CreateChildSeed(new ISeed[]{seed1, seed2, seed3, seed4, seed5},12);
                
                Assert.That(child.Id, Is.EqualTo(12),"Should be the given one");
            }
            
            // MutateSeed different from original
            [Test]
            public void DeterministicMutationTest()
            {
                Seed seed1 = new Seed(122,1);
                Seed mutation1 = Genetic.MutateSeed(seed1, MutateTypes.Deterministic, 0);
                Seed mutation2 = Genetic.MutateSeed(seed1, MutateTypes.Deterministic, 0);
                Seed mutation3 = Genetic.MutateSeed(seed1, MutateTypes.Deterministic, 0, new byte[] {0x20,0x20,0x20,0x00,0x00});
                
                Assert.AreNotEqual(seed1.Value, mutation1.Value,"Value after mutation needs to be different");
                Assert.AreEqual(mutation2.Value, mutation1.Value,"Value after mutation needs to be deterministic");
                Assert.AreNotEqual(mutation3.Value, mutation1.Value,"Different crossovers_map result in different outputs");
            }
            
            // HammingDistance Helpers Test
            private static int CallPrivateCalculateHammingDistance(byte[] a, byte[] b)
            {
                var method = typeof(Genetic).GetMethod("CalculateHammingDistance", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    
                if (method == null)
                    throw new Exception("Method not found");
    
                return (int)method.Invoke(null, new object[] { a, b });
            }
            
            [Test]
            public void HammingDistanceTest0()
            {
                // Arrange
                byte[] a = { 0x00, 0xFF, 0x55, 0xAA };
                byte[] b = { 0x00, 0xFF, 0x55, 0xAA };

                // Use reflection to call the private method
                int result0 = CallPrivateCalculateHammingDistance(a, b);
    
                Assert.AreEqual(0, result0); // Note: Use AreEqual, not Equals
            }
            
            [Test]
            public void HammingDistanceTest1()
            {
                // Arrange
                byte[] a = { 0x00 }; // 00000000
                byte[] b = { 0x01 }; // 00000001
        
                // Act
                int result = CallPrivateCalculateHammingDistance(a, b);
        
                // Assert
                Assert.AreEqual(1, result);
                
                // Arrange
                byte[] a2 = { 0xFF }; // 11111111
                byte[] b2 = { 0x00 }; // 00000000
        
                // Act
                result = CallPrivateCalculateHammingDistance(a2, b2);
        
                // Assert
                Assert.AreEqual(8, result); // All 8 bits are different
            }
            
            [Test]
            public void HammingDistanceTest2()
            {
                // Arrange
                byte[] a = { 0xFF, 0x00, 0xAA }; // 11111111, 00000000, 10101010
                byte[] b = { 0xFE, 0x01, 0xAB }; // 11111110, 00000001, 10101011
        
                // Act
                int result = CallPrivateCalculateHammingDistance(a, b);
        
                // Assert
                // 0xFF vs 0xFE: 1 bit difference
                // 0x00 vs 0x01: 1 bit difference  
                // 0xAA vs 0xAB: 1 bit difference
                Assert.AreEqual(3, result);
                
                // Arrange
                byte[] a2 = { 0b10101010, 0b11110000, 0b00001111 };
                byte[] b2 = { 0b10101011, 0b11110001, 0b00001110 };
        
                // Act
                result = CallPrivateCalculateHammingDistance(a2, b2);
        
                // Assert
                // 0b10101010 vs 0b10101011: 1 bit difference
                // 0b11110000 vs 0b11110001: 1 bit difference
                // 0b00001111 vs 0b00001110: 1 bit difference
                Assert.AreEqual(3, result);
            }
            
            // MutateSeed different from original With Hamming Distance
            [Test]
            public void CreateDistinctTest()
            {
                Seed seed1 = new Seed(122,1);
                Seed seed2 = new Seed(123,2);
                Seed seed3 = new Seed(321,3);
                
                var child = Genetic.CreateChildSeedDistinct(new ISeed[]{seed1, seed2, seed3});
                
                Assert.That(child, Is.TypeOf<Seed>(),"Should be a Seed");
                var parentValues = new[] { seed1.Value, seed2.Value, seed3.Value };
                Assert.That(parentValues, Does.Not.Contain(child.Value.ToString()),"Should be different");
            }
            
            // MutateSeed different from original with Hamming Distance and given ID
            [Test]
            public void CreateDistinctTestWithID()
            {
                Seed seed1 = new Seed(122,1);
                Seed seed2 = new Seed(123,2);
                Seed seed3 = new Seed(321,3);
                
                var child = Genetic.CreateChildSeedDistinct(new ISeed[]{seed1, seed2, seed3},23);
                
                Assert.That(child, Is.TypeOf<Seed>(),"Should be a Seed");
                var parentValues = new[] { seed1.Value, seed2.Value, seed3.Value };
                Assert.That(parentValues, Does.Not.Contain(child.Value.ToString()),"Should be different");
                
                Assert.That(child.Id, Is.EqualTo(23),"Should be given id");
            }
        }
}