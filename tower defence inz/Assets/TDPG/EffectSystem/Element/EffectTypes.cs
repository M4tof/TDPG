using UnityEngine;

namespace TDPG.EffectSystem.Element
{

    public class SlowDown : Effect
    {
        public SlowDown(float factor) : base("SlowDown", factor,
            $"Slows down the target by {factor}%") {}

        public override void Apply(GameObject target)
        {
            // e.g., target.GetComponent<Movement>().Speed *= 1 - Value;
        }
    }


    public class HealthDown : Effect
    {
        public HealthDown(float factor) : base("HealthDown", factor,
            $"Lower health of the  target by {factor} points, once") {}

        public override void Apply(GameObject target)
        {
            //e.g., target.GetComponent<Health>().Value - Value;
        }
    }

    public class Heal : Effect
    {
        public Heal(float factor) : base("Heal", factor,
            $"Heal the target by {factor} points") {}

        public override void Apply(GameObject target)
        {
            //e.g., target.GetComponent<Health>().Value + Value;
        }
    }
    
    //Write actual effects HERE
}