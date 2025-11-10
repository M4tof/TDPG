using System;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace Tests
{
    [SetUpFixture]
    public class BaseTest
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            TestUtils.InitializePerformanceScaling();
        }
        
    }
}