using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TDPG.Generators.Seed
{
    [Serializable]
    public class GlobalSeed:ISeed
    {
        public ulong Base { get; private set; }
        public string Name { get; private set; }
        
        public string Description { get; private set; }

        [SerializeField]
        private List<Seed> subSeeds;
        
        [SerializeField]
        private int currIndex;

        public GlobalSeed(ulong baseSeed, string name = null, string description = null)
        {
            Base = baseSeed;
            Name = name;
            Description = description;
            subSeeds = new List<Seed>();
            currIndex = 0;
        }
        
        public ulong GetBaseValue()
        {
            return Base;
        }

        public string GetName()
        {
            return Name;
        }
        
        [Serializable]
        private class SerializableGlobalSeed
        {
            public ulong @base;
            public string @name;
            public string @description;
            public List<Seed> @subSeeds;
            public int @currIndex;
        }

        public Seed GetSubSeed(int id)
        {
            return subSeeds[id];
        }

        public Seed NextSubSeed(string key) // create new subSeed based on a key (could be for example currentTime), adds to list and returns it.
        {
            // Convert key to hash, then to bytes
            var hash = new Hash128();
            hash.Append(key);
            byte[] hashedKey = Encoding.ASCII.GetBytes(hash.ToString());
    
            // Convert base to bytes
            byte[] bitBase = BitConverter.GetBytes(this.GetBaseValue());
            
            ulong value = Genetic.ByteCrossover(bitBase, hashedKey);
    
            Seed result = new Seed(value, currIndex++, "Child of global seed");
            subSeeds.Add(result);
            
            return result;
        }

        public string ToShortHash()
        {
            string importantData = $"{Name}{Base}{Description}{subSeeds?.Count ?? 0}";
            var hash = new Hash128();
            hash.Append(importantData);
            return hash.ToString();
        }

        public string Serialize()
        {
            var serializableData = new SerializableGlobalSeed
            {
                @base = GetBaseValue(),
                @name = Name,
                @description = Description,
                @subSeeds = subSeeds,
                @currIndex = currIndex
            };
            
            return JsonUtility.ToJson(serializableData, false); 
        }
        
        public static GlobalSeed Deserialize(string serializedData)
        {
            try
            {
                var data = JsonUtility.FromJson<SerializableGlobalSeed>(serializedData);
                
                var globalSeed = new GlobalSeed(data.@base, data.@name, data.@description);

                if (data.@subSeeds != null)
                {
                    globalSeed.subSeeds = data.@subSeeds;
                    globalSeed.currIndex = data.@currIndex;
                }
                
                return globalSeed;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize GlobalSeed: {ex.Message}");
                return null;
            }
        }

    }
}