using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    public EnemyFactory Instance {get; private set;}
    private Seed EnemySeed;
    
    private FloatGenerator Gen;
    private float Difficulty;

    
    void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            // If another GameManager already exists, destroy this one
            Destroy(gameObject);
            Debug.LogWarning("Duplicate EnemyFactory destroyed. Only one instance allowed.");
        }
        else
        {
            // If this is the first GameManager, make it the instance
            Instance = this;
            // Prevents the GameObject from being destroyed when reloading a scene
            DontDestroyOnLoad(gameObject);
            Debug.Log("EnemyFactory created and set to not destroy on load.");

        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Init(GlobalSeed gs, int slot)
    {
        EnemySeed = gs.NextSubSeed(InitializerFromData.QuickGenerate(slot));
        Difficulty = 1f;
        Gen = new FloatGenerator { mode = FloatGenerator.Mode.Uniform, min = 1f, max = Difficulty };
    }

    public Enemy GenerateNextEnemy(EnemyData template, int waveDifficulty)
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

        return new Enemy(template, overrides);
    }
}
