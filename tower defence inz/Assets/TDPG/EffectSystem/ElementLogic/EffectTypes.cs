using System.Collections.Generic;
using UnityEngine;

namespace TDPG.EffectSystem.ElementLogic
{

    public class SlowDown : Effect
    {
        public SlowDown(float factor, float duration) 
            : base("SlowDown", $"Slows down the target by {factor * 100}% for {duration} seconds.", factor, duration) { }

        public override void Apply(GameObject target)
        {
            // TODO: Implement effect behavior in gameplay
        }

        public override Dictionary<EffectParameter, float> LogicTransfer()
        {
            float factor = Values.Length > 0 ? Values[0] : 0f;
            float duration = Values.Length > 1 ? Values[1] : 0f;

            return new Dictionary<EffectParameter, float>
            {
                { EffectParameter.SlowdownFactor, factor },
                { EffectParameter.Duration, duration }
            };
        }

        public override int ParamNum() => Values.Length;

        public override Effect Clone() => new SlowDown(Values[0], Values[1]);

        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 2) newValues = new float[] { Values[0], Values[1] };
            return new SlowDown(newValues[0], newValues[1]);
        }
        
    }


    public class HealthDown : Effect
    {
        public HealthDown(float amount)
            : base("HealthDown", $"Lowers health of the target by {amount} points, once.", amount) {}

        public override void Apply(GameObject target)
        {
            // TODO: Implement actual effect on target
        }

        public override Dictionary<EffectParameter, float> LogicTransfer()
        {
            float amount = Values.Length > 0 ? Values[0] : 0f;
            return new Dictionary<EffectParameter, float>
            {
                { EffectParameter.HealthChange, -amount }
            };
        }

        public override int ParamNum() => Values.Length;

        public override Effect Clone() => new HealthDown(Values[0]);

        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 1) newValues = new float[] { Values[0] };
            return new HealthDown(newValues[0]);
        }
        
    }

    public class Heal : Effect
    {
        public Heal(float amount)
            : base("Heal", $"Heals the target by {amount} points, once.", amount) {}

        public override void Apply(GameObject target)
        {
            // TODO: Implement actual heal effect
        }

        public override Dictionary<EffectParameter, float> LogicTransfer()
        {
            float amount = Values.Length > 0 ? Values[0] : 0f;
            return new Dictionary<EffectParameter, float>
            {
                { EffectParameter.HealthChange, amount }
            };
        }

        public override int ParamNum() => Values.Length;

        public override Effect Clone() => new Heal(Values[0]);

        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 1) newValues = new float[] { Values[0] };
            return new Heal(newValues[0]);
        }
        
    }
    
    //TODO: LONG-TERM Write actual effects HERE
}