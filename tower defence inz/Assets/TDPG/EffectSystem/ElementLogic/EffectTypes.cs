using System.Collections.Generic;
using UnityEngine;

namespace TDPG.EffectSystem.ElementLogic
{

    /// <summary>
    /// Applies a permanent movement speed reduction to the target.
    /// </summary>
    public class SlowDown : Effect
    {
        /// <summary>
        /// Creates a permanent slow effect.
        /// </summary>
        /// <param name="factor">The percentage to slow by (0.0 to 1.0). 0.5 = 50% speed.</param>
        public SlowDown(float factor) 
            : base("SlowDown", $"Slows down the target by {factor * 100}% indefinitely.", factor) { }

        /// <summary>
        /// Maps parameters to <see cref="EffectParameter.SlowdownFactor"/>.
        /// </summary>
        public override Dictionary<EffectParameter, List<float>> LogicTransfer()
        {
            float factor = Values.Length > 0 ? Values[0] : 0f;

            return new Dictionary<EffectParameter, List<float>>
            {
                { EffectParameter.SlowdownFactor, new List<float>{factor} }
            };
        }

        public override int ParamNum() => Values.Length;

        public override Effect Clone() => new SlowDown(Values[0]);

        /// <summary>
        /// Creates a new SlowDown with a modified intensity.
        /// </summary>
        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 1) newValues = new float[] { Values[0] };
            return new SlowDown(newValues[0]);
        }
        
    }

    /// <summary>
    /// Applies a movement speed reduction that expires after a set duration.
    /// </summary>
    public class TempSlowDown : Effect
    {
        /// <summary>
        /// Creates a temporary slow effect.
        /// </summary>
        /// <param name="factor">The percentage to slow by (0.0 to 1.0).</param>
        /// <param name="duration">Time in seconds before the slow is removed.</param>
        public TempSlowDown(float factor, float duration)
            : base("SlowDown", $"Slows down the target by {factor * 100}% for {duration} seconds.", factor, duration) { }

        /// <summary>
        /// Maps parameters to <see cref="EffectParameter.SlowdownOverTime"/>.
        /// <br/>List Format: [0] = Factor, [1] = Duration.
        /// </summary>
        public override Dictionary<EffectParameter, List<float>> LogicTransfer()
        {
            float factor = Values.Length > 0 ? Values[0] : 0f;
            float duration = Values.Length > 1 ? Values[1] : 0f;

            return new Dictionary<EffectParameter, List<float>>
            {
                { EffectParameter.SlowdownOverTime, new List<float>{factor, duration } },
            };
        }
        
        public override int ParamNum() => Values.Length;
        public override Effect Clone() => new TempSlowDown(Values[0], Values[1]);

        /// <summary>
        /// Creates a new TempSlowDown with modified intensity [0] and duration [1].
        /// </summary>
        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 2) newValues = new float[] { Values[0], Values[1] };
            return new TempSlowDown(newValues[0], newValues[1]);
        }

    }


    /// <summary>
    /// Applies immediate damage to the target.
    /// </summary>
    public class HealthDown : Effect
    {
        /// <summary>
        /// Creates an instant damage effect.
        /// </summary>
        /// <param name="amount">The absolute amount of health to remove.</param>
        public HealthDown(float amount)
            : base("HealthDown", $"Lowers health of the target by {amount} points, once.", amount) {}
        
        /// <summary>
        /// Maps parameters to <see cref="EffectParameter.HealthChange"/>.
        /// <br/><b>Note:</b> The value is automatically negated (multiplied by -1) to represent damage.
        /// </summary>
        public override Dictionary<EffectParameter, List<float>> LogicTransfer()
        {
            float amount = Values.Length > 0 ? Values[0] : 0f;
            return new Dictionary<EffectParameter, List<float>>
            {
                { EffectParameter.HealthChange, new List<float>{-amount } }
            };
        }

        public override int ParamNum() => Values.Length;
        public override Effect Clone() => new HealthDown(Values[0]);

        /// <summary>
        /// Creates a new HealthDown with modified damage amount.
        /// </summary>
        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 1) newValues = new float[] { Values[0] };
            return new HealthDown(newValues[0]);
        }
        
    }
    
    /// <summary>
    /// Applies immediate restoration of health to the target.
    /// </summary>
    public class Heal : Effect
    {
        /// <summary>
        /// Creates an instant heal effect.
        /// </summary>
        /// <param name="amount">The absolute amount of health to restore.</param>
        public Heal(float amount)
            : base("Heal", $"Heals the target by {amount} points, once.", amount) {}

        /// <summary>
        /// Maps parameters to <see cref="EffectParameter.HealthChange"/>.
        /// </summary>
        public override Dictionary<EffectParameter, List<float>> LogicTransfer()
        {
            float amount = Values.Length > 0 ? Values[0] : 0f;
            return new Dictionary<EffectParameter, List<float>>
            {
                { EffectParameter.HealthChange, new List<float>{amount} }
            };
        }

        public override int ParamNum() => Values.Length;

        public override Effect Clone() => new Heal(Values[0]);

        /// <summary>
        /// Creates a new Heal with modified amount.
        /// </summary>
        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 1) newValues = new float[] { Values[0] };
            return new Heal(newValues[0]);
        }
        
    }

    /// <summary>
    /// Applies damage over time (DoT) in discrete ticks.
    /// </summary>
    public class HealthDrain : Effect
    {
        /// <summary>
        /// Creates Damage over Time effect.
        /// </summary>
        /// <param name="healthPerDrain">Damage applied per tick.</param>
        /// <param name="drainCount">Total number of ticks.</param>
        /// <param name="idleTime">Time interval (seconds) between ticks.</param>
        public HealthDrain(float healthPerDrain, float drainCount, float idleTime)
            : base("HealthDrain", $"Drains health from enemy by {healthPerDrain} points every {idleTime} seconds {drainCount} times.", healthPerDrain, idleTime, drainCount) { }

        /// <summary>
        /// Maps parameters to <see cref="EffectParameter.HealthDrain"/>.
        /// <br/>List Format: [0] = Damage Per Tick, [1] = Total Ticks, [2] = Interval.
        /// </summary>
        public override Dictionary<EffectParameter, List<float>> LogicTransfer()
        {
            float healthPerDrain = Values.Length > 0 ? Values[0] : 0f;
            float idleTime = Values.Length > 1 ? Values[1] : 0f;
            float drainCount = Values.Length > 2 ? Values[2] : 0f;

            return new Dictionary<EffectParameter, List<float>>
            {
                { EffectParameter.HealthDrain, new List<float>{healthPerDrain, drainCount, idleTime } },
            };
        }

        public override int ParamNum() => Values.Length;

        public override Effect Clone() => new HealthDrain(Values[0], Values[1], Values[2]);

        /// <summary>
        /// Creates a new HealthDrain with modified Damage[0], Interval[1], and Count[2].
        /// </summary>
        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 3) newValues = new float[] { Values[0], Values[1], Values[2] };
            return new HealthDrain(newValues[0], newValues[1], newValues[2]);
        }

    }

    /// <summary>
    /// Incapacitates the target for a specific duration (Hard Crowd Control).
    /// </summary>
    public class Stun : Effect
    {
        /// <summary>
        /// Creates a Stun effect.
        /// </summary>
        /// <param name="duration">Duration in seconds.</param>
        public Stun(float duration)
            : base("Stun", $"Stuns the target for {duration} seconds.", duration) { }

        /// <summary>
        /// Maps parameters to <see cref="EffectParameter.StunDuration"/>.
        /// </summary>
        public override Dictionary<EffectParameter, List<float>> LogicTransfer()
        {
            float duration = Values.Length > 0 ? Values[0] : 0f;

            return new Dictionary<EffectParameter, List<float>>
            {
                { EffectParameter.StunDuration, new List<float>{duration} }
            };
        }

        public override int ParamNum() => Values.Length;

        public override Effect Clone() => new Stun(Values[0]);

        /// <summary>
        /// Creates a new Stun with modified duration.
        /// </summary>
        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 1) newValues = new float[] { Values[0] };
            return new Stun(newValues[0]);
        }

    }

    /// <summary>
    /// Modifies the physical or visual size of the target.
    /// </summary>
    public class Scale : Effect
    {
        /// <summary>
        /// Creates a Scaling effect.
        /// </summary>
        /// <param name="factor">The size multiplier (1.0 = normal, 2.0 = double size).</param>
        public Scale(float factor)
            : base("Scale", $"Scales the target {factor} times.", factor) { }

        /// <summary>
        /// Maps parameters to <see cref="EffectParameter.Scaling"/>.
        /// </summary>
        public override Dictionary<EffectParameter, List<float>> LogicTransfer()
        {
            float factor = Values.Length > 0 ? Values[0] : 0f;

            return new Dictionary<EffectParameter, List<float>>
            {
                { EffectParameter.Scaling, new List<float>{factor} }
            };
        }

        public override int ParamNum() => Values.Length;

        public override Effect Clone() => new Scale(Values[0]);

        /// <summary>
        /// Creates a new Scale effect with a modified multiplier.
        /// </summary>
        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 1) newValues = new float[] { Values[0] };
            return new Scale(newValues[0]);
        }

    }

    //TODO: LONG-TERM Write actual effects HERE
}