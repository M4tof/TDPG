using System;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;
using UnityEngine;

namespace TDPG.Generators.AttackPatterns
{
    [CreateAssetMenu(fileName = "New Spiral Pattern", menuName = "TDPG/Patterns/Spiral")]
    public class SpiralAttackPatternGenerator : AbstractAttackPatternGenerator
    {
        public FloatGenerator SpeedGenerator { get; set; }
        public IntGenerator DamageGenerator { get; set; }

        public IAttackPatternLayout Layout { get; set; }

        public SpiralAttackPatternGenerator()
        {
            patternName = "Spiral";
            DurationGenerator = new FloatGenerator { min = 1.5f, max = 4f };
            EventCountGenerator = new IntGenerator { min = 12, max = 36 };
            SpeedGenerator = new FloatGenerator { min = 2f, max = 5f };
            DamageGenerator = new IntGenerator { min = 1, max = 3 };
            Layout = new SpiralLayout();
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
                null,
                SpeedGenerator,
                DamageGenerator,
                null
            );

            return pattern;
        }
    }
}
