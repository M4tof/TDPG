using NUnit.Framework;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TDPG.EffectSystem.ElementLogic;
using UnityEngine;
using Formatting = System.Xml.Formatting;

namespace Tests.EffectSystem.ElementTests
{
    /*[TestFixture, Category("EffectSystemTest")]
    public class EffectTests
    {
        [Test]
        public void Heal_Constructor_SetsPropertiesCorrectly()
        {
            var heal = new Heal(10f);

            Assert.AreEqual("Heal", heal.Name);
            Assert.IsTrue(heal.Description.Contains("Heals the target"));
            Assert.AreEqual(10f, GetPrivateValues(heal)[0]);
        }

        [Test]
        public void HealthDown_Constructor_SetsPropertiesCorrectly()
        {
            var healthDown = new HealthDown(5f);

            Assert.AreEqual("HealthDown", healthDown.Name);
            Assert.IsTrue(healthDown.Description.Contains("Lowers health of the target"));
            Assert.AreEqual(5f, GetPrivateValues(healthDown)[0], "Should be change by 5hp (- is implicit by name 'health DOWN'");

            var logic = healthDown.LogicTransfer();
            Assert.IsTrue(logic.ContainsKey(EffectParameter.HealthChange));
            Assert.IsTrue(logic.ContainsValue(-5f), "Should transfer logic as HealthChange: -5");
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
        public void Effect_SerializeDeserialize_SerializesCorrectly()
        {
            // Arrange
            var settings = new JsonSerializerSettings
            {
                Formatting = (Newtonsoft.Json.Formatting)Formatting.Indented,
                Converters = new List<JsonConverter> { new EffectConverter() }
            };

            Effect original = new SlowDown(0.5f, 3f);

            // Act: Serialize
            string json = JsonConvert.SerializeObject(original, settings);
            Debug.Log($"Serialized JSON:\n{json}");

            // Act: Deserialize
            Effect restored = JsonConvert.DeserializeObject<Effect>(json, settings);
            Debug.Log($"Restored: {restored.Name} ({restored.GetType().Name})");
            
            // Assert
            Assert.IsNotNull(restored, "Deserialized effect should not be null.");
            Assert.AreEqual(original.GetType(), restored.GetType(), "Deserialized type should match original type.");
            Assert.AreEqual(original.Name, restored.Name, "Name should match.");
            Assert.AreEqual(original.Description, restored.Description, "Description should match.");

            float[] originalValues = original.GetValues();
            float[] restoredValues = restored.GetValues();

            Assert.AreEqual(originalValues.Length, restoredValues.Length, "Values length should match.");
            for (int i = 0; i < originalValues.Length; i++)
                Assert.AreEqual(originalValues[i], restoredValues[i], 0.0001f, $"Value {i} should match.");

            // sanity check on LogicTransfer
            var origLogic = original.LogicTransfer();
            var restoredLogic = restored.LogicTransfer();
            Assert.AreEqual(origLogic.Count, restoredLogic.Count, "LogicTransfer should have same param count.");

            foreach (var kvp in origLogic)
                Assert.AreEqual(kvp.Value, restoredLogic[kvp.Key], 0.0001f, $"LogicTransfer param {kvp.Key} should match.");
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
    }*/
}
