using NUnit.Framework;
using TDPG.Generators.Seed;

namespace Tests.SeedTests
{
    [TestFixture]
    public class SeedTests
    {
        [Test]
        public void Seed_Creation_noParent()
        {
            // Arrange 
            var seed = new Seed(1234,0);

            // Act
            string extractedValue = seed.Id.ToString() + seed.Value.ToString();

            // Assert
            Assert.That(extractedValue, Is.EqualTo("01234"),
                "Basic seed creation successful");
        }
        
        [Test]
        public void Seed_Creation_Parent()
        {
            // Arrange 
            var seed = new Seed(12345,1,"ValueName");

            // Act
            string extractedValue = seed.Id.ToString() + seed.Value.ToString() + seed.ParentName;

            // Assert
            Assert.That(extractedValue, Is.EqualTo("112345ValueName"),
                "Seed creation with parent name successful");
        }

        [Test]
        public void Seed_ISeed()
        {
            // Arrange
            ISeed seed = new Seed(1234, 1, "ValueName");
            Seed seed2 = new Seed(1234, 1, "ValueName");
    
            // Assert
            Assert.AreEqual(seed.GetBaseValue(), seed2.GetBaseValue(),"Should hold same value");
        }
        
        
    }
}