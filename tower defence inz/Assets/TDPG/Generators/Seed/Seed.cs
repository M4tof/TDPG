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

        public ulong Value { get => value; private set => this.value = value; }
        public int Id { get => id; internal set => id = value; }
        public string ParentName { get => parentName; internal set => parentName = value; }

        public Seed(ulong key, int id, string parentName = null)
        {
            Value = key;
            Id = id;
            ParentName = parentName;
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
        
        public static Seed operator +(Seed a, Seed b) //needed if we want to explicitly add seed to seed instead of Iseed to seed
        {
            return (a as ISeed) + (b as ISeed);
        }
        
    }
}