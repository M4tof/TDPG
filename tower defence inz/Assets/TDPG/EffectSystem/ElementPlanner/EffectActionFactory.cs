using System;
using System.Collections.Generic;
using TDPG.EffectSystem.ElementLogic;

namespace TDPG.EffectSystem.ElementPlanner
{
    /// <summary>
    /// Static factory responsible for translating semantic <see cref="EffectParameter"/> keys into executable <see cref="IEffectAction"/> instances.
    /// <br/>
    /// Acts as the bridge between the Data Layer (EffectLogic) and the Execution Layer (ElementPlanner).
    /// </summary>
    public static class EffectActionFactory
    {
        /// <summary>
        /// Instantiates a simple action derived from a single scalar value.
        /// </summary>
        /// <remarks>
        /// <b>Special Logic:</b>
        /// <br/>For <see cref="EffectParameter.HealthChange"/>:
        /// <list type="bullet">
        /// <item><description>Negative Value → Returns <see cref="HealthDownAction"/> (Damage).</description></item>
        /// <item><description>Positive Value → Returns <see cref="HealAction"/> (Restoration).</description></item>
        /// </list>
        /// </remarks>
        /// <param name="parameter">The semantic key identifying the effect type.</param>
        /// <param name="value">The raw magnitude or duration.</param>
        /// <returns>A concrete, executable <see cref="IEffectAction"/>.</returns>
        /// <exception cref="Exception">Thrown if the provided parameter has no mapped Action implementation.</exception>
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
        
        /// <summary>
        /// Instantiates a complex action derived from a list of values (e.g., Duration + Intensity).
        /// </summary>
        /// <remarks>
        /// Used for effects like <see cref="EffectParameter.HealthDrain"/> which require [Damage, Count, Interval] or <see cref="EffectParameter.SlowdownOverTime"/> [Factor, Duration].
        /// </remarks>
        /// <param name="parameter">The semantic key identifying the effect type.</param>
        /// <param name="value">The list of arguments. The order (Index 0, 1, 2) MUST match the constructor of the target Action.</param>
        /// <returns>A concrete, executable <see cref="IEffectAction"/>.</returns>
        /// <exception cref="Exception">Thrown if the provided parameter has no mapped Action implementation.</exception>
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
