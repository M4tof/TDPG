using System;
using UnityEngine;

namespace TDPG.Generators.Seed
{
    /// <summary>
    /// The standard concrete implementation of <see cref="ISeed"/>.
    /// <br/>
    /// Wraps a 64-bit entropy value with metadata (ID, Parentage) and provides 
    /// genetic operators for combining seeds.
    /// </summary>
    /// <remarks>
    /// <b>Interpretation Modes:</b>
    /// <br/>The library can interpret this seed in two ways:
    /// <list type="bullet">
    /// <item><description><b>DNA (Bit-based):</b> A chain of 64 independent flags/genes.</description></item>
    /// <item><description><b>Scalar (Magnitude-based):</b> A numeric value where digits/size matter.</description></item>
    /// </list>
    /// </remarks>
    [Serializable]
    public class Seed : ISeed
    {
        /// <summary>
        /// The raw 64-bit entropy value, can be interpreted as 64bits or long int.
        /// </summary>
        [SerializeField]
        private ulong value;
        
        /// <summary>
        /// Unique Identifier for this seed. -1 indicates a generated/temporary seed outside GlobalSeed.
        /// </summary>
        [SerializeField]
        private int id;
        
        /// <summary>
        /// String description of the seed's origin (e.g., '_ChildOf:A+B'). 
        /// </summary>
        [SerializeField]
        private string parentName;
        
        /// <summary>
        /// If TRUE, treated as a bitmask (DNA). If FALSE, treated as a scalar number (magnitude matters).
        /// </summary>
        [SerializeField] internal bool isBitBased = true;

        /// <summary>
        /// The raw 64-bit unsigned integer serving as the entropy source.
        /// </summary>
        public ulong Value { get => value; private set => this.value = value; }
        
        /// <summary>
        /// The unique ID of this seed instance.
        /// </summary>
        public int Id { get => id; internal set => id = value; }
        
        /// <summary>
        /// Metadata describing the lineage or origin of this seed. In cases outside GlobalSeed it functions as metadata.
        /// </summary>
        public string ParentName { get => parentName; internal set => parentName = value; }
        
        /// <summary>
        /// Determines how the value is interpreted during normalization.
        /// <br/>
        /// <b>True:</b> The specific bits matter (Flags/DNA).
        /// <br/>
        /// <b>False:</b> The magnitude/digits matter (Scalar).
        /// </summary>
        public bool IsBitBased { get => isBitBased;
            set => isBitBased = value; }

        /// <summary>
        /// Creates a new Seed instance.
        /// </summary>
        /// <param name="key">The raw entropy value.</param>
        /// <param name="id">The unique ID (use -1 for temporary seeds).</param>
        /// <param name="parentName">Optional debug name describing origin.</param>
        /// <param name="isBitBased">Whether this seed represents a bitmask (True) or a scalar value (False).</param>
        public Seed(ulong key, int id, string parentName = null, bool isBitBased = true)
        {
            Value = key;
            Id = id;
            ParentName = parentName;
            IsBitBased = isBitBased;
        }

        /// <summary>
        /// Parameterless constructor required for Serialization.
        /// </summary>
        public Seed() { }

        public ulong GetBaseValue()
        {
            return Value;
        }

        public string GetName()
        {
            return ParentName;
        }
        
        /// <summary>
        /// Performs a <b>Genetic Crossover</b>.
        /// <br/>
        /// Invokes the <see cref="ISeed"/> operator logic (Byte Crossover) to create a child that mixes genes from both parents.
        /// </summary>
        public static Seed operator *(Seed a, Seed b)
        {
            if (!a.IsBitBased) a.NormalizeSeedValue();
            if (!b.IsBitBased) b.NormalizeSeedValue();
            return (a as ISeed) * (b as ISeed);
        }

        /// <summary>
        /// Performs a <b>Genetic Union</b> (Bitwise OR).
        /// <br/>
        /// The child inherits all active traits (1s) present in <i>either</i> parent.
        /// </summary>
        /// <remarks>
        /// Example: Parent A has gene 1, Parent B has gene 2 -> Child has genes 1 and 2.
        /// </remarks>
        public static Seed operator +(Seed a, Seed b)
        {
            string parentName = "_ChildOf:" + a.GetName() + b.GetName();
            int id = -1;
            
            if (!a.IsBitBased) a.NormalizeSeedValue();
            if (!b.IsBitBased) b.NormalizeSeedValue();
            
            byte[] aByte = BitConverter.GetBytes(a.GetBaseValue()); 
            byte[] bByte = BitConverter.GetBytes(b.GetBaseValue());
            
            byte[] result = new byte[aByte.Length];
            
            for (int i = 0; i < aByte.Length; i++) { 
                result[i] = (byte)(aByte[i] | bByte[i]); 
            } 
            
            byte[] resultBytes = new byte[8];
            Array.Copy(result, resultBytes, Math.Min(result.Length, 8));
            ulong value = BitConverter.ToUInt64(resultBytes, 0);
            return new Seed(value, id, parentName);
        }
        
        /// <summary>
        /// Performs a <b>Genetic Divergence</b> (Bitwise XOR).
        /// <br/>
        /// The child inherits traits that are unique to one parent or the other, but <b>discards</b> traits shared by both.
        /// </summary>
        /// <remarks>
        /// Useful for creating "Mutant" variations that differ significantly from the "Norm".
        /// </remarks>
        public static Seed operator ^(Seed a, Seed b)
        {
            string parentName = "_ChildOf:" + a.GetName() + b.GetName();
            int id = -1;
            
            if (!a.IsBitBased) a.NormalizeSeedValue();
            if (!b.IsBitBased) b.NormalizeSeedValue();
            
            byte[] aByte = BitConverter.GetBytes(a.GetBaseValue()); 
            byte[] bByte = BitConverter.GetBytes(b.GetBaseValue());
            
            byte[] result = new byte[aByte.Length];
            
            for (int i = 0; i < aByte.Length; i++) { 
                result[i] = (byte)(aByte[i] ^ bByte[i]); 
            } 
            
            byte[] resultBytes = new byte[8];
            Array.Copy(result, resultBytes, Math.Min(result.Length, 8));
            ulong value = BitConverter.ToUInt64(resultBytes, 0);
            return new Seed(value, id, parentName);
        }
        
        public override string ToString()
        {
            return $"Value: {value}, Id: {Id}, Parent: {ParentName}";
        }
        
        /// <summary>
        /// Ensures the internal value has sufficient magnitude (digits) for scalar operations.
        /// <br/>
        /// If <see cref="IsBitBased"/> is false, this pads the number with zeros until it has at least 10 digits.
        /// </summary>
        public void NormalizeSeedValue()
        {
            if (IsBitBased) return;
            
            ulong normalizeSeedValue = value;
            // if it's already long enough (>= 1_000_000_000), don't touch it
            if (normalizeSeedValue >= 1_000_000_000)
                this.value = normalizeSeedValue;

            int digits = normalizeSeedValue == 0 ? 1 : (int)Math.Floor(Math.Log10(normalizeSeedValue)) + 1;
            int zerosToAdd = Math.Max(0, 9 - digits); // we want total ~10 digits
            ulong multiplier = (ulong)Math.Pow(10, zerosToAdd);

            this.value =  normalizeSeedValue * multiplier;
        }
        
        /// <summary>
        /// Checks if a specific bit index is set (1) in the seed's value.
        /// </summary>
        /// <param name="bitIndex">The index (0-63) to check.</param>
        /// <returns>True if the bit is 1, False if 0.</returns>
        public bool IsBitSet(int bitIndex)
        {
            return (value & ((ulong)1 << bitIndex)) != 0;
        }
        
    }
}