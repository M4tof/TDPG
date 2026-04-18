using System;
using NUnit.Framework;
using TDPG.Generators.Seed;
using UnityEngine;
using static TDPG.Generators.Scalars.InitializerFromDate;

namespace Tests.GeneratorTests
{
    [TestFixture, Category("IntegrationTest")]
    public class IntegrateScalarGenWithSeedTests
    {
        [Test]
        public void CreateSeedWithInitializer()
        {
            // Arrange
            var initVal = QuickGenerate(1);
            var gs = new GlobalSeed(initVal,"testGS","testDescription");
            string key = DateTime.Now.Ticks.ToString();
            
            // Assert
            Assert.That(gs.GetBaseValue(),Is.EqualTo(initVal),"Global seed created properly");
            
            gs.NextSubSeed(key);
             
            // Assert after
            Assert.That(gs.GetBaseValue(),Is.Not.EqualTo(initVal),"Global seed created properly");
            Assert.IsInstanceOf(typeof(Seed),gs.GetSubSeed(0));
             
            Debug.Log($"{gs.GetSubSeed(0).GetName()} , {gs.GetSubSeed(0).Id} , {gs.GetSubSeed(0).GetBaseValue()} ");
        }
        
        
        
    }
    
}