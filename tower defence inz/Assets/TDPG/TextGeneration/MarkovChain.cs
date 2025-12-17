using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using System.Linq;
using TDPG.Generators.Seed;

namespace TDPG.TextGeneration
{
    /// <summary>
    /// A procedural text generator based on Markov Chains.
    /// <br/>
    /// Supports variable-order chains(<see cref="order"/>), forbidden word filtering (<see cref="blacklist"/>), and weighted prefix/suffix application.
    /// </summary>
    [Serializable]
    public class MarkovChain
    {
        [Tooltip("The depth of the chain. 1 = looks at previous letter. 2 = looks at previous 2 letters (more coherent, less random).")]
        [SerializeField] private int order = 1;
        
        /// <summary>
        /// prefix (string of length N) -> dict of next char counts
        /// </summary>
        private Dictionary<string, Dictionary<char, int>> chain = new();
        
        private static readonly char[] trailingBadChars =
        {
            ' ', ',', '.', ';', ':', '-', '_', '!', '?', '\'', '"', ')', '(', '[', ']', '/', '\\'
        };
        private static readonly Random globalRand = new();
        
        /// <summary>
        /// A list of prefixes that can be forcefully prepended to generated text.
        /// </summary>
        [SerializeField] internal List<string> forcedPrefixes = new();

        /// <summary>
        /// A list of suffixes that can be forcefully appended to generated text.
        /// </summary>
        [SerializeField] internal List<string> forcedSuffixes = new();

        /// <summary>
        /// Normalization weights for each prefix. Used during random selection.
        /// </summary>
        [SerializeField] internal Dictionary<string, double> prefixWeights = new();

        /// <summary>
        /// Normalization weights for each suffix. Used during random selection.
        /// </summary>
        [SerializeField] internal Dictionary<string, double> suffixWeights = new();

        /// <summary>
        /// A list of forbidden words.
        /// <br/>
        /// If the generator produces a word containing any of these, it will discard the result and retry.
        /// </summary>
        [SerializeField] internal List<string> blacklist = new();

        /// <summary>
        /// Creates a new MarkovChain generator. 
        /// <br/>
        /// <b>Note:</b> You must call <see cref="Train"/> before generating text.
        /// </summary>
        /// <param name="order">
        /// How many previous characters are considered when choosing the next one.
        /// <br/>Higher values result in words that closer resemble the training data.
        /// </param>
        public MarkovChain(int order = 1)
        {
            if (order < 1) throw new ArgumentException("Order must be >= 1");
            this.order = order;
        }

        // -------------------------
        // TRAINING
        // -------------------------
        
        /// <summary>
        /// Ingests a text corpus to build the probability chain.
        /// <br/>
        /// The input text is converted to lowercase and filtered.
        /// </summary>
        /// <param name="text">The raw training data (e.g., a list of names or sentences).</param>
        /// <remarks>
        /// <b>Allowed Characters:</b> a-z, 0-9, apostrophe ('), underscore (_), hyphen (-), and space.
        /// <br/>All other characters are stripped.
        /// </remarks>
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
        
        /// <summary>
        /// Registers a new prefix rule.
        /// </summary>
        /// <param name="prefix">The string to prepend (e.g., 'King ', 'Super-').</param>
        /// <param name="weight">The relative likelihood of picking this prefix vs others.</param>
        public void AddPrefixRule(string prefix, double weight = 1.0)
        {
            if (!prefixWeights.ContainsKey(prefix))
                prefixWeights[prefix] = weight;
            else
                prefixWeights[prefix] += weight;

            forcedPrefixes.Add(prefix);
        }

        /// <summary>
        /// Registers a new suffix rule.
        /// </summary>
        /// <param name="suffix">The string to append (e.g., ' Rex', '-Man').</param>
        /// <param name="weight">The relative likelihood of picking this suffix vs others.</param>
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
        
        /// <summary>
        /// Generates a new string based on the training data probabilities.
        /// <br/>
        /// Attempts to generate a word not found in the <see cref="blacklist"/>.
        /// </summary>
        /// <param name="length">The target length of the generated string.</param>
        /// <param name="start">
        /// The starting sequence. If null, a random start is chosen. 
        /// <br/>If the start sequence doesn't exist in training data, it attempts to find a close match.
        /// </param>
        /// <returns>A newly generated string.</returns>
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

        /// <summary>
        /// A deterministic wrapper for <see cref="Generate(int,string)"/>.
        /// <br/>
        /// Extracts length and starting character derived specifically from the provided Seed.
        /// </summary>
        /// <param name="seed">The seed object used to derive generation parameters.</param>
        /// <returns>A newly generated string.</returns>
        public string Generate(Seed seed)
        {
            seed.IsBitBased = false;
            seed.NormalizeSeedValue();
            
            // Extract length from the first digit
            int length = int.Parse(seed.GetBaseValue().ToString()[0].ToString());
            
            // Extract starting character from specific digits
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

        /// <summary>
        /// Randomly applies a prefix from <see cref="forcedPrefixes"/> to the base string.
        /// </summary>
        /// <param name="baseString">The word onto which the prefix will be applied.</param>
        /// <returns>The concatenated string (Prefix + Base).</returns>
        public string ApplyPrefix(string baseString)
        {
            if (string.IsNullOrEmpty(baseString) || forcedPrefixes.Count == 0)
                return baseString;

            string chosen = PickWeightedRandom(forcedPrefixes, prefixWeights);
            return chosen + baseString;
        }
        
        /// <summary>
        /// Randomly applies a suffix from <see cref="forcedSuffixes"/> to the base string.
        /// </summary>
        /// <param name="baseString">The word onto which the suffix will be applied.</param>
        /// <returns>The concatenated string (Base + Suffix).</returns>
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
        
        /// <summary>
        /// Logs the internal Markov Chain probabilities to the Console for debugging.
        /// </summary>
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
        
        /// <summary>
        /// Retrieves the raw probability chain. Useful for comparing different generator states.
        /// </summary>
        /// <returns>The internal chain dictionary.</returns>
        public Dictionary<string, Dictionary<char, int>> GetProbabilities()
        {
            return chain;
        }

        /// <summary>
        /// Retrieves the list of learned or forced prefixes.
        /// </summary>
        /// <returns>The list of prefixes.</returns>
        public List<string> GetPrefixes()
        {
            return forcedPrefixes;
        }
        
        /// <summary>
        /// Retrieves the list of learned or forced suffixes.
        /// </summary>
        /// <returns>The list of suffixes.</returns>
        public List<string> GetSuffixes()
        {
            return forcedSuffixes;
        }

        /// <summary>
        /// Retrieves the current blacklist.
        /// </summary>
        /// <returns>The list of forbidden words.</returns>
        public List<string> GetBlacklist()
        {
            return blacklist;
        }
    
        /// <summary>
        /// Returns the Order (N-gram size) of this generator.
        /// </summary>
        public int GetOrder()
        {
            return order;
        }
        
        /// <summary>
        /// Resets the generator to a blank state.
        /// <br/>
        /// Clears the chain, all prefixes/suffixes/weights, and the blacklist.
        /// </summary>
        public void Clear()
        {
            chain.Clear();
            forcedPrefixes.Clear();
            forcedSuffixes.Clear();
            prefixWeights.Clear();
            suffixWeights.Clear();
            blacklist.Clear();
        }

        /// <summary>
        /// The total number of unique prefixes currently stored in the chain.
        /// </summary>
        public int PrefixCount => chain.Count;
        
        // -------------------------
        // RULE MANAGEMENT 
        // -------------------------
        
        /// <summary>
        /// Adds a word to the blacklist.
        /// </summary>
        /// <param name="word">The 'bad' word (e.g., profanity or copyrighted term) to forbid in output.</param>
        public void AddToBlacklist(string word)
        {
            if (!string.IsNullOrWhiteSpace(word))
                blacklist.Add(word.ToLowerInvariant());
        }

    }
}