using System.Collections.Generic;
using UnityEngine;

namespace TDPG.EffectSystem.ElementLogic
{
    // This is abstract of the actual effect applied in game, ex. SlowDown
    public abstract class Effect
    {
        public string Name { get; protected set; }
        protected float[] Values { get; set; } 
        public string  Description { get; protected set; }

        protected Effect(string name, string description, params float[] values)
        {
            Name = name;
            Values = values;
            Description = description;
        }

        public abstract void Apply(GameObject target);  // Actually apply the effect

        // Transfer structured data
        public abstract Dictionary<EffectParameter, float> LogicTransfer();  //Transfer Effect definition, for use in the planner
    }
    
}