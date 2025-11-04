using UnityEngine;

namespace TDPG.EffectSystem.Element
{
    // This is abstract of the actual effect applied in game, ex. SlowDown
    public abstract class Effect
    {
        public string Name { get; protected set; }
        public float Value { get; protected set; }
        
        public string  Description { get; protected set; }

        protected Effect(string name, float value, string description)
        {
            Name = name;
            Value = value;
            Description = description;
        }

        public abstract void Apply(GameObject target);
    }
    
}