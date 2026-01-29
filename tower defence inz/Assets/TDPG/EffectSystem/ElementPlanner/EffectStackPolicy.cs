using System.Collections.Generic;

namespace TDPG.EffectSystem.ElementPlanner
{
    /// <summary>
    /// A logic handler that resolves value collisions according to a specific <see cref="StackMode"/>.
    /// <br/>
    /// Used by the Planner to aggregate multiple active effects into a final resolved state.
    /// </summary>
    public class EffectStackPolicy
    {
        /// <summary>
        /// The strategy this policy employs (Additive vs Override).
        /// </summary>
        public StackMode Mode { get; }
        
        /// <summary>
        /// Creates a new policy with the specified behavior.
        /// </summary>
        /// <param name="mode">The stacking logic to enforce.</param>
        public EffectStackPolicy(StackMode mode) { Mode = mode; }

        /// <summary>
        /// Iterates through a list of parameter maps and merges them into a single result 
        /// based on the <see cref="Mode"/>.
        /// </summary>
        /// <param name="maps">
        /// A chronological list of effect data. 
        /// <br/>Each dictionary represents one effect's modifications (Key=ParamName, Value=Magnitude).
        /// </param>
        /// <returns>
        /// A single dictionary containing the final calculated values for all keys.
        /// </returns>
        public Dictionary<string, float> Combine(List<Dictionary<string, float>> maps)
        {
            var result = new Dictionary<string, float>();
            foreach (var map in maps)
            {
                foreach (var kv in map)
                {
                    if (Mode == StackMode.Additive)
                    {
                        if (!result.ContainsKey(kv.Key)) result[kv.Key] = 0;
                        result[kv.Key] += kv.Value;
                    }
                    else if (Mode == StackMode.Override)
                    {
                        result[kv.Key] = kv.Value;
                    }
                }
            }
            return result;
        }
    }
}
