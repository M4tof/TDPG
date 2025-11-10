using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TDPG.EffectSystem.ElementLogic;
using TDPG.Generators.Seed;
using UnityEngine;

namespace Tests.EffectSystem.ElementTests
{
    public class ElementTests
    {
        [Test]
        public void Element_Constructor_AssignsNameAndId()
        {
            Seed simpleSeed = new Seed(123,1);
            var element = new Element("Fire", 1,simpleSeed);
            Assert.AreEqual("Fire", element.Name);
            Assert.AreEqual(1, element.Id);
            Assert.IsEmpty(element.GetEffects());
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
        public void Element_AddsAndRemovesEffectsCorrectly()
        {
            Seed simpleSeed = new Seed(123,1);
            var element = new Element("Ice", 2,simpleSeed);
            var slow = new SlowDown(0.25f, 2f);

            element.AddEffect(slow);
            var effects = element.GetEffects();

            Assert.Contains(slow, effects.ToList());

            element.RemoveEffect(slow);
            Assert.IsFalse(element.GetEffects().Contains(slow));
        }

        [Test]
        public void Element_Constructor_WithEffectList_AssignsEffects()
        {
            Seed simpleSeed = new Seed(123,1);
            var slow = new SlowDown(0.25f, 3f);
            var burn = new HealthDown(1.2f);
            var effects = new List<Effect> { slow, burn };
            
            var element = new Element("Fire", 1, effects, simpleSeed);
            
            Assert.AreEqual("Fire", element.Name);
            Assert.AreEqual(1, element.Id);
        
            var stored = element.GetEffects();
            Assert.Contains(slow, stored.ToList());
            Assert.Contains(burn, stored.ToList());
        }

        [Test]
        public void Element_CanStoreAndApplyEffects()
        {
            Seed simpleSeed = new Seed(123,1);
            var dummyTarget = new GameObject("Dummy");
            var element = new Element("Fire", 1,simpleSeed);
            var burn = new HealthDown(5f);
            var heal = new Heal(10f);
            var slow = new SlowDown(0.25f, 2f);

            element.AddEffect(burn);
            element.AddEffect(heal);
            element.AddEffect(slow);

            Assert.AreEqual(3, element.GetEffects().Count);

            // Should apply all effects without error
            Assert.DoesNotThrow(() => element.ApplyEffects(dummyTarget));
        }

        [Test]
        public void Element_CanHoldMultipleEffectsWithUniqueLogicTransfers()
        {
            Seed simpleSeed = new Seed(123,1);
            var element = new Element("Arcane", 42,simpleSeed);
            element.AddEffect(new Heal(10f));
            element.AddEffect(new HealthDown(5f));
            element.AddEffect(new SlowDown(0.2f, 4f));

            var effects = element.GetEffects();
            Assert.AreEqual(3, effects.Count);

            // Each effect should have a valid LogicTransfer dictionary
            foreach (var effect in effects)
            {
                var data = effect.LogicTransfer();

                Assert.IsNotNull(data, $"LogicTransfer() returned null for {effect.Name}");
                Assert.IsNotEmpty(data, $"LogicTransfer() returned empty data for {effect.Name}");

                foreach (var kvp in data)
                {
                    // Ensure the key is a valid enum and value is a float
                    Assert.IsTrue(System.Enum.IsDefined(typeof(EffectParameter), kvp.Key),
                        $"Invalid EffectParameter key '{kvp.Key}' in effect {effect.Name}");
                    Assert.IsInstanceOf<float>(kvp.Value, $"Value for {kvp.Key} is not a float");
                }
            }
        }
        
        
        
        
    }
}
