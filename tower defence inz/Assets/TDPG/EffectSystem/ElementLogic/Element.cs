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
        
        public Element(string name, int id, Seed dna,  params float[] values)
        {
            Name = name;
            Id = id;
            this.dna = dna;
            //TODO: effects from seed and values
            MetaData = new List<string> { dna.ToString() };
        }
        
        public Element(string name, int id, List<Effect> effects)
        {
            Name = name;
            Id = id;
            this.effects = effects ?? new List<Effect>();
            //TODO: seed from effects
            Seed effectsToSeed = new Seed();
            
            
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