using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDPG.EffectSystem.ElementPlanner
{
    /// <summary>
    /// Defines a concrete, executable command derived from an abstract Effect.
    /// <br/>
    /// While <see cref="TDPG.EffectSystem.ElementLogic.Effect"/> holds the <i>Data</i> ("What happens"), 
    /// this interface handles the <i>Execution</i> ("How it happens") within the Unity Game World.
    /// </summary>
    public interface IEffectAction
    {
        /// <summary>
        /// The unique identifier or display name of this action (e.g., "ApplySlow", "DealDamage").
        /// <br/>Useful for debugging or logging AI decisions.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// A scalar value representing the 'strength', cost, or utility of this action.
        /// </summary>
        float Intensity { get; }
        
        /// <summary>
        /// Performs the actual gameplay logic on the targets defined in the context.
        /// <br/>
        /// This is where Unity components (NavMeshAgents, HealthControllers) are modified.
        /// </summary>
        /// <param name="context">Runtime data containing the Source, Target, and World state.</param>
        void Execute(EffectContext context);
    }
}
