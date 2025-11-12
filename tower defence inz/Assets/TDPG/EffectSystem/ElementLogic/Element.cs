using System;
using System.Collections.Generic;
using TDPG.Generators.Seed;
using UnityEngine;
using Newtonsoft.Json;

namespace TDPG.EffectSystem.ElementLogic
{
    // This is structure holding a set of effects to apply, ex. Fire
    public class Element
    {
        public string Name { get; private set; }
        public int Id { get; internal set; }
        private readonly List<Effect> effects = new();
        public List<string> MetaData { get; internal set; } = new();
        private readonly Seed dna;
        private float[] values;
        
        private static readonly Dictionary<int, Func<float[], Effect>> EffectRegistry = new()
        {
            { 1, values => new SlowDown(values[0], values[1]) },
            { 2, values => new HealthDown(values[0]) },
            { 3, values => new Heal(values[0]) },
            //todo: LONG-TERM EACH EFFECT NEEDS ENUM
        };
        
        public Element(string name, int id, Seed dna,  params float[] values)
        {
            Name = name;
            Id = id;
            this.dna = dna;

            dna.NormalizeSeedValue();
            for (int i = 0; i < 64; i++)
            {
                if (dna.IsBitSet(i)) //if a bit corresponding to effect of equal ID is 1
                {
                    if (EffectRegistry.TryGetValue(i, out var effectFactory))
                    {
                        Effect newEffect = effectFactory(values);
                        effects.Add(newEffect);
                    }
                }
            }
            
            MetaData = new List<string> { dna.ToString() };
        }
        
        public Element(string name, int id, List<Effect> effects)
        {
            Name = name;
            Id = id;
            this.effects = effects ?? new List<Effect>();
            
            // Reverse lookup: Effect type -> bit index
            Dictionary<Type, int> reverseMap = new();
            foreach (var kvp in EffectRegistry)
            {
                int bit = kvp.Key;
                var effectType = kvp.Value.Invoke(Array.Empty<float>()).GetType();
                reverseMap[effectType] = bit;
            }
            
            ulong seedValue = 0;
            foreach (var effect in this.effects)
            {
                Type type = effect.GetType();

                if (reverseMap.TryGetValue(type, out int bitIndex))
                {
                    seedValue |= (1UL << bitIndex);
                }
                else
                {
                    Debug.LogWarning($"No registry entry found for effect type {type.Name} — skipping.");
                }
            }
            dna = new Seed(seedValue, -1, "SeedFromEffectList");
            
            MetaData = new List<string> { dna.ToString() };
        }

        /// <summary>
        /// Constructor used only for deserialization. 
        /// Assumes <paramref name="dna"/> and <paramref name="effects"/> already match.
        /// </summary>
        internal Element(string name, int id, Seed dna, List<Effect> effects)
        {
            #if UNITY_EDITOR
            Debug.Log($"[Element] Deserialization constructor called for {name}");
            #endif
            Name = name;
            Id = id;
            this.dna = dna;
            this.effects = effects ?? new List<Effect>();
            MetaData = new List<string> { dna.ToString() };
        }
        
        public void AddEffect(Effect effect) => effects.Add(effect);
        public void RemoveEffect(Effect effect) => effects.Remove(effect);

        public void AddMetaData(string metaData) => MetaData.Add(metaData);

        public void ApplyEffects(GameObject target)
        {
            foreach (Effect effect in effects)
            {
                effect.Apply(target);
            }
        }
        
        public List<Effect> GetEffects() => effects;
        
        public Seed GetDna() => dna;

        public override string ToString()
        {
            return $"Name: {Name}, Id: {Id},  MetaData: {string.Join(",", MetaData)}, Seed: {dna}";
        }

        private static JsonSerializerSettings DefaultSettings => new()
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>
            {
                new EffectConverter(),
                new ElementConverter(),
                new SeedConverter()
            }
        };

        public string Serialize() => JsonConvert.SerializeObject(this, DefaultSettings);
        public static Element Deserialize(string json) => JsonConvert.DeserializeObject<Element>(json, DefaultSettings);

    }
}