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
                case EffectParameter.StunDuration:
                    return new StunAction(value);
                case EffectParameter.Scaling:
                    return new ScaleAction(value);

                default:
                    throw new Exception($"No action for parameter {parameter}");
            }
        }
        public static IEffectAction Create(EffectParameter parameter, List<float>value)
        {
            switch (parameter)
            {
                case EffectParameter.SlowdownFactor:
                    return new SlowDownAction(value[0]);
                case EffectParameter.HealthChange:
                    return value[0] < 0
                        ? new HealthDownAction(value[0])
                        : new HealAction(value[0]);
                case EffectParameter.StunDuration:
                    return new StunAction(value[0]);
                case EffectParameter.Scaling:
                    return new ScaleAction(value[0]);
                case EffectParameter.SlowdownOverTime:
                    return new TempSlowDownAction(value[0], value[1]);
                case EffectParameter.HealthDrain:
                    return new HealthDrainAction(value[0], (int)value[1], value[2]);

                default:
                    throw new Exception($"No action for parameter {parameter}");
            }
        }
        //TODO: Long-term
    }
}
