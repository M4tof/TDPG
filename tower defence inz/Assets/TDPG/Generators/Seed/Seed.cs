using System;
using UnityEngine;

namespace TDPG.Generators.Seed
{
    [Serializable]
    public class Seed : ISeed
    {
        [SerializeField]
        private ulong value;
        [SerializeField]
        private int id;
        [SerializeField]
        private string parentName;
        [SerializeField] internal bool isBitBased = true;

        public ulong Value { get => value; private set => this.value = value; }
        public int Id { get => id; internal set => id = value; }
        public string ParentName { get => parentName; internal set => parentName = value; }
        public bool IsBitBased { get => isBitBased; internal set => isBitBased = value; }

        public Seed(ulong key, int id, string parentName = null, bool isBitBased = true)
        {
            Value = key;
            Id = id;
            ParentName = parentName;
            IsBitBased = isBitBased;
        }

        public Seed() { } // Parameterless constructor for serializing

        public ulong GetBaseValue()
        {
            return Value;
        }

        public string GetName()
        {
            return ParentName;
        }
        
        public static Seed operator *(Seed a, Seed b) //needed if we want to explicitly multiply seed to seed instead of Iseed to seed
        {
            if (!a.IsBitBased) a.NormalizeSeedValue();
            if (!b.IsBitBased) b.NormalizeSeedValue();
            return (a as ISeed) * (b as ISeed);
        }

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
                result[i] = (byte)(aByte[i] | bByte[i]); // Bitwise OR
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
        
        public bool IsBitSet(int bitIndex)
        {
            return (value & ((ulong)1 << bitIndex)) != 0;
        }
        
    }
}