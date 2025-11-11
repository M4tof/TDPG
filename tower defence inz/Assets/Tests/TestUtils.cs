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

        private static float? _performanceMultiplier;

        public static void InitializePerformanceScaling()
        {
            if (_performanceMultiplier.HasValue)
                return;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Simple synthetic CPU test
            double result = 0;
            for (int i = 0; i < 10_000_000; i++)
                result += System.Math.Sqrt(i);

            stopwatch.Stop();

            // Example reference time (baseline)
            double referenceCpuMs = 98.32; // Ryzen 5 5600X, RTX 3060 baseline
            double measuredTimeMs = stopwatch.Elapsed.TotalMilliseconds;

            _performanceMultiplier = (float)(referenceCpuMs / measuredTimeMs);

            // Scale expected times based on the multiplier (slow PC = smaller multiplier)
            ExpectedTimeToExecute = Mathf.RoundToInt(ExpectedTimeToExecute / _performanceMultiplier.Value);
            ExpectedTimeToExecuteLonger = Mathf.RoundToInt(ExpectedTimeToExecuteLonger / _performanceMultiplier.Value);

            Debug.Log($"[PerformanceEstimator] CPU multiplier: {_performanceMultiplier:F2}, adjusted short={ExpectedTimeToExecute}ms, long={ExpectedTimeToExecuteLonger}ms");
        }

        public static float GetPerformanceMultiplier()
        {
            if (!_performanceMultiplier.HasValue)
                InitializePerformanceScaling();

            return _performanceMultiplier.Value;
        }
        
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