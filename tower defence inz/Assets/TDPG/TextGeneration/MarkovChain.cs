using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using System.Linq;
using TDPG.Generators.Seed;

namespace TDPG.TextGeneration
{
    [Serializable]
    public class MarkovChain
    {
        [SerializeField] private int order = 1;

        // prefix (string of length N) -> dict of next char counts
        private Dictionary<string, Dictionary<char, int>> chain = new();
        private static readonly char[] trailingBadChars =
        {
            ' ', ',', '.', ';', ':', '-', '_', '!', '?', '\'', '"', ')', '(', '[', ']', '/', '\\'
        };
        private static readonly Random globalRand = new();



        [SerializeField] internal List<string> forcedPrefixes = new();
        [SerializeField] internal List<string> forcedSuffixes = new();
        [SerializeField] internal Dictionary<string, double> prefixWeights = new();
        [SerializeField] internal Dictionary<string, double> suffixWeights = new();
        [SerializeField] internal List<string> blacklist = new();


        public MarkovChain(int order = 1)
        {
            if (order < 1) throw new ArgumentException("Order must be >= 1");
            this.order = order;
        }

        // -------------------------
        // TRAINING
        // -------------------------
        public void Train(string text)
        {
            text = text.ToLowerInvariant();
    
            // Define allowed characters using a HashSet for fast lookup
            var allowedChars = new HashSet<char>("abcdefghijklmnopqrstuvwxyz0123456789'_- ");
    
            foreach (string line in text.Split('\n'))
            {
                // Clean the line - remove unwanted characters and trim
                string cleanLine = new string(line.Where(c => allowedChars.Contains(c)).ToArray()).Trim();
        
                if (cleanLine.Length <= order) continue;
        
                for (int i = 0; i <= cleanLine.Length - order - 1; i++)
                {
                    string prefix = cleanLine.Substring(i, order);
                    char next = cleanLine[i + order];

                    if (!chain.ContainsKey(prefix))
                        chain[prefix] = new Dictionary<char, int>();

                    if (!chain[prefix].ContainsKey(next))
                        chain[prefix][next] = 0;

                    chain[prefix][next]++;
                }
            }
        }
        
        public void AddPrefixRule(string prefix, double weight = 1.0)
        {
            if (!prefixWeights.ContainsKey(prefix))
                prefixWeights[prefix] = weight;
            else
                prefixWeights[prefix] += weight;

            forcedPrefixes.Add(prefix);
        }

        public void AddSuffixRule(string suffix, double weight = 1.0)
        {
            if (!suffixWeights.ContainsKey(suffix))
                suffixWeights[suffix] = weight;
            else
                suffixWeights[suffix] += weight;

            forcedSuffixes.Add(suffix);
        }

        // -------------------------
        // GENERATION
        // -------------------------
        public string Generate(int length = 5, string start = "C")
        {
            const int maxAttempts = 50;    // prevents infinite loops
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                string result = GenerateInternal(length, start);
                result = CleanupWord(result);
                if (!IsBlacklisted(result))
                    return result;
            }

            Debug.LogWarning("MarkovChain: Could not generate a non-blacklisted word after many attempts.");
            string fallback = GenerateInternal(length, start);
            return CleanupWord(fallback);
        }

        public string Generate(Seed seed)
        {
            seed.IsBitBased = false;
            seed.NormalizeSeedValue();
            int length = int.Parse(seed.GetBaseValue().ToString()[0].ToString());
            int charAscii =
                65 +
                int.Parse(seed.GetBaseValue().ToString().Substring(1, 2));

            charAscii = Math.Clamp(charAscii, 65, 90);
            
            return Generate(length, ((char)charAscii).ToString());
        }
        
        private string GenerateInternal(int length, string start = null)
        {
            if (chain.Count == 0)
                throw new InvalidOperationException("MarkovChain: Generate() called before Train().");

            Random rand = globalRand;

            string selectedPrefix = string.IsNullOrEmpty(start) 
                ? GetRandomPrefix(rand) 
                : FindClosestPrefix(start);

            string currentPrefix = selectedPrefix;
            string result = currentPrefix;

            while (result.Length < length)
            {
                char? next = GetNextChar(currentPrefix, rand);
                if (next == null) break;

                result += next.Value;

                // slide window using original order
                currentPrefix = result.Substring(result.Length - order, order);
            }

            return FixOutputCase(result);
        }

        public string ApplyPrefix(string baseString)
        {
            if (string.IsNullOrEmpty(baseString) || forcedPrefixes.Count == 0)
                return baseString;

            string chosen = PickWeightedRandom(forcedPrefixes, prefixWeights);
            return chosen + baseString;
        }
        
        public string ApplySuffix(string baseString)
        {
            if (string.IsNullOrEmpty(baseString) || forcedSuffixes.Count == 0)
                return baseString;

            string chosen = PickWeightedRandom(forcedSuffixes, suffixWeights);
            return baseString + chosen;
        }
        
            // -------------------------
            // GENERATION HELPERS
            // -------------------------
        private string FixOutputCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            if (s.Length == 1)
                return char.ToUpperInvariant(s[0]).ToString();

            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }
        
        private string FindClosestPrefix(string start)
        {
            if (string.IsNullOrEmpty(start))
                return null;

            start = start.ToLowerInvariant();

            // Try full -> progressively shorter
            for (int len = Math.Min(start.Length, order); len > 0; len--)
            {
                string sub = start.Substring(0, len);
    
                // look for any chain key that starts with sub
                foreach (var key in chain.Keys)
                {
                    if (key.StartsWith(sub))
                    {
                        return key;
                    }
                }
            }
    
            // fallback: random key
            Debug.Log($"    no sub found");
            return GetRandomPrefix(new Random());
        }
        
        private string GetRandomPrefix(Random rand)
        {
            var keys = new List<string>(chain.Keys);
            return keys[rand.Next(keys.Count)];
        }

        private char? GetNextChar(string prefix, Random rand)
        {
            if (!chain.ContainsKey(prefix))
                return null;

            var dict = chain[prefix];

            int total = 0;
            foreach (var kv in dict)
                total += kv.Value;

            int pick = rand.Next(total);
            int running = 0;

            foreach (var kv in dict)
            {
                running += kv.Value;
                if (pick < running)
                    return kv.Key;
            }

            return null;
        }
        
        private bool IsBlacklisted(string generated)
        {
            string lower = generated.ToLowerInvariant();

            foreach (string bad in blacklist)
            {
                if (lower.Contains(bad))     // contains
                    return true;
            }
            return false;
        }
        
        private string CleanupWord(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            // Remove trailing unwanted characters
            while (s.Length > 0 && Array.Exists(trailingBadChars, c => c == s[^1]))
            {
                s = s.Substring(0, s.Length - 1);
            }

            return s;
        }
        
        private string PickWeightedRandom(List<string> options, Dictionary<string, double> weights)
        {
            double totalWeight = options.Sum(o => weights.ContainsKey(o) ? weights[o] : 1.0);
            Random rand = globalRand;
            double pick = rand.NextDouble() * totalWeight;

            double cumulative = 0;
            foreach (string o in options)
            {
                double w = weights.ContainsKey(o) ? weights[o] : 1.0;
                cumulative += w;
                if (pick <= cumulative)
                    return o;
            }

            return options.Last(); // fallback
        }
        
        // -------------------------
        // DEBUGGING
        // -------------------------
        public void PrintProbabilities()
        {
            foreach (var kv in chain)
            {
                string prefix = kv.Key;
                var sortedProbabilities = kv.Value.OrderByDescending(x => x.Value);
                foreach (var nxt in sortedProbabilities)
                {
                    Debug.Log($"{prefix} -> {nxt.Key}: {nxt.Value}");
                }
            }
        }

        public Dictionary<string, Dictionary<char, int>> getProbabilities()
        {
            return chain;
        }

        public List<string> getPrefixes()
        {
            return forcedPrefixes;
        }
        
        public List<string> getSuffixes()
        {
            return forcedSuffixes;
        }

        public List<string> GetBlacklist()
        {
            return blacklist;
        }

        public int getOrder()
        {
            return order;
        }
        
        public void Clear()
        {
            chain.Clear();
            forcedPrefixes.Clear();
            forcedSuffixes.Clear();
            prefixWeights.Clear();
            suffixWeights.Clear();
            blacklist.Clear();
        }

        public int PrefixCount => chain.Count;
        
        // -------------------------
        // RULE MANAGEMENT 
        // -------------------------
        public void AddToBlacklist(string word)
        {
            if (!string.IsNullOrWhiteSpace(word))
                blacklist.Add(word.ToLowerInvariant());
        }

    }
}
