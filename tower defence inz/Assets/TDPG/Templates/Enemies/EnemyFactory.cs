using UnityEngine;
using TDPG.Generators.Seed;
using TDPG.Generators.Scalars;
using System;

namespace TDPG.Templates.Enemies { 
    /// <summary>
    /// A factory responsible for instantiating <see cref="EnemyBase"/> logic models with procedurally generated stats.
    /// <br/>
    /// It scales enemy attributes (Health, Speed) based on a difficulty parameter and a deterministic seed.
    /// </summary>
    public class EnemyFactory
    {
        private Seed EnemySeed;
        
        private FloatGenerator Gen;
        private float Difficulty;
        
        /// <summary>
        /// Delegate function that handles the actual instantiation of the concrete EnemyBase class.
        /// </summary>
        private readonly Func<EnemyData, EnemyStatsOverride, EnemyBase> _creationStrategy;

        /// <summary>
        /// Initializes the factory with a specific random stream and creation logic.
        /// </summary>
        /// <param name="gs">The Global Seed manager (source of entropy).</param>
        /// <param name="slot">A unique slot index (e.g. Save Slot) to salt the RNG, ensuring different runs feel different.</param>
        /// <param name="createStrategy">
        /// A function that takes Data + Overrides and returns a new EnemyBase instance. 
        /// <br/>Allows the factory to create different subclasses (e.g. Boss vs Minion) without hardcoding types.
        /// </param>
        public EnemyFactory(GlobalSeed gs, int slot, Func<EnemyData, EnemyStatsOverride, EnemyBase> createStrategy)
        {
            _creationStrategy = createStrategy;
            EnemySeed = gs.NextSubSeed(InitializerFromDate.QuickGenerate(slot).ToString());
            Difficulty = 1f;
            Gen = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 1f, max = Difficulty };
        }

        /// <summary>
        /// Generates a new enemy logic instance with stats scaled by the provided wave difficulty.
        /// </summary>
        /// <param name="template">The base configuration (Sprite, Name, Base HP).</param>
        /// <param name="waveDifficulty">
        /// The current difficulty scalar. 
        /// <br/>Example: If 2.0, stats can roll between 1.0x and 2.0x base values.
        /// </param>
        /// <returns>A fully initialized <see cref="EnemyBase"/> object.</returns>
        public EnemyBase GenerateNextEnemy(EnemyData template, int waveDifficulty)
        {
            if (waveDifficulty != Difficulty)
            {
                Difficulty = waveDifficulty;
                Gen = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 1f, max = Difficulty };
            }
            
            var overrides = new EnemyStatsOverride
            {
                HealthMultiplier = Gen.Generate(EnemySeed),
                SpeedMultiplier = Gen.Generate(EnemySeed)
            };

            return _creationStrategy(template, overrides);
        }
    }

}