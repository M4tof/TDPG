using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TDPG.EffectSystem.Element;

namespace Tests.EffectSystem.ElementTests
{
    public class ElementTests
    {
        
        [Test]
        public void Element_Constructor_AssignsNameAndId()
        {
            var element = new Element("Fire", 1);
            Assert.AreEqual("Fire", element.Name);
            Assert.AreEqual(1, element.Id);
        }
        
        [Test]
        public void Element_MetaDataTest()
        {
            var element = new Element("Thunder", 3);
            element.AddMetaData("Created: NOW!");
            Assert.AreEqual("Created: NOW!", element.MetaData[0]);
        }
        
        [Test]
        public void Element_AddsAndRemovesEffectsCorrectly()
        {
            var element = new Element("Ice", 2);
            var slow = new SlowDown(0.25f);

            element.AddEffect(slow);
 
            Assert.Contains(slow, element.Effects.ToList());

            element.RemoveEffect(slow);
            Assert.IsFalse(element.Effects.Contains(slow));
        }

        [Test]
        public void Element_bigConstructor()
        {
            var slow = new SlowDown(0.25f);
            var burn = new HealthDown(1.2f);
            List<Effect> effects = new List<Effect>(){slow, burn};
            
            var element = new Element("Fire", 1, effects);
            
            Assert.AreEqual("Fire", element.Name);
            Assert.AreEqual(1, element.Id);
        
            Assert.Contains(slow, element.Effects.ToList());
            Assert.Contains(burn, element.Effects.ToList());
        }
        
    }
}