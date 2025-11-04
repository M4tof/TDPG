using System.Collections.Generic;
using UnityEngine;

namespace TDPG.EffectSystem.Element
{
    // This is structure holding a set of effects to apply, ex. Fire
    public class Element
    {
        public string Name { get; private set; }
        public int Id { get; private set; }
        public IReadOnlyList<Effect> Effects => _effects;

        private readonly List<Effect> _effects = new();
        public List<string> MetaData { get; private set; } = new();

        
        public Element(string name, int id)
        {
            Name = name;
            Id = id;
        }
        
        public Element(string name, int id, List<Effect> effects)
        {
            Name = name;
            Id = id;
            _effects = effects ?? new List<Effect>();
        }
        
        
        public void AddEffect(Effect effect) => _effects.Add(effect);
        public void RemoveEffect(Effect effect) => _effects.Remove(effect);

        public void AddMetaData(string metaData) => MetaData.Add(metaData);

        public void ApplyEffects(GameObject target)
        {
            foreach (Effect effect in _effects)
            {
                effect.Apply(target);
            }
        }
        
        
    }
}