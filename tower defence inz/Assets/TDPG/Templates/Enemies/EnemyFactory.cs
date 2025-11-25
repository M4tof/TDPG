using UnityEngine;
using TDPG.Generators.Seed;
using TDPG.Generators.Scalars;
using System;

public class EnemyFactory
{
    private Seed EnemySeed;
    
    private FloatGenerator Gen;
    private float Difficulty;
    private readonly Func<EnemyData, EnemyStatsOverride, EnemyBase> _creationStrategy;

    public EnemyFactory(GlobalSeed gs, int slot, Func<EnemyData, EnemyStatsOverride, EnemyBase> createStrategy)
    {
        _creationStrategy = createStrategy;
        EnemySeed = gs.NextSubSeed(InitializerFromDate.QuickGenerate(slot).ToString());
        Difficulty = 1f;
        Gen = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 1f, max = Difficulty };
    }

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
