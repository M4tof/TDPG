using System.IO;
using NUnit.Framework;
using TDPG.Generators.Seed;
using TDPG.TextGeneration;
using UnityEngine;

namespace Tests.Markov
{
    [TestFixture, Category("MarkovTest")]
    public class TextGenTest
    {
        private string trainingText;
        // private const string filePath = "Assets/TDPG/TextGeneration/TrainingFiles/Elements_Clean.txt";
        private const string filePath = "Assets/TDPG/TextGeneration/TrainingFiles/Names_Fantasy_Clean.txt";

        [OneTimeSetUp]
        public void LoadTrainingData()
        {
            Assert.IsTrue(File.Exists(filePath), $"Training file missing: {filePath}");
            trainingText = File.ReadAllText(filePath);
            Assert.IsNotNull(trainingText);
            Assert.IsTrue(trainingText.Length > 50, "Training text too short.");
        }
        
        private string InvokeCleanup(MarkovChain mc, string word)
        {
            var m = typeof(MarkovChain).GetMethod("CleanupWord",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            return (string)m.Invoke(mc, new object[] { word });
        }

        private string InvokeFixCase(MarkovChain mc, string word)
        {
            var m = typeof(MarkovChain).GetMethod("FixOutputCase",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            return (string)m.Invoke(mc, new object[] { word });
        }
        
        [Test]
        public void CanTrainWithoutThrowing()
        {
            var markov = new MarkovChain(2);
            Assert.DoesNotThrow(() => markov.Train(trainingText));
        }
        

        [Test]
        public void GeneralGenerationForAllLetters([NUnit.Framework.Range(65, 90)] int ascii)   // A-Z 
        {
            var markov = new MarkovChain(2); //Short words with order 2 work best
            markov.Train(trainingText);

            char c = (char)ascii;
            string start = c.ToString();

            string generated = markov.Generate(5, start);
            Debug.Log($"{start} -> {generated}");

            Assert.IsNotNull(generated);
            Assert.GreaterOrEqual(generated.Length, 1);
            Assert.IsFalse(string.IsNullOrWhiteSpace(generated));
            Assert.AreEqual(generated.Length, 5);
        }
        
        [Test]
        public void Order1_GeneratesWords()
        {
            var markov = new MarkovChain(1);
            markov.Train(trainingText);

            string word = markov.Generate(8, "N");
            Debug.Log(word);

            Assert.IsNotNull(word);
            Assert.Greater(word.Length, 2);
        }
        
        [Test]
        public void Order2_GeneratesWords()
        {
            var markov = new MarkovChain(2);
            markov.Train(trainingText);

            string word = markov.Generate(12, "Cl");
            Debug.Log(word);

            Assert.IsNotNull(word);
            Assert.Greater(word.Length, 2);
        }
        
        [Test]
        public void Order5_GeneratesWords()
        {
            var markov = new MarkovChain(5);
            markov.Train(trainingText);

            string word = markov.Generate(24, "Volun");
            Debug.Log(word);

            Assert.IsNotNull(word);
            Assert.GreaterOrEqual(word.Length, 5);
        }
        
        [Test]
        public void PrefixFallback_SelectsShorterPrefixWhenNeeded()
        {
            var markov = new MarkovChain(3);
            markov.Train("abcdefg\nabczzz\nabyyyy\naxxxx");

            string word = markov.Generate(6, "ABQ");
            Debug.Log($"Generated: {word}");

            // Valid output checks
            Assert.IsNotNull(word);
            Assert.GreaterOrEqual(word.Length, 2);
            Assert.IsTrue(word.StartsWith("Ab") || word.StartsWith("AB"),
                $"Fallback prefix not used properly. Generated: {word}");
        }

        
        [Test]
        public void Blacklist_PreventsForbiddenWords()
        {
            var markov = new MarkovChain(2);
            markov.Train(trainingText);

            markov.AddToBlacklist("gas");

            for (int i = 0; i < 200; i++)
            {
                string word = markov.Generate(3, "ga");
                Assert.IsFalse(word.ToLower().Contains("gas"));
            }
            
        }
        
        [Test]
        public void PrintProbabilities_DoesNotThrow()
        {
            var markov = new MarkovChain(2);
            markov.Train(trainingText);

            Assert.DoesNotThrow(() => markov.PrintProbabilities());
        }
        
        [Test]
        public void Cleanup_RemovesTrailingGarbage()
        {
            var markov = new MarkovChain(2);
            string cleaned = InvokeCleanup(markov, "Test-name,.");
            Assert.AreEqual("Test-name", cleaned);
        }
        
        [Test]
        public void FixOutputCase_UppercasesFirstLetter()
        {
            var markov = new MarkovChain(2);
            string fixedCase = InvokeFixCase(markov, "hello");
            Assert.AreEqual("Hello", fixedCase);
        }
        
        [Test]
        public void ApplyPrefixAndSuffix_AddsCorrectly()
        {
            var markov = new MarkovChain(2);

            markov.AddPrefixRule("Neo", 2.0);
            markov.AddPrefixRule("Ultra-", 1.0);

            markov.AddSuffixRule("-ium", 2.0);
            markov.AddSuffixRule("ex", 1.0);

            string baseName = "roma";

            // Apply prefix
            string prefixed = markov.ApplyPrefix(baseName);
            Debug.Log(prefixed);
            Assert.IsTrue(prefixed.EndsWith(baseName));
            Assert.IsTrue(prefixed.Contains(baseName));

            // Apply suffix
            string suffixed = markov.ApplySuffix(baseName);
            Debug.Log(suffixed);
            Assert.IsTrue(suffixed.StartsWith(baseName));
            Assert.IsTrue(suffixed.Length > baseName.Length);

            // Ensure original base string remains unchanged
            Assert.AreEqual("roma", baseName);
        }

        [Test]
        public void ChoosesStart()
        {
            var markov = new MarkovChain(2);
            markov.Train(trainingText);
            string word = markov.Generate(10);
            
            Debug.Log(word);
            Assert.IsNotNull(word);
            Assert.AreEqual(word.Length, 10);
        }

        [Test]
        public void GenerateBySeed()
        {
            var markov = new MarkovChain(2);
            markov.Train(trainingText);
            Seed seed = new Seed(5_11,-1,"MarkovTest", false);

            string word = markov.Generate(seed);
            Debug.Log(word);
            
            Assert.IsNotNull(word);
            Assert.AreEqual(word.Length, 5); //since key is 510 so length 5
            Assert.AreEqual('L', word[0]); // verify starting character is L
        }
        
    }
}