using System;
using NUnit.Framework;
using TDPG.Generators.Seed;
using UnityEngine;
using static TDPG.Generators.Scalars.InitializerFromDate;

namespace Tests.GeneratorTests
{
    public class IntegrateScalarGenWithSeedTests
    {
        [Test]
        public void CreateSeedWithInitializer()
        {
            // Arrange
            var initVal = QuickGenerate();
            var gs = new GlobalSeed(initVal,"testGS","testDescription");
            string key = DateTime.Now.Ticks.ToString();
            gs.NextSubSeed(key);
             
            // Assert
            Assert.That(gs.GetBaseValue(),Is.EqualTo(initVal),"Global seed created properly");
            Assert.IsInstanceOf(typeof(Seed),gs.GetSubSeed(0));
             
            Debug.Log($"{gs.GetSubSeed(0).GetName()} , {gs.GetSubSeed(0).Id} , {gs.GetSubSeed(0).GetBaseValue()} ");
        }
        
        
        
    }
    
}