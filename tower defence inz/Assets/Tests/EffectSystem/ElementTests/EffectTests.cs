using NUnit.Framework;
using System.Collections.Generic;
using TDPG.EffectSystem.ElementLogic;
using UnityEngine;

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
            Assert.AreEqual(10f, GetPrivateValues(heal)[0]);
        }

        [Test]
        public void HealthDown_Constructor_SetsPropertiesCorrectly()
        {
            var healthDown = new HealthDown(5f);

            Assert.AreEqual("HealthDown", healthDown.Name);
            Assert.IsTrue(healthDown.Description.Contains("Lower health"));
            Assert.AreEqual(5f, GetPrivateValues(healthDown)[0], "Should be change by 5hp (- is implicit by name 'health DOWN'"); 
        }

        [Test]
        public void SlowDown_Constructor_SetsPropertiesCorrectly()
        {
            var slow = new SlowDown(0.3f, 2f);

            Assert.AreEqual("SlowDown", slow.Name);
            StringAssert.Contains("30%", slow.Description); // because factor * 100
            StringAssert.Contains("2", slow.Description);

            float[] values = GetPrivateValues(slow);
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

        /// <summary>
        /// Helper to reflectively access protected float[] Values in tests
        /// </summary>
        private static float[] GetPrivateValues(Effect effect)
        {
            var type = typeof(Effect);

            // Try to get it as a field first
            var fieldInfo = type.GetField("Values",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (fieldInfo != null)
                return (float[])fieldInfo.GetValue(effect);

            // If it's a property, handle that
            var propInfo = type.GetProperty("Values",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            return propInfo != null ? (float[])propInfo.GetValue(effect) : null;
        }
    }
}
