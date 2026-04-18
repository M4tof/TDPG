using System;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;
using UnityEngine;

namespace TDPG.Generators.AttackPatterns
{
    [CreateAssetMenu(fileName = "New Fan Pattern", menuName = "TDPG/Patterns/Fan")]
    public class FanAttackPatternGenerator : AbstractAttackPatternGenerator
    {
        public FloatGenerator SpeedGenerator { get; set; }
        public IntGenerator DamageGenerator { get; set; }
        public FloatGenerator FanAngleGenerator { get; set; }

        public IAttackPatternLayout Layout { get; set; }

        public FanAttackPatternGenerator()
        {
            patternName = "Fan";
            DurationGenerator = new FloatGenerator { min = 0.5f, max = 1.5f };
            EventCountGenerator = new IntGenerator { min = 3, max = 9 };
            SpeedGenerator = new FloatGenerator { min = 2f, max = 6f };
            DamageGenerator = new IntGenerator { min = 1, max = 4 };
            FanAngleGenerator = new FloatGenerator { min = 30f, max = 120f };
            Layout = new FanLayout();
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
                FanAngleGenerator
            );

            return pattern;
        }
    }
}
