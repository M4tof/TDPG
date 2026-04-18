using System;
using TDPG.Generators.Interfaces;
using TDPG.Generators.Scalars;
using TDPG.Generators.Vectors;
using UnityEngine;

namespace TDPG.Generators.AttackPatterns
{
    /// <summary>
    /// A concrete generator configured to produce "Burst" style patterns (e.g., Shotguns, Multi-missile Volleys).
    /// <br/>
    /// It composes multiple scalar generators to randomize the properties of every individual projectile 
    /// within the burst.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Burst Pattern", menuName = "TDPG/Patterns/Burst")]
    public class BurstAttackPatternGenerator : AbstractAttackPatternGenerator
    {
        

        /// <summary>
        /// Generates the specific start time for each projectile relative to the pattern start.
        /// <br/>For bursts, these values are typically very small (close to 0.0).
        /// </summary>
        public FloatGenerator TimeOffsetGenerator { get; set; }
        
        /// <summary>
        /// Generates the base direction vector for the attack events.
        /// <br/>Default is initialized for 2D (X, Y).
        /// </summary>
        public VectorGenerator<float> DirectionGenerator { get; set; }
        
        /// <summary>
        /// Generates the travel speed for individual projectiles.
        /// </summary>
        public FloatGenerator SpeedGenerator { get; set; }
        
        /// <summary>
        /// Generates the damage value for individual projectiles.
        /// </summary>
        public IntGenerator DamageGenerator { get; set; }
        
        /// <summary>
        /// Generates the angular deviation (in degrees) to apply to the base direction.
        /// </summary>
        public FloatGenerator SpreadAngleGenerator { get; set; }

        /// <summary>
        /// The strategy used to arrange the events.
        /// <br/>Defaults to <see cref="BurstLayout"/>.
        /// </summary>
        public IAttackPatternLayout Layout { get; set; }

        /// <summary>
        /// Initializes the generator with sensible defaults for a standard 2D shotgun-style burst.
        /// <br/>
        /// <b>Defaults:</b> 1-6 projectiles, 0.5-3s duration, 2D random direction, 30-degree spread.
        /// </summary>
        public BurstAttackPatternGenerator()
        {
            // Sensible default values
            patternName = "Burst";
            DurationGenerator = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0.5f, max = 3f };
            EventCountGenerator = new IntGenerator { min = 1, max = 6 };
            TimeOffsetGenerator = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0f, max = 0.3f };
            DirectionGenerator = new VectorGenerator<float>(new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = -1f, max = 1f }, 2);
            SpeedGenerator = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 1f, max = 6f };
            DamageGenerator = new IntGenerator { min = 1, max = 10 };
            SpreadAngleGenerator = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 0f, max = 30f };
            Layout = new BurstLayout();
        }

        /// <summary>
        /// Orchestrates the generation of a complete <see cref="AttackPattern"/>.
        /// <br/>
        /// Validates inputs, generates the unique ID and macro-properties (Duration, Count), 
        /// and delegates event creation to the <see cref="Layout"/>.
        /// </summary>
        /// <param name="source">The entropy source.</param>
        /// <returns>A populated AttackPattern instance.</returns>
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
