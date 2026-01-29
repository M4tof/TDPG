namespace TDPG.EffectSystem.ElementLogic
{
    /// <summary>
    /// Defines standard semantic keys for all possible numerical parameters within the Effect System.
    /// <br/>
    /// Using an Enum instead of strings ensures type safety and consistency across the Planner, Registry, and Transfer layers.
    /// </summary>
    public enum EffectParameter
    {
        /// <summary>
        /// The lifespan of the effect in seconds.
        /// <br/>Used by temporal effects (e.g., SlowDown, Infection, Buffs).
        /// </summary>
        Duration,            
        
        /// <summary>
        /// An immediate modification to the target's Health Points.
        /// <br/>Positive values typically Heal, negative values Deal Damage (e.g., Execution, Instant Heal).
        /// </summary>
        HealthChange,        
        
        /// <summary>
        /// A multiplier applied to movement speed.
        /// <br/>(e.g., 0.5 = 50% speed, 0.0 = Rooted).
        /// </summary>
        SlowdownFactor,      
        
        /// <summary>
        /// A multiplier applied to movement speed, lasts only for some time.
        /// <br/>Used for "progressive" slows (e.g., turning to stone), or for temporary slowing down.
        /// </summary>
        SlowdownOverTime,    
        
        /// <summary>
        /// Damage applied per interval (Tick).
        /// <br/>Used for Damage over Time (DoT) effects like Poison or Bleeding.
        /// </summary>
        HealthDrain,         
        
        /// <summary>
        /// A multiplier for the visual or physical size of the object.
        /// <br/>Used for projectile transformations or growth/shrink effects.
        /// </summary>
        Scaling,             
        
        /// <summary>
        /// The duration in seconds that the target is incapacitated.
        /// <br/>Used for Hard CC (Crowd Control) like Stuns or Freezes.
        /// </summary>
        StunDuration,  
        
    }
}