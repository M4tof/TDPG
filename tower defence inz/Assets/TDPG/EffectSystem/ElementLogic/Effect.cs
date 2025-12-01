using System.Collections.Generic;
using UnityEngine;

namespace TDPG.EffectSystem.ElementLogic
{
    // This is abstract of the actual effect applied in game, e.g., SlowDown
    public abstract class Effect
    {
        public string Name { get; protected set; }
        protected internal float[] Values { get; set; } 
        public string Description { get; protected set; }

        protected Effect(string name, string description, params float[] values)
        {
            Name = name;
            Values = values;
            Description = description;
        }


        // Transfer structured data
        public abstract Dictionary<EffectParameter, List<float>> LogicTransfer(); // For use in planners, etc.
        public abstract int ParamNum(); // How many parameters (Values.Length)

        public float[] GetValues() => Values;

        // Cloning & reconstruction API
        public abstract Effect Clone(); // return copy of self
        public abstract Effect WithValues(float[] newValues);
    }
}