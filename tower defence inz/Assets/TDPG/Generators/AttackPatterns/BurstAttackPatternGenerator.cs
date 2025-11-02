using System;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;
using TDPG.Generators.Vectors;

namespace TDPG.Generators.AttackPatterns
{
    [Serializable]
    public class BurstAttackPatternGenerator : AbstractAttackPatternGenerator
    {
        public FloatGenerator TimeOffsetGenerator { get; set; }
        public VectorGenerator<float> DirectionGenerator { get; set; }
        public FloatGenerator SpeedGenerator { get; set; }
        public IntGenerator DamageGenerator { get; set; }
        public FloatGenerator SpreadAngleGenerator { get; set; }

        public IAttackPatternLayout Layout { get; set; }

        public BurstAttackPatternGenerator()
        {
            // sensible defaults
            DurationGenerator = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0.5f, max = 3f };
            EventCountGenerator = new IntGenerator { min = 1, max = 6 };
            TimeOffsetGenerator = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0f, max = 0.3f };
            DirectionGenerator = new VectorGenerator<float>(new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = -1f, max = 1f }, 2);
            SpeedGenerator = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 1f, max = 6f };
            DamageGenerator = new IntGenerator { min = 1, max = 10 };
            SpreadAngleGenerator = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0f, max = 30f };
            Layout = new BurstLayout();
        }

        public override AttackPattern Generate(IRandomSource source)
        {
            Validate();
            var pattern = new AttackPattern();
            pattern.id = Guid.NewGuid().ToString();
            float duration = DurationGenerator.Generate(source);
            pattern.duration = duration;
            int count = EventCountGenerator.Generate(source);
            pattern.events = Layout.GenerateEvents(source, count, duration, DirectionGenerator, TimeOffsetGenerator, SpeedGenerator, DamageGenerator, SpreadAngleGenerator);
            return pattern;
        }

        public override void Validate()
        {
            base.Validate();
            if (Layout == null) throw new InvalidOperationException("Layout is null");
            if (DirectionGenerator == null) throw new InvalidOperationException("DirectionGenerator is null");
        }
    }
}
