using System.Collections.Generic;
using UnityEngine;

namespace TDPG.EffectSystem.ElementLogic
{
    /// <summary>
    /// The abstract base class representing an active gameplay modifier or status condition.
    /// <br/>
    /// It's base for effects like "SlowDown", "Poison", or "StatBoost".
    /// Implementations wrap raw numerical data (<see cref="Values"/>) with semantic logic.
    /// </summary>
    public abstract class Effect
    {
        /// <summary>
        /// The display name or unique identifier of the effect (e.g., "Frostbite").
        /// </summary>
        public string Name { get; protected set; }
        
        /// <summary>
        /// The raw numerical parameters driving the effect's logic.
        /// <br/>
        /// Index [0] might be Duration, [1] might be Intensity, etc., depending on the concrete implementation.
        /// </summary>
        protected internal float[] Values { get; set; } 
        
        /// <summary>
        /// A human-readable description of what the effect does.
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// Base constructor for an Effect.
        /// </summary>
        /// <param name="name">Identifier.</param>
        /// <param name="description">Tooltip text.</param>
        /// <param name="values">Variable number of float parameters definition the effect strength/duration.</param>
        protected Effect(string name, string description, params float[] values)
        {
            Name = name;
            Values = values;
            Description = description;
        }

        // Transfer structured data
        
        /// <summary>
        /// Converts the raw <see cref="Values"/> array into a semantic Dictionary.
        /// <br/>
        /// Used by AI Planners or UI systems to understand what the numbers actually represent 
        /// (e.g., mapping <c>Values[0]</c> to <c>EffectParameter.Damage</c>).
        /// </summary>
        /// <returns>A dictionary mapping semantic keys to value lists.</returns>
        public abstract Dictionary<EffectParameter, List<float>> LogicTransfer(); // For use in planners, etc.
        
        /// <summary>
        /// Returns the expected number of parameters for this effect.
        /// <br/>
        /// Used for validation to ensure the <see cref="Values"/> array has the correct length.
        /// </summary>
        public abstract int ParamNum();

        /// <summary>
        /// Retrieves the raw parameter array.
        /// </summary>
        public float[] GetValues() => Values;

        // Cloning & reconstruction API
        
        /// <summary>
        /// Creates a deep copy of this effect instance.
        /// </summary>
        public abstract Effect Clone();
        
        /// <summary>
        /// Creates a new instance of this Effect type with modified numerical parameters.
        /// <br/>
        /// <b>Procedural Use:</b> This is used to create "Leveled" versions of effects 
        /// (e.g., creating a "Level 2 Fireball" from a "Level 1 Fireball" by just injecting new floats).
        /// </summary>
        /// <param name="newValues">The new parameter array (must match <see cref="ParamNum"/>).</param>
        /// <returns>A new Effect instance with the updated values.</returns>
        public abstract Effect WithValues(float[] newValues);
    }
}