using System;
using System.Linq;

namespace TDPG.Generators.Seed
{
    /// <summary>
    /// Static utility class handling the low-level bitwise operations for Seed combination (Breeding) and Mutation.
    /// </summary>
    public static class Genetic
    {
        /// <summary>
        /// The minimum number of bits that must differ between a child and its parents 
        /// to be considered a "Distinct" generation.
        /// </summary>
        private const int MinimumHammingDistanceThreshold = 2;

        /// <summary>
        /// Combines two byte arrays into a single 64-bit entropy value.
        /// <br/>
        /// Algorithm: 
        /// 1. XORs the two arrays (overlapping).
        /// 2. Performs a cyclic bit-shift (Rotation) to scramble patterns.
        /// 3. Truncates/Converts to ulong.
        /// <br/>This is a "gentle" crossover that preserves some bitwise relationships 
        /// </summary>
        /// <param name="aByte">First byte array (DNA A).</param>
        /// <param name="bByte">Second byte array (DNA B).</param>
        /// <returns>A new 64-bit entropy value.</returns>
        public static ulong ByteCrossover(byte[] aByte, byte[] bByte)
        {
            // Perform XOR with automatic length handling
            int maxLength = Math.Max(aByte.Length, bByte.Length);
            byte[] valueXor = Enumerable.Range(0, maxLength)
                .Select(i => (byte)
                    ((i < aByte.Length ? aByte[i] : 0) ^ 
                     (i < bByte.Length ? bByte[i] : 0)))
                .ToArray();
            
            // Handle empty case
            if (valueXor.Length == 0)
            {
                return ulong.MaxValue;
            }
            
            // Mix bytes with shift-right operation
            byte[] finalValue = new byte[valueXor.Length];
            byte carryOver = (byte)(valueXor[^1] << 6);

            for (int i = 0; i < valueXor.Length; i++)
            {
                byte shifted = (byte)(valueXor[i] >> 2);
                if (i > 0)
                {
                    shifted |= (byte)(valueXor[i - 1] << 6);
                }
                finalValue[i] = shifted;
            }
    
            finalValue[0] |= carryOver;
            
            // Ensure we have enough bytes for ulong conversion
            byte[] resultBytes = new byte[8];
            Array.Copy(finalValue, resultBytes, Math.Min(finalValue.Length, 8));
            ulong value = BitConverter.ToUInt64(resultBytes, 0);
            return value;
        }
        
        /// <summary>
        /// Creates a new offspring by iteratively combining a list of parent seeds.
        /// <br/>The resulting seed has ID -1 (Untracked).
        /// </summary>
        /// <param name="parents">Array of ancestor seeds.</param>
        /// <returns>A new Seed representing the combination of all parents.</returns>
        public static Seed CreateChildSeed(ISeed[] parents) //Child seed of many parents, -1 id
        {
            Seed tmpSeed = new Seed(parents[0].GetBaseValue(),-1,parents[0].GetName());
            foreach (var parent in parents)
            {
                if(parent == parents[0]) { }
                else
                {
                    tmpSeed *= parent;
                }
            }
            
            return tmpSeed;
        }
        
        /// <summary>
        /// Creates a new offspring from a list of parents and assigns a specific ID.
        /// </summary>
        /// <param name="parents">Array of ancestor seeds.</param>
        /// <param name="id">The explicit ID to assign to the new seed.</param>
        /// <returns>A new Seed representing the combination of all parents.</returns>
        public static Seed CreateChildSeed(ISeed[] parents, int id) //Child seed of many parents, explicit id
        {
            Seed tmpSeed = new Seed(parents[0].GetBaseValue(),-1,parents[0].GetName());
            foreach (var parent in parents)
            {
                if(parent == parents[0]) { }
                else
                {
                    tmpSeed *= parent;
                }
            }
            
            tmpSeed.Id = id;
            
            return tmpSeed;
        }
        
        /// <summary>
        /// Alters the entropy of an existing seed based on the specified mutation strategy.
        /// </summary>
        /// <param name="original">The source seed.</param>
        /// <param name="type">The mutation strategy (Deterministic vs Random).</param>
        /// <param name="flipPercentage">Probability (0-100) of a byte being inverted. Only used in <see cref="MutateTypes.Random"/>.</param>
        /// <param name="crossOverMap">Optional byte mask for <see cref="MutateTypes.Deterministic"/>. Defaults to an internal constant if null.</param>
        /// <returns>A new, mutated Seed instance.</returns>
        public static Seed MutateSeed(Seed original,
            MutateTypes type,
            int flipPercentage = 15,
            byte[] crossOverMap = null)
        { // Function used to mutate a seed, takes an enumerate to decide if it should be done Deterministic or Random

            crossOverMap ??= new byte[] { 0x24, 0xFF, 0x11, 0x00, 0x12, 0xCC, 0x16, 0x42 }; // birthday/random  meaning of life repeat
            
            byte[] originalBytes = BitConverter.GetBytes(original.GetBaseValue());
            byte[] mutatedMap = originalBytes;
            if (type == MutateTypes.Deterministic) //Deterministic mutation works by making a ByteCrossover with a given map
            {
                // Ensure crossOverMap is exactly the same length as , repeat until to size
                if (crossOverMap.Length != originalBytes.Length)
                {
                    crossOverMap = Enumerable.Range(0, originalBytes.Length)
                        .Select(i => crossOverMap[i % crossOverMap.Length])
                        .ToArray();
                }
                ulong value = ByteCrossover(originalBytes, crossOverMap);
                Seed mutated = new Seed(value,original.Id,original.ParentName);
                return mutated;
            }
            
            if (type == MutateTypes.Random)
            { //Random mutation is undeterministic, gives each byte a chance to be flipped
                Random randomizer = new Random();
                
                for(int i = 0; i < mutatedMap.Length; i++ )
                {
                    if (randomizer.Next(0, 100) < flipPercentage)
                    {
                        mutatedMap[i] = (byte)~mutatedMap[i];
                    }
                }
                
                Seed mutated = new Seed(BitConverter.ToUInt64(mutatedMap, 0),original.Id,original.ParentName);
                return mutated;
            }

            else
            {
                return original;
            }
        }
        
        /// <summary>
        /// Checks if a child is sufficiently different (diverse) from all of its parents.
        /// </summary>
        /// <returns>True if the Hamming Distance is >= <see cref="MinimumHammingDistanceThreshold"/> for all parents.</returns>
        private static bool PassesHammingDistance(Seed child, ISeed[] parents)
        {
            // Get the child's base value as bytes for comparison
            byte[] childBytes = BitConverter.GetBytes(child.GetBaseValue());
    
            foreach (var parent in parents)
            {
                if (parent is Seed parentSeed)
                {
                    // Get the parent's base value as bytes
                    byte[] parentBytes = BitConverter.GetBytes(parentSeed.GetBaseValue());
            
                    // Calculate Hamming distance between child and this parent
                    int hammingDistance = CalculateHammingDistance(childBytes, parentBytes);
            
                    // If the distance is too small, it fails the check
                    if (hammingDistance < MinimumHammingDistanceThreshold)
                    {
                        return false;
                    }
                }
            }
    
            return true;
        }

        private static int CalculateHammingDistance(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Byte arrays must be of equal length");
    
            int distance = 0;
            for (int i = 0; i < a.Length; i++)
            {
                // XOR the bytes and count the number of set bits (1s)
                byte xorResult = (byte)(a[i] ^ b[i]);
                while (xorResult != 0)
                {
                    distance++;
                    xorResult &= (byte)(xorResult - 1); // Clear the lowest set bit
                }
            }
            return distance;
        }

        /// <summary>
        /// Creates a child seed that is guaranteed to be genetically distinct from its parents.
        /// <br/>
        /// If the initial combination results in a clone (or near-clone), it recursively applies 
        /// deterministic mutation until sufficient diversity (Hamming Distance) is achieved.
        /// </summary>
        /// <param name="parents">Array of ancestor seeds.</param>
        /// <returns>A unique, divergent Seed.</returns>
        public static Seed CreateChildSeedDistinct(ISeed[] parents)
        {
            Seed startSeed = CreateChildSeed(parents);
            while (!PassesHammingDistance(startSeed, parents))
            {
                startSeed = MutateSeed(startSeed, MutateTypes.Deterministic);
            }
            return startSeed;
        }
        
        /// <summary>
        /// Creates a distinct child seed with a specific ID.
        /// <br/>See <see cref="CreateChildSeedDistinct(ISeed[])"/>.
        /// </summary>
        public static Seed CreateChildSeedDistinct(ISeed[] parents, int id)
        {
            Seed tmpSeed = CreateChildSeedDistinct(parents);
            tmpSeed.Id = id;
            return tmpSeed;
        }
        
        /// <summary>
        /// A 64-bit non-linear mixer (Finalizer) based on MurmurHash3/SplitMix64.
        /// <br/>Ensures the Avalanche Effect: changing one bit in the input 
        /// results in roughly 32 bits changing in the output.
        /// </summary>
        public static ulong Scramble(ulong x)
        {
            x = (x ^ (x >> 30)) * 0xbf58476d1ce4e5b9L;
            x = (x ^ (x >> 27)) * 0x94d049bb133111ebL;
            x = x ^ (x >> 31);
            return x;
        }
        
        /// <summary>
        /// Generates a stable 64-bit hash from a string using the FNV-1a algorithm.
        /// <br/>Unlike string.GetHashCode(), this is guaranteed to be identical across 
        /// different .NET runtimes and OS platforms.
        /// </summary>
        /// <param name="str">Text input value</param>
        /// <returns>Hashed value from the str</returns>
        public static ulong GetDeterministicHash(string str)
        {
            // A simple FNV-1a hash for strings
            ulong hash = 0xcbf29ce484222325;
            foreach (char c in str)
            {
                hash ^= (ulong)c;
                hash *= 0x100000001b3;
            }
            return hash;
        }
    }
}