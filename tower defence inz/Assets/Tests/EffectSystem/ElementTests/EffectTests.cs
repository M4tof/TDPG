using NUnit.Framework;
using TDPG.EffectSystem.Element;
using UnityEngine;
using Object = System.Object;

namespace Tests.EffectSystem.ElementTests
{
    public class EffectTests
    {
        [Test]
        public void Effect_Constructor()
        {
            var heal = new Heal(10f);
            Assert.AreEqual("Heal", heal.Name);
            Assert.AreEqual(10f, heal.Value);
            StringAssert.Contains("Heal the target", heal.Description);
        }
        
        [Test]
        public void Effect_Description()
        {
            var slow = new SlowDown(0.3f);
            StringAssert.Contains("0.3", slow.Description);
        }
        
        [Test]
        public void Effects_CanBeAppliedWithoutError()
        {   //This is a test that only make sense until we have actually applicable effects
            var dummyTarget = new GameObject("Dummy");
            var burn = new HealthDown(5f);
            var heal = new Heal(10f);
            var slow = new SlowDown(0.25f);

            // Just ensure Apply() doesn’t throw exceptions (since they’re fake for now)
            Assert.DoesNotThrow(() => burn.Apply(dummyTarget));
            Assert.DoesNotThrow(() => heal.Apply(dummyTarget));
            Assert.DoesNotThrow(() => slow.Apply(dummyTarget));
            
        }
        
        
    }
}