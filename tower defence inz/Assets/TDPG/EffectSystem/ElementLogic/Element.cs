using System;
using System.Collections.Generic;
using System.Linq;
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
        
        private static float[] NormalizeValues(float[] input, int needed)
        {
            if (input == null || input.Length == 0)
                return Enumerable.Repeat(0.1f, needed).ToArray();

            if (input.Length >= needed)
                return input;

            // repeat pattern until we reach needed count
            List<float> extended = new List<float>(input);
            while (extended.Count < needed)
                extended.AddRange(input);
            return extended.Take(needed).ToArray();
        }
        
        // --- Effect factory registry ---
        public static readonly Dictionary<int, Func<float[], Effect>> EffectFactories = new()
        {
            {
                0, values => {
                    var v = NormalizeValues(values, 2);
                    return new SlowDown(v[0], v[1]);
                }
            },
            {
                1, values => {
                    var v = NormalizeValues(values, 1);
                    return new HealthDown(v[0]);
                }
            },
            {
                2, values => {
                    var v = NormalizeValues(values, 1);
                    return new Heal(v[0]);
                }
            }
            //TODO: LONG TERM FILL HERE
        };
        public Element(string name, int id, Seed dna, params float[] values)
        {
            Name = name;
            Id = id;
            this.dna = dna;
            this.values = values ?? Array.Empty<float>();

            dna.NormalizeSeedValue();

            for (int i = 0; i < 64; i++)
            {
                if (dna.IsBitSet(i) && EffectFactories.TryGetValue(i, out var factory))
                    effects.Add(factory(this.values));
            }

            MetaData = new List<string> { dna.ToString() };
        }
        
        //TODO: LONG TERM FILL HERE
        // --- Reverse lookup map (bit -> effect type) ---
        public static readonly Dictionary<int, Type> EffectTypes = new()
        {
            { 0, typeof(SlowDown) },
            { 1, typeof(HealthDown) },
            { 2, typeof(Heal) }
        };
        public Element(string name, int id, List<Effect> effects)
        {
            Name = name;
            Id = id;
            this.effects.AddRange(effects ?? new());

            ulong seedValue = 0;
            foreach (var effect in this.effects)
            {
                var match = EffectTypes.FirstOrDefault(kvp => kvp.Value == effect.GetType());
                if (!match.Equals(default(KeyValuePair<int, Type>)))
                    seedValue |= (1UL << match.Key);
                else
                    Debug.LogWarning($"[Element] No bit mapping for {effect.GetType().Name}, treats it as 0");
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
            Debug.Log($"[Element] Deserialization constructor called for {name}");
            Name = name;
            Id = id;
            this.dna = dna;
            this.effects = effects ?? new List<Effect>();
            MetaData = new List<string> { dna.ToString() };
        }

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

        public string ReNameElement(string newName)
        {
            string oldName = Name;
            this.Name = newName;
            return oldName;
        }
        
    }
}