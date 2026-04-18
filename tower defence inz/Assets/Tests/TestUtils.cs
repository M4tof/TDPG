using System;
using TDPG.EffectSystem.ElementLogic;
using TDPG.Generators.Seed;
using UnityEngine;

namespace Tests
{
    public static class TestUtils
    {
        public static int ExpectedTimeToExecute = 20; // ms
        public static int ExpectedTimeToExecuteLonger = 1120; // ms
        
        // HammingDistance Helpers Test
        public static int CallPrivateCalculateHammingDistance(byte[] a, byte[] b)
        {
            var method = typeof(Genetic).GetMethod("CalculateHammingDistance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    
            if (method == null)
                throw new Exception("Method not found");
    
            return (int)method.Invoke(null, new object[] { a, b });
        }
        
        /// <summary>
        /// Helper to reflectively access protected float[] Values in tests
        /// </summary>
        public static float[] GetPrivateValues_Effect(Effect effect)
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