using System.Collections.Generic;
using UnityEngine;

namespace TDPG.EffectSystem.Element
{

    public class SlowDown : Effect
    {
        // Expect two values: factor (Values[0]) and duration (Values[1])
        public SlowDown(float factor, float duration) 
            : base("SlowDown", $"Slows down the target by {factor * 100}% for {duration} seconds.", factor, duration) { }

        public override void Apply(GameObject target)
        {
            float factor = Values.Length > 0 ? Values[0] : 0f;
            float duration = Values.Length > 1 ? Values[1] : 0f;

            // Example usage — TODO: replace!!!!
            // var movement = target.GetComponent<Movement>();
            // if (movement != null)
            // {
            //     movement.Speed *= 1f - factor;
            //     StartCoroutine(RestoreSpeedAfterDelay(movement, duration));
            // }
        }

        // Return a dict describing the effect and its values
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
    }


    public class HealthDown : Effect
    {
        public HealthDown(float factor) : base("HealthDown", $"Lower health of the target by {factor} points, once.", factor) {}

        public override void Apply(GameObject target)
        {
            //e.g., target.GetComponent<Health>().Value - target;
        }

        public override Dictionary<EffectParameter, float> LogicTransfer()
        {
            float amount = Values.Length > 0 ? Values[0] : 0f;
            return new Dictionary<EffectParameter, float>
            {
                { EffectParameter.HealthChange, -amount }
            };
        }

    }

    public class Heal : Effect
    {
        public Heal(float factor) : base("Heal",$"Heal the target by {factor} points, once." , factor) {}

        public override void Apply(GameObject target)
        {
            //e.g., target.GetComponent<Health>().Value + Value;
        }

        public override Dictionary<EffectParameter, float> LogicTransfer()
        {
            float amount = Values.Length > 0 ? Values[0] : 0f;
            return new Dictionary<EffectParameter, float>
            {
                { EffectParameter.HealthChange, amount }
            };
        }

    }
    
    //TODO: Write actual effects HERE
}