using PlasticGui.Help.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDPG.EffectSystem.ElementPlanner;
using TDPG.EffectSystem.ElementLogic;

namespace Assets.TDPG.EffectSystem.ElementPlanner
{
    public static class EffectActionFactory
    {
        public static IEffectAction Create(EffectParameter parameter, float value)
        {
            switch (parameter)
            {
                case EffectParameter.SlowdownFactor:
                    return new SlowDownAction(value);

                case EffectParameter.HealthChange:
                    return value < 0
                        ? new HealthDownAction(value)
                        : new HealAction(value);

                case EffectParameter.Duration:
                    return new DurationAction(value);

                default:
                    throw new Exception($"No action for parameter {parameter}");
            }
            //TODO: LONG-TERM
        }
    }
}
