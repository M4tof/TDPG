using System;
using System.Linq;
using NUnit.Framework;
using TDPG.Generators.Seed;
using UnityEngine;

namespace Tests.GeneratorTests.SeedTests
{
    [TestFixture, Category("SeedTests")]
    public class SeedTests
    {
        [Test]
        public void Seed_Creation_noParent()
        {
            // Arrange 
            var seed = new Seed(1234,0);

            // Act
            string extractedValue = seed.Id.ToString() + seed.Value.ToString();

            // Assert
            Assert.That(extractedValue, Is.EqualTo("01234"),
                "Basic seed creation successful");
        }
        
        [Test]
        public void Seed_Creation_Parent()
        {
            // Arrange 
            var seed = new Seed(12345,1,"ValueName");

            // Act
            string extractedValue = seed.Id.ToString() + seed.Value.ToString() + seed.ParentName;

            // Assert
            Assert.That(extractedValue, Is.EqualTo("112345ValueName"),
                "Seed creation with parent name successful");
        }

        [Test]
        public void Seed_ISeed()
        {
            // Arrange
            ISeed seed = new Seed(1234, 1, "ValueName");
            Seed seed2 = new Seed(1234, 1, "ValueName");
    
            // Assert
            Assert.AreEqual(seed.GetBaseValue(), seed2.GetBaseValue(),"Should hold same value");
        }
        
        [Test]
        public void Seed_Addition()
        {
            // Arrange
            byte[] aBytes = { 0b_10101010, 0b_00001111, 0b_11110000, 0b_01010101, 0, 0, 0, 0 };
            byte[] bBytes = { 0b_11110000, 0b_00110011, 0b_00001111, 0b_11001100, 0, 0, 0, 0 };
            // or should be FA 3F FF DD 00 00 00 00

            ulong aValue = BitConverter.ToUInt64(aBytes, 0);
            ulong bValue = BitConverter.ToUInt64(bBytes, 0);

            Seed seedA = new Seed(aValue, id: 1, parentName: "A");
            Seed seedB = new Seed(bValue, id: 2, parentName: "B");

            // Act
            Seed result = seedA + seedB;

            // Compute expected OR manually
            byte[] expectedBytes = new byte[aBytes.Length];
            for (int i = 0; i < aBytes.Length; i++)
                expectedBytes[i] = (byte)(aBytes[i] | bBytes[i]);
            
            Debug.Log(BitConverter.ToString(expectedBytes));
            
            ulong expectedValue = BitConverter.ToUInt64(expectedBytes, 0);

            // Assert
            Assert.AreEqual(expectedValue, result.GetBaseValue(), 
                "Result value should be OR of both seeds.");

            StringAssert.Contains("A", result.GetName());
            StringAssert.Contains("B", result.GetName());
            Assert.AreEqual(-1, result.Id);
        }
        
        [Test]
        public void Seed_Multiplication()
        {
            // Arrange
            byte[] aBytes = { 0b_10101010, 0b_00001111, 0b_11110000, 0b_01010101, 0, 0, 0, 0 };
            byte[] bBytes = { 0b_11110000, 0b_00110011, 0b_00001111, 0b_11001100, 0, 0, 0, 0 };
            // xor 01011010 00111100 11111111 10011001 00000000 00000000 00000000 00000000 [5A 3C FF 99 00 00 00 00]
            //0b_10000000
            // shift [0x16, 0x8F, 0x3F, 0xE6, 0x40, 0x00, 0x00, 0x00]

            ulong aValue = BitConverter.ToUInt64(aBytes, 0);
            ulong bValue = BitConverter.ToUInt64(bBytes, 0);

            Seed seedA = new Seed(aValue, id: 1, parentName: "A");
            Seed seedB = new Seed(bValue, id: 2, parentName: "B");

            // Act
            Seed result = seedA * seedB;

            // Reproduce ByteCrossover logic for expected value 
            int maxLength = Math.Max(aBytes.Length, bBytes.Length);
            byte[] valueXor = Enumerable.Range(0, maxLength)
                .Select(i => (byte)
                    ((i < aBytes.Length ? aBytes[i] : 0) ^
                     (i < bBytes.Length ? bBytes[i] : 0)))
                .ToArray();

            byte[] finalValue = new byte[valueXor.Length];
            byte carryOver = (byte)(valueXor[^1] << 6);

            for (int i = 0; i < valueXor.Length; i++)
            {
                byte shifted = (byte)(valueXor[i] >> 2);
                if (i > 0)
                {
                    shifted |= (byte)(valueXor[i - 1] << 6);
                }
                finalValue[i] = shifted;
            }

            finalValue[0] |= carryOver;
            
            Debug.Log(BitConverter.ToString(finalValue));

            byte[] resultBytes = new byte[8];
            Array.Copy(finalValue, resultBytes, Math.Min(finalValue.Length, 8));
            ulong expectedValue = BitConverter.ToUInt64(resultBytes, 0);

            // Assert 
            Assert.AreEqual(expectedValue, result.GetBaseValue(),
                $"Expected multiplication (XOR + shift) result to match computed value. Expected: {expectedValue:X}, Got: {result.GetBaseValue():X}");

            StringAssert.Contains("A", result.GetName());
            StringAssert.Contains("B", result.GetName());
            Assert.AreEqual(-1, result.Id);
        }

        [Test]
        public void Seed_Normalize_Test()
        {
            Seed seed = new Seed(1234, 1, "ValueName", true);
            Seed reference = new Seed(1234, 1, "ValueName", false);
            
            Assert.AreEqual(seed.GetBaseValue(), reference.GetBaseValue());
            seed.NormalizeSeedValue(); //Should do nothing since seed isBitBased 
            Assert.AreEqual(seed.GetBaseValue(), reference.GetBaseValue());
            
            reference.NormalizeSeedValue(); // should normalize value to 1234000000
            Assert.AreNotEqual(seed.GetBaseValue(), reference.GetBaseValue());
            Debug.Log(reference.GetBaseValue());
            Assert.That(reference.GetBaseValue() == 123_400_000);
            
        }

        [Test]
        public void Seed_Bitwise_Test()
        {
            ulong seedValue = 0b_0000_0000_0000_0000_0000_0000_0000_0111UL;
            Seed seed = new Seed(seedValue, -1, "BitwiseTest");
            
            Debug.Log(seed.GetBaseValue());
            Assert.That(seed.GetBaseValue() == 7);
            
            seedValue = 0b_0000_0000_0000_0000_0000_0000_0001_1111UL;
            Assert.That(seedValue == 31);
        }

        [Test]
        public void Seed_NotBitwise_Normalized()
        {
            Seed seed = new Seed(24011, -1,"missingSeedInMapGen",false);
            seed.IsBitBased =  false;
            seed.NormalizeSeedValue();
            ulong seedVal = seed.GetBaseValue();
            string seedStr = seedVal.ToString();
            
            Assert.That(seedStr == "240110000", "Int based seed should normalize to longer form");
        }
        
    }
}