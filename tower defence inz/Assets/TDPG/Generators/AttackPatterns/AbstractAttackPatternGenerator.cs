using System;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;

namespace TDPG.Generators.AttackPatterns
{
    [Serializable]
    public abstract class AbstractAttackPatternGenerator : IAttackPatternGenerator
    {
        public FloatGenerator DurationGenerator { get; set; }
        public IntGenerator EventCountGenerator { get; set; }

        public abstract AttackPattern Generate(IRandomSource source);

        public virtual AttackPattern Generate(Seed.Seed seed, string context = null)
        {
            var local = new SplitMix64Random(seed.GetBaseValue());
            return Generate(local);
        }

        public virtual AttackPattern Preview(ulong pseudo)
        {
            return Generate(new SplitMix64Random(pseudo));
        }

        public virtual void Validate()
        {
            if (DurationGenerator == null) throw new InvalidOperationException("DurationGenerator is null");
            if (EventCountGenerator == null) throw new InvalidOperationException("EventCountGenerator is null");
        }
    }
}
