using System.Collections.Generic;
using UnityEngine;

namespace TDPG.EffectSystem.ElementLogic
{

    public class SlowDown : Effect
    {
        public SlowDown(float factor) 
            : base("SlowDown", $"Slows down the target by {factor * 100}% indefinitely.", factor) { }

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

        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 1) newValues = new float[] { Values[0] };
            return new SlowDown(newValues[0]);
        }
        
    }

    public class TempSlowDown : Effect
    {
        public TempSlowDown(float factor, float duration)
            : base("SlowDown", $"Slows down the target by {factor * 100}% for {duration} seconds.", factor, duration) { }

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

        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 2) newValues = new float[] { Values[0], Values[1] };
            return new TempSlowDown(newValues[0], newValues[1]);
        }

    }


    public class HealthDown : Effect
    {
        public HealthDown(float amount)
            : base("HealthDown", $"Lowers health of the target by {amount} points, once.", amount) {}


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

        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 1) newValues = new float[] { Values[0] };
            return new Heal(newValues[0]);
        }
        
    }

    public class HealthDrain : Effect
    {
        public HealthDrain(float healthPerDrain, float drainCount, float idleTime)
            : base("HealthDrain", $"Drains health from enemy by {healthPerDrain} points every {idleTime} seconds {drainCount} times.", healthPerDrain, idleTime, drainCount) { }

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

        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 3) newValues = new float[] { Values[0], Values[1], Values[2] };
            return new HealthDrain(newValues[0], newValues[1], newValues[2]);
        }

    }

    public class Stun : Effect
    {
        public Stun(float duration)
            : base("Stun", $"Stuns the target for {duration} seconds.", duration) { }

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

        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 1) newValues = new float[] { Values[0] };
            return new Stun(newValues[0]);
        }

    }

    public class Scale : Effect
    {
        public Scale(float factor)
            : base("Scale", $"Scales the target {factor} times.", factor) { }

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

        public override Effect WithValues(float[] newValues)
        {
            if (newValues.Length < 1) newValues = new float[] { Values[0] };
            return new Scale(newValues[0]);
        }

    }

    //TODO: LONG-TERM Write actual effects HERE
}