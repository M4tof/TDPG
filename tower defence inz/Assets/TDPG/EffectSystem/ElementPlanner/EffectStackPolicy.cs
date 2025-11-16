using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;

namespace TDPG.EffectSystem.ElementPlanner
{
    public class EffectStackPolicy
    {
        public StackMode Mode { get; }
        public EffectStackPolicy(StackMode mode) { Mode = mode; }


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
