using UnityEngine;
using System.Collections.Generic;
using TDPG.Generators.Seed; // Namespace from your lib

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject EnemyPrefab;

    [Header("Runtime")]
    private EnemyFactory _factory;

    void Start()
    {
        InitializeFactory();
    }

    private void InitializeFactory()
    {
        // 2. Initialize Factory with the Lambda Injection
        _factory = new EnemyFactory(GameManager.Instance.GSeed, GameManager.Instance.Slot, (data, overrides) =>
        {
            // This is the bridge: Factory asks for an object, we give the concrete implementation
            return new Enemy(data, overrides);
        });
    }

    // public void SpawnEnemy(string enemyID, int waveDifficulty)
    // {
    //     // 1. Get Data
    //     EnemyData data = EnemyRegistry.Instance.Get(enemyID);
    //     if (data == null) return;

    //     // 2. Generate Logic (Factory)
    //     // Returns EnemyBase, we cast to Enemy because we know our Lambda returns Enemy
    //     Enemy logicalEnemy = (Enemy)_factory.GenerateNextEnemy(data, waveDifficulty);

    //     // TODO: check if this *really* goes here. Gemini claims it does
    //     // 3. Inject Path (Placeholder for now)
    //     // logicalEnemy.SetPath(PathfindingSystem.GetPath(SpawnPoint, EndPoint));
    //     // logicalEnemy.Position = SpawnPoint; 

    //     // 4. Register
    //     EnemyCompendium.Instance.RegisterEnemy(logicalEnemy);

    //     // 5. Create Visuals
    //     GameObject go = Instantiate(EnemyPrefab, transform.position, Quaternion.identity);
    //     go.GetComponent<EnemyBehavior>().Initialize(logicalEnemy);
    // }

    public void SpawnEnemy(string enemyID, int waveDifficulty)
    {
        // Debug 1: Check Registry
        if (EnemyRegistry.Instance == null) { Debug.LogError("❌ Registry is NULL"); return; }

        EnemyData data = EnemyRegistry.Instance.Get(enemyID);
        if (data == null) { Debug.LogError($"❌ Enemy Data '{enemyID}' not found!"); return; }

        // Debug 2: Check Factory (Did Awake run?)
        if (_factory == null) { Debug.LogError("❌ Factory is NULL! InitializeFactory didn't run."); return; }

        Enemy logicalEnemy = (Enemy)_factory.GenerateNextEnemy(data, waveDifficulty);

        // Debug 3: Check Compendium (Is object in scene?)
        if (EnemyCompendium.Instance == null) { Debug.LogError("❌ EnemyCompendium is NULL! Missing GameObject in scene?"); return; }

        EnemyCompendium.Instance.RegisterEnemy(logicalEnemy);

        // ... Visuals ...
        GameObject go = Instantiate(EnemyPrefab, transform.position, Quaternion.identity);
        go.GetComponent<EnemyBehavior>().Initialize(logicalEnemy);
    }

    public void DebugSpawn()
    {
        SpawnEnemy("Walker", 1);
    }
}