using System;
using System.Linq;

namespace TDPG.Generators.Seed
{
    public static class Genetic
    {
        private const int MinimumHammingDistanceThreshold = 2;

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
        
        public static Seed CreateChildSeed(ISeed[] parents) //Child seed of many parents, -1 id
        {
            Seed tmpSeed = new Seed(parents[0].GetBaseValue(),-1,parents[0].GetName());
            foreach (var parent in parents)
            {
                if(parent == parents[0]) { }
                else
                {
                    tmpSeed += parent;
                }
            }
            
            return tmpSeed;
        }
        
        public static Seed CreateChildSeed(ISeed[] parents, int id) //Child seed of many parents, explicit id
        {
            Seed tmpSeed = new Seed(parents[0].GetBaseValue(),-1,parents[0].GetName());
            foreach (var parent in parents)
            {
                if(parent == parents[0]) { }
                else
                {
                    tmpSeed += parent;
                }
            }
            
            tmpSeed.Id = id;
            
            return tmpSeed;
        }
        
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
            
            else
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
            
        }
        
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

        public static Seed CreateChildSeedDistinct(ISeed[] parents)
        {
            Seed startSeed = CreateChildSeed(parents);
            while (!PassesHammingDistance(startSeed, parents))
            {
                startSeed = MutateSeed(startSeed, MutateTypes.Deterministic);
            }
            return startSeed;
        }
        
        public static Seed CreateChildSeedDistinct(ISeed[] parents, int id)
        {
            Seed tmpSeed = CreateChildSeedDistinct(parents);
            tmpSeed.Id = id;
            return tmpSeed;
        }
        
    }
}