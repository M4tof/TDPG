using NUnit.Framework;
using System.Collections.Generic;
using TDPG.EffectSystem.Element;
using UnityEngine;
using static Tests.TestUtils;

namespace Tests.EffectSystem.ElementTests
{
    public class EffectTests
    {
        [Test]
        public void Heal_Constructor_SetsPropertiesCorrectly()
        {
            var heal = new Heal(10f);

            Assert.AreEqual("Heal", heal.Name);
            Assert.IsTrue(heal.Description.Contains("Heal the target"));
            Assert.AreEqual(10f, GetPrivateValues_Effect(heal)[0]);
        }

        [Test]
        public void HealthDown_Constructor_SetsPropertiesCorrectly()
        {
            var healthDown = new HealthDown(5f);

            Assert.AreEqual("HealthDown", healthDown.Name);
            Assert.IsTrue(healthDown.Description.Contains("Lower health"));
            Assert.AreEqual(5f, GetPrivateValues_Effect(healthDown)[0], "Should be change by 5hp (- is implicit by name 'health DOWN'"); 
        }

        [Test]
        public void SlowDown_Constructor_SetsPropertiesCorrectly()
        {
            var slow = new SlowDown(0.3f, 2f);

            Assert.AreEqual("SlowDown", slow.Name);
            StringAssert.Contains("30%", slow.Description); // because factor * 100
            StringAssert.Contains("2", slow.Description);

            float[] values = GetPrivateValues_Effect(slow);
            Assert.AreEqual(0.3f, values[0]);
            Assert.AreEqual(2f, values[1]);
        }

        [Test]
        public void LogicTransfer_ReturnsCorrectKeyValuePairs()
        {
            // Heal test
            var heal = new Heal(10f);
            Dictionary<EffectParameter, float> healData = heal.LogicTransfer();

            Assert.IsTrue(healData.ContainsKey(EffectParameter.HealthChange));
            Assert.AreEqual(10f, healData[EffectParameter.HealthChange]);

            // HealthDown test
            var burn = new HealthDown(5f);
            Dictionary<EffectParameter, float> burnData = burn.LogicTransfer();

            Assert.IsTrue(burnData.ContainsKey(EffectParameter.HealthChange));
            Assert.AreEqual(-5f, burnData[EffectParameter.HealthChange]);

            // SlowDown test
            var slow = new SlowDown(0.25f, 3f);
            Dictionary<EffectParameter, float> slowData = slow.LogicTransfer();

            Assert.IsTrue(slowData.ContainsKey(EffectParameter.SlowdownFactor));
            Assert.IsTrue(slowData.ContainsKey(EffectParameter.Duration));

            Assert.AreEqual(0.25f, slowData[EffectParameter.SlowdownFactor]);
            Assert.AreEqual(3f, slowData[EffectParameter.Duration]);
        }

        [Test]
        public void Effects_CanBeAppliedWithoutError()
        {   //fake test until they actually do apply to an target
            var dummyTarget = new GameObject("Dummy");
            var burn = new HealthDown(5f);
            var heal = new Heal(10f);
            var slow = new SlowDown(0.25f, 2f);

            Assert.DoesNotThrow(() => burn.Apply(dummyTarget));
            Assert.DoesNotThrow(() => heal.Apply(dummyTarget));
            Assert.DoesNotThrow(() => slow.Apply(dummyTarget));
        }
        
    }
}
