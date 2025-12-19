using System;
using System.Collections.Generic;

namespace TDPG.Generators.AttackPatterns
{
    /// <summary>
    /// A data container representing a fully generated attack sequence.
    /// <br/>
    /// Contains the high-level metadata (Duration, ID) and the list of specific atomic events (Projectiles/Hitboxes).
    /// </summary>
    [Serializable]
    public class AttackPattern
    {
        /// <summary>
        /// A unique identifier (usually a GUID) for this specific instance of the pattern.
        /// <br/>Useful for tracking active patterns in an Entity Component System or Dictionary.
        /// </summary>
        public string id;
        
        /// <summary>
        /// The total duration of the pattern in seconds.
        /// <br/>This is often the lifespan of the "caster" state or the cooldown before the next pattern begins.
        /// </summary>
        public float duration;
        
        /// <summary>
        /// The chronological list of atomic actions (e.g., firing a bullet, spawning a hitbox) that occur during this pattern.
        /// </summary>
        public List<AttackEvent> events = new List<AttackEvent>();
    }

    
    /// <summary>
    /// Represents a single atomic action within an <see cref="AttackPattern"/>.
    /// <br/>
    /// Typically maps to spawning one projectile or activating one hitbox.
    /// </summary>
    [Serializable]
    public struct AttackEvent
    {
        /// <summary>
        /// The scheduled time for this event, relative to the start of the pattern ($T=0$).
        /// <br/>Unit: Seconds.
        /// </summary>
        public float timeOffset;     // sec from pattern start
        
        /// <summary>
        /// The direction vector represented as a list of floats.
        /// <br/>
        /// [0]=X, [1]=Y, [2]=Z (if applicable).
        /// </summary>
        public List<float> direction; // vector as list of floats (dim-dependent)
        
        /// <summary>
        /// The scalar speed of the projectile or effect.
        /// </summary>
        public float speed;
        
        /// <summary>
        /// The raw damage value associated with this event.
        /// </summary>
        public int damage;
        
        /// <summary>
        /// The angular deviation applied to the direction.
        /// <br/>Unit: Degrees.
        /// </summary>
        public float spreadAngle;
        
        /// <summary>
        /// Arbitrary string data used to tag the event type (e.g., "Fireball", "Sniper", "Crit").
        /// <br/>Game logic can use this to switch between different projectile prefabs.
        /// </summary>
        public string metaTag;
    }
}
