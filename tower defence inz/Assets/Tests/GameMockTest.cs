using System;
using NUnit.Framework;
using TDPG.Generators.Seed;
using UnityEngine;
using static TDPG.Generators.Scalars.InitializerFromDate;

namespace Tests
{
    [TestFixture, Category("IntegrationTest")]
    public class GameMockTest
    {
        private static GlobalSeed gs;
        private static GlobalSeed gs2;
        private static GlobalSeed gsLoaded;
        private static string key;

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            // ---------- 1. CREATE SAVE GAME ----------
            var initVal = QuickGenerate(1);
            gs = new GlobalSeed(initVal, "testGS", "testDescription");
            string savePoint1 = gs.Serialize();

            key = DateTime.Now.Ticks.ToString();
            // ---------- 2. CREATE ANOTHER GAME (Different values) ----------
            var initVal2 = QuickGenerate(2);
            gs2 = new GlobalSeed(initVal2, "testGS", "testDescription");
            string key2 = DateTime.Now.Ticks.ToString();

            // ---------- 3. LOAD SAVE GAME ----------
            gsLoaded = GlobalSeed.Deserialize(savePoint1);

            Debug.Log("Global mock setup complete. Seed state initialized.");
        }

        [Test]
        public void FirstNewSeed_Test()
        {
            var from1 = gs.NextSubSeed(key);
            var from2 = gs2.NextSubSeed(key);
            var fromLoaded = gsLoaded.NextSubSeed(key);
            
            Assert.That(from1.ToString(), Is.EqualTo(fromLoaded.ToString()));
            Assert.That(from1.ToString(), Is.Not.EqualTo(from2.ToString()));
        }
        
        
    }
}