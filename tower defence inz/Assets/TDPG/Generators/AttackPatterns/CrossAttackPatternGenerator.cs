using System;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;
using UnityEngine;

namespace TDPG.Generators.AttackPatterns
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Cross Pattern", menuName = "TDPG/Patterns/Cross")]
    public class CrossAttackPatternGenerator : AbstractAttackPatternGenerator
    {
        public FloatGenerator TimeOffsetGenerator { get; set; }
        public FloatGenerator SpeedGenerator { get; set; }
        public IntGenerator DamageGenerator { get; set; }
        public FloatGenerator SpreadAngleGenerator { get; set; }

        public IAttackPatternLayout Layout { get; set; }

        public CrossAttackPatternGenerator()
        {
            patternName = "Cross";
            DurationGenerator = new FloatGenerator { min = 0.5f, max = 2f };
            EventCountGenerator = new IntGenerator { min = 4, max = 12 };
            TimeOffsetGenerator = new FloatGenerator { min = 0f, max = 0.2f };
            SpeedGenerator = new FloatGenerator { min = 2f, max = 6f };
            DamageGenerator = new IntGenerator { min = 1, max = 5 };
            SpreadAngleGenerator = new FloatGenerator { min = 0f, max = 5f };
            Layout = new CrossLayout();
        }

        public override AttackPattern Generate(IRandomSource source)
        {
            Validate();
            var pattern = new AttackPattern
            {
                id = Guid.NewGuid().ToString(),
                duration = DurationGenerator.Generate(source)
            };

            pattern.events = Layout.GenerateEvents(
                source,
                EventCountGenerator.Generate(source),
                pattern.duration,
                null,
                TimeOffsetGenerator,
                SpeedGenerator,
                DamageGenerator,
                SpreadAngleGenerator
            );

            return pattern;
        }
    }
}
