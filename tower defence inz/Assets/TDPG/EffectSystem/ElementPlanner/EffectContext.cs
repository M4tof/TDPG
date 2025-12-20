using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TDPG.EffectSystem.ElementPlanner
{
    /// <summary>
    /// A runtime payload containing the state required to execute a gameplay effect.
    /// <br/>
    /// Passed to <see cref="IEffectAction.Execute"/> to resolve interactions between the source, target, and environment.
    /// </summary>
    public class EffectContext
    {
        /// <summary>
        /// The entity responsible for triggering the effect (The Instigator).
        /// </summary>
        public GameObject Attacker { get; set; }
        
        /// <summary>
        /// The entity receiving the effect (The Victim/Recipient).
        /// </summary>
        public GameObject Target { get; set; }
        
        /// <summary>
        /// The specific world-space coordinate where the interaction occurred.
        /// <br/>
        /// Useful for spawning impact particles, decals, or calculating knockback vectors.
        /// </summary>
        public Vector3 HitPosition { get; set; }
        
        /// <summary>
        /// Reference to the local grid or environment system.
        /// <br/>
        /// Required for effects that interact with tiles (e.g., spreading fire, freezing water) or Area of Effect logic.
        /// </summary>
        public Grid Grid { get; set; }
    }
}
