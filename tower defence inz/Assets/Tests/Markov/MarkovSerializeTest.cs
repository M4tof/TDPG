using Newtonsoft.Json;
using NUnit.Framework;
using TDPG.TextGeneration;
using UnityEngine;

namespace Tests.Markov
{
    [TestFixture, Category("MarkovTest")]
    public class MarkovSerializeTest
    {
        [Test]
        public void MarkovChain_CanSerializeAndDeserialize_Full()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new MarkovChainConverter() }
            };

            var markov = new MarkovChain(3);
            markov.Train("abcdefg\nabczzz\nabyyyy\naxxxx");
            markov.AddPrefixRule("abcdefg", 2.0);
            markov.AddPrefixRule("abczzz", 2.0);
            markov.AddSuffixRule("zzz", 5.0);
            markov.AddToBlacklist("bad_word");

            string json = JsonConvert.SerializeObject(markov, settings);
            MarkovChain clone = JsonConvert.DeserializeObject<MarkovChain>(json, settings);
            
            CollectionAssert.AreEquivalent(markov.getPrefixes(), clone.getPrefixes());
            CollectionAssert.AreEquivalent(markov.getSuffixes(), clone.getSuffixes());
            CollectionAssert.AreEquivalent(markov.GetBlacklist(), clone.GetBlacklist());
            Assert.That(markov.getOrder(), Is.EqualTo(clone.getOrder()));
            
            var chainA = markov.getProbabilities();
            var chainB = clone.getProbabilities();
            
            Assert.AreEqual(chainA.Count, chainB.Count);

            foreach (var kv in chainA)
            {
                Assert.IsTrue(chainB.ContainsKey(kv.Key), $"Missing key: {kv.Key}");

                var innerA = kv.Value;
                var innerB = chainB[kv.Key];

                CollectionAssert.AreEquivalent(innerA.Keys, innerB.Keys);

                foreach (var ch in innerA.Keys)
                    Assert.AreEqual(innerA[ch], innerB[ch], $"Mismatch for prefix {kv.Key}, char {ch}");
            }
            Assert.DoesNotThrow(() => clone.Generate(8));
            
            markov.PrintProbabilities();
            Debug.Log(markov.Generate(5,"A"));
            
            Debug.Log("----------------");
            
            clone.PrintProbabilities();
            Debug.Log(clone.Generate(5,"A"));
        }

        
    }
}