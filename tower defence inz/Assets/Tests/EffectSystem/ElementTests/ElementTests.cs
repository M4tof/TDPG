using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using TDPG.EffectSystem.ElementLogic;
using TDPG.Generators.Seed;
using UnityEngine;

namespace Tests.EffectSystem.ElementTests
{
    [TestFixture, NUnit.Framework.Category("EffectSystemTest")]
    public class ElementTests
    {
        [Test]
        public void Element_Constructor_AssignsNameAndId()
        {
            Seed simpleSeed = new Seed(0UL,1);
            var element = new Element("Fire", 1, simpleSeed);
            
            Assert.AreEqual("Fire", element.Name);
            Assert.AreEqual(1, element.Id);
            Assert.IsEmpty(element.GetEffects(), "Seed 0b_0000000000000000000... means empty effect list");
        }
        
        [Test]
        public void Element_MetaDataTest()
        {
            Seed simpleSeed = new Seed(123,1);
            var element = new Element("Thunder", 3,simpleSeed);
            element.AddMetaData("Created: NOW!");
            
            Assert.AreEqual("Created: NOW!", element.MetaData[1]);
            Assert.AreEqual(2, element.MetaData.Count);
            
            Assert.AreEqual("Value: 123, Id: 1, Parent: ", element.MetaData[0]);
        }

        [Test]
        public void Element_Constructor_WithEffectList_AssignsEffects()
        {
            var slow = new SlowDown(0.25f);
            var burn = new HealthDown(1.2f);
            var effects = new List<Effect> { slow, burn };

            var element = new Element("Fire", 1, effects);

            Assert.AreEqual("Fire", element.Name);
            Assert.AreEqual(1, element.Id);
            var stored = element.GetEffects();
            Assert.Contains(slow, stored.ToList());
            Assert.Contains(burn, stored.ToList());
        }

        /*[Test]
        public void Element_Effects_HaveValidLogicTransfers_WhenCreatedFromList()
        {
            var effects = new List<Effect>
            {
                new Heal(10f),
                new HealthDown(5f),
                new SlowDown(0.2f, 4f)
            };

            var element = new Element("Arcane", 42, effects);

            var storedEffects = element.GetEffects();
            Assert.AreEqual(3, storedEffects.Count);

            foreach (var effect in storedEffects)
            {
                var data = effect.LogicTransfer();
                Assert.IsNotNull(data, $"{effect.Name} LogicTransfer() returned null");
                Assert.IsNotEmpty(data, $"{effect.Name} LogicTransfer() returned empty");

                foreach (var kvp in data)
                {
                    Assert.IsTrue(Enum.IsDefined(typeof(EffectParameter), kvp.Key),
                        $"Invalid key '{kvp.Key}' for {effect.Name}");
                    Assert.IsInstanceOf<float>(kvp.Value);
                }
            }
        }
        
        [Test]
        public void Element_SeedBitPattern_CreatesExpectedEffects()
        {
            // EffectRegistry:
            // bit 0 => SlowDown
            // bit 1 => HealthDown
            // bit 2 => Heal

            // Set bits 1, 2, and 0 => 0b0111 (bit 0 to 2)
            ulong seedValue = 0b_0000_0000_0000_0000_0000_0000_0000_0111UL;
            Seed seed = new Seed(seedValue, -1, "BitPatternTest");

            // Values used by constructors when bits are on
            float[] values = { 0.5f, 2f, 10f };

            var element = new Element("BitTest", -1, seed, values);

            var effects = element.GetEffects();

            // Verify the right types are created
            Assert.AreEqual(3, effects.Count, "Expected 3 effects for bits 1,2,3 set.");

            Assert.IsTrue(effects.Any(e => e is SlowDown), "Expected SlowDown effect for bit 1.");
            Assert.IsTrue(effects.Any(e => e is HealthDown), "Expected HealthDown effect for bit 2.");
            Assert.IsTrue(effects.Any(e => e is Heal), "Expected Heal effect for bit 3.");
        }
        
        [Test]
        public void Element_SerializeDeserialize_PreservesState()
        {
            var effects = new List<Effect>
            {
                new SlowDown(0.8f, 5f),
                new HealthDown(20f)
            };

            var element = new Element("Ice", 2, effects);
            element.AddMetaData("Cold");

            string json = element.Serialize();
            Debug.Log(json);

            var restored = Element.Deserialize(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(element.Name, restored.Name);
            Assert.AreEqual(element.Id, restored.Id);
            Assert.AreEqual(element.GetEffects().Count, restored.GetEffects().Count);
            Assert.AreEqual(element.MetaData.Count, restored.MetaData.Count);
            Assert.AreEqual(element.GetDna().Value, restored.GetDna().Value);
        }
        
        [Test]
        public void Element_Constructor_WithEffectList_AssignsEffects()
        {
            var slow = new SlowDown(0.25f, 3f);
            var burn = new HealthDown(1.2f);
            var effects = new List<Effect> { slow, burn };

            var element = new Element("Fire", 1, effects);

            Assert.AreEqual("Fire", element.Name);
            Assert.AreEqual(1, element.Id);
            var stored = element.GetEffects();
            Assert.Contains(slow, stored.ToList());
            Assert.Contains(burn, stored.ToList());
        }
        
        [Test]
        public void Element_ConstructorTests_SeedAndEffectSymmetry()
        {
            var slow = new SlowDown(0.25f, 3f);
            var burn = new HealthDown(1.2f);
            var heal = new Heal(10f);

            List<Effect> effects1 = new List<Effect> { slow, burn, heal };
            List<Effect> effects2 = new List<Effect> { heal };

            // Create from effects
            Element fromList1 = new Element("Combo", 1, effects1);
            Element fromList2 = new Element("SingleHeal", 2, effects2);

            // Create from seeds reconstructed from those lists
            Seed seed1 = fromList1.GetDna();
            Seed seed2 = fromList2.GetDna();

            Element fromSeed1 = new Element("Combo", 3, seed1, 0.25f, 3f, 1.2f, 10f);
            Element fromSeed2 = new Element("SingleHeal", 4, seed2, 10f);

            // 1 ↔ 3 should match
            Assert.AreEqual(fromList1.GetDna().ToString(), fromSeed1.GetDna().ToString());
            Assert.AreEqual(fromList1.GetEffects().Count, fromSeed1.GetEffects().Count);

            // 2 ↔ 4 should match
            Assert.AreEqual(fromList2.GetDna().ToString(), fromSeed2.GetDna().ToString());
            Assert.AreEqual(fromList2.GetEffects().Count, fromSeed2.GetEffects().Count);

            // 1 and 2 should differ
            Assert.AreNotEqual(fromList1.GetDna().ToString(), fromList2.GetDna().ToString());
            Assert.AreNotEqual(fromList1.GetEffects().Count, fromList2.GetEffects().Count);
        }
        
        [Test]
        public void Element_Rename_ReturnsOldNameAndChangesName()
        {
            var seed = new Seed(321, 1, "RenameSeed");
            var element = new Element("OldName", 5, seed);

            string oldName = element.ReNameElement("NewName");

            Assert.AreEqual("OldName", oldName);
            Assert.AreEqual("NewName", element.Name);
        }
        
        [Test]
        public void Element_FromEffects_SetsExpectedBitsInSeed()
        {
            // Create effects matching bits 0, 1, 2
            var effects = new List<Effect>
            {
                new SlowDown(0.25f, 1.5f),
                new HealthDown(10f),
                new Heal(5f)
            };

            var element = new Element("ReverseBitTest", 88, effects);
            Seed dna = element.GetDna();

            ulong expectedBits = (1UL << 0) | (1UL << 1) | (1UL << 2);
            ulong actualValue = dna.Value;

            Assert.AreEqual(expectedBits, actualValue,
                $"Expected seed bits {Convert.ToString((long)expectedBits, 2)} but got {Convert.ToString((long)actualValue, 2)}");
        }

        [Test]
        public void Element_FromPartialBits_CreatesPartialEffects()
        {
            // Only set bits for HealthDown (1) and Heal (2)
            ulong seedValue = (1UL << 1) | (1UL << 2);
            Seed seed = new Seed(seedValue, 7, "PartialBitTest");

            var element = new Element("PartialTest", 7, seed, 0.5f, 10f);
            var effects = element.GetEffects();

            Assert.AreEqual(2, effects.Count, "Expected only 2 effects for bits 1 and 2.");

            Assert.IsTrue(effects.Any(e => e is HealthDown), "Expected HealthDown from bit 1.");
            Assert.IsTrue(effects.Any(e => e is Heal), "Expected Heal from bit 2.");
            Assert.IsFalse(effects.Any(e => e is SlowDown), "Did not expect SlowDown (bit 0 not set).");
        }
        
        [Test]
        public void Element_Factory_SafeWithEmptyOrShortValues()
        {
            // Empty array should not throw and should use defaults
            Assert.DoesNotThrow(() => Element.EffectFactories[0](Array.Empty<float>()));
            Assert.DoesNotThrow(() => Element.EffectFactories[1](new float[] { 0.5f }));
            Assert.DoesNotThrow(() => Element.EffectFactories[2](null));

            var slow = Element.EffectFactories[0](new float[] { 0.5f });
            Assert.IsInstanceOf<SlowDown>(slow);
        }
        
        [Test]
        public void Element_Factory_RepeatValuesPattern()
        {
            float[] input = { 2f };
            var normalized = typeof(Element)
                .GetMethod("NormalizeValues", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { input, 4 }) as float[];

            Assert.AreEqual(new float[] { 2f, 2f, 2f, 2f }, normalized);
        }
        
        [Test]
        public void Element_SeedToEffectAndBack_RoundTripIntegrity()
        {
            var seed = new Seed((1UL << 0) | (1UL << 2), 42, "RoundTrip");
            var element = new Element("RoundTrip", 1, seed, 0.25f, 2f, 10f);

            var effects = element.GetEffects();
            Assert.AreEqual(2, effects.Count);

            var reverse = new Element("RoundTrip2", 2, effects);
            Assert.AreEqual(seed.Value, reverse.GetDna().Value);
        }
        
        [Test]
        public void Element_Factory_DefaultValuesWorkWithNoInput()
        {
            // All effects created with no input should not crash and have numeric defaults
            foreach (var factory in Element.EffectFactories.Values)
            {
                var e = factory(Array.Empty<float>());
                Assert.IsNotNull(e);
            }
        }
        
        [Test]
        public void Element_Factory_And_Types_AreSymmetric()
        {
            foreach (var (bit, factory) in Element.EffectFactories)
            {
                var effect = factory(new float[] { 0.5f, 1f });
                Assert.AreEqual(Element.EffectTypes[bit], effect.GetType(),
                    $"Effect type mismatch for bit {bit}");
            }
        }
        
        [Test]
        public void Element_SeedBitAlignment_IsConsistentWithFactories()
        {
            ulong seedValue = 0;
            foreach (var key in Element.EffectFactories.Keys)
                seedValue |= (1UL << key);

            var seed = new Seed(seedValue, 99, "AlignmentTest");
            var element = new Element("Align", 99, seed, 0.5f, 1f, 10f);
            Assert.AreEqual(Element.EffectFactories.Count, element.GetEffects().Count,
                "Each factory bit should produce exactly one effect.");
        }*/


    }
}
