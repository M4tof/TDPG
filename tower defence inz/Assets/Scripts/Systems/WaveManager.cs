using System;
using System.Collections;
using System.Collections.Generic;
using TDPG.Generators.Seed;
using TDPG.Generators.Scalars;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int currentWaveNumber = 0;
    [SerializeField] private float cooldownPeriod = 5f;
    [SerializeField] private float spawnStaggerDelay = 0.5f;

    [SerializeField] private int waveSetup = 3;
    [SerializeField] private int maxEnemyTypeCountPerWaveNumber = 3;
    [SerializeField] private int minEnemyTypeCountPerWaveNumber = 1;

    [SerializeField] private int Reward;
    [SerializeField] private float WaveRewardMultiplier;

    [Header("References")]
    // [SerializeField] private EnemyRegistry enemyRegistry;
    [SerializeField] private List<EnemysSpawner> spawners; // Assuming a script called EnemySpawner exists

    private float _cooldownTimer;
    private bool _isWaveActive = false;
    private bool _isSpawning = false;

    private IntGenerator Gen;
    private Seed WaveSeed;

    public static WaveManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            // If another GameManager already exists, destroy this one
            Destroy(gameObject);
            Debug.LogWarning("Duplicate WaveManager destroyed. Only one instance allowed.");
        }
        else
        {
            // If this is the first GameManager, make it the instance
            Instance = this;
            // Prevents the GameObject from being destroyed when reloading a scene
            Debug.Log("WaveManager created.");
        }
    }

    private void Start()
    {
        // Start the first cooldown immediately
        _cooldownTimer = cooldownPeriod;
        WaveSeed = GameManager.Instance.GSeed.NextSubSeed(InitializerFromDate.QuickGenerate(GameManager.Instance.Slot).ToString());
        // Inside your WaveManager Start or Awake:
        spawners = new List<EnemysSpawner>(FindObjectsByType<EnemysSpawner>(FindObjectsSortMode.None));
    }

    private void Update()
    {
        // Only check for wave completion if a wave is active AND we are done spawning enemies
        if (_isWaveActive && !_isSpawning)
        {
            if (!AreEnemiesAlive())
            {
                EndWave();
            }
            return;
        }

        // Cooldown Logic (only runs when not in a wave)
        if (!_isWaveActive && _cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;

            if (_cooldownTimer <= 0)
            {
                StartNextWave();
            }
        }
    }

    /// <summary>
    /// Prepares and triggers the next wave of enemies.
    /// </summary>
    public void StartNextWave()
    {
        currentWaveNumber++;
        _cooldownTimer = 0;
        _isWaveActive = true;

        // Generate the list of IDs for this wave
        Queue<string> enemyIdsToSpawn = GenerateWaveData(currentWaveNumber);

        if (spawners.Count <= 0)
        {
            Debug.Log("[WaveManager] spawners empty. Searching again");
            spawners = new List<EnemysSpawner>(FindObjectsByType<EnemysSpawner>(FindObjectsSortMode.None));
        }
        // Push the list to all registered spawners
        StartCoroutine(SpawnWaveRoutine(enemyIdsToSpawn));

        Debug.Log($"Wave {currentWaveNumber} started!");
    }

    /// <summary>
    /// Coroutine that handles the staggered release of enemies across available spawners.
    /// </summary>
    private IEnumerator SpawnWaveRoutine(Queue<string> enemyQueue)
    {
        _isSpawning = true;
        int spawnerIndex = 0;

        while (enemyQueue.Count > 0)
        {
            Debug.Log("[WaveManager] Pushing enemy to the spawner");
            string enemyId = enemyQueue.Dequeue();

            // Round-robin selection of spawners
            if (spawners.Count > 0)
            {
                var targetSpawner = spawners[spawnerIndex];
                if (targetSpawner != null)
                {
                    targetSpawner.SpawnEnemy(enemyId, currentWaveNumber);
                }

                // Move to next spawner index, loop back to 0 if at end
                spawnerIndex = (spawnerIndex + 1) % spawners.Count;
            }

            // Wait for the specified delay before spawning the next enemy
            yield return new WaitForSeconds(spawnStaggerDelay);
        }

        _isSpawning = false;
    }

    /// <summary>
    /// Skips the remaining cooldown and starts the next wave immediately.
    /// </summary>
    public void ForceEarlySpawn()
    {
        StartNextWave();
    }

    private void EndWave()
    {
        _isWaveActive = false;
        _cooldownTimer = cooldownPeriod;
        ResourceSystem.Instance.mana.Grant(Reward + WaveRewardMultiplier * currentWaveNumber);
        Debug.Log($"Wave {currentWaveNumber} cleared. Cooldown started.");
    }

    /// <summary>
    /// Logic to determine how many and which enemies to spawn.
    /// Uses placeholder logic for random selection.
    /// </summary>
    private Queue<string> GenerateWaveData(int waveNumber)
    {
        List<string> EnemyIDs = EnemyRegistry.Instance.ListIDs();
        Queue<string> waveIds = new Queue<string>();
        for (int i = 0; i < EnemyIDs.Count; i++)
        {
            if (i * waveSetup < waveNumber)
            {
                for (int j = CalculateEnemyCount(waveNumber); j > 0; j--)
                {
                    waveIds.Enqueue(EnemyIDs[i]);
                }
            }
        }


        // Placeholder for random logic: Determine how many enemies for this wave
        // int enemyCount = CalculateEnemyCount(waveNumber);

        // for (int i = 0; i < enemyCount; i++)
        // {
        //     // Placeholder for random logic: Select a random ID from the registry
        //     string randomId = GetRandomEnemyIdFromRegistry();
        //     waveIds.Enqueue(randomId);
        // }

        return waveIds;
    }

    #region Placeholder Functions

    // I will fill this with your library's utility later
    private int CalculateEnemyCount(int wave)
    {
        Gen = new IntGenerator { min = minEnemyTypeCountPerWaveNumber * wave, max = maxEnemyTypeCountPerWaveNumber * wave };
        return Gen.Generate(WaveSeed);
    }

    // I will fill this with your library's utility later
    // private string GetRandomEnemyIdFromRegistry()
    // {
    //     // Logic to pick an ID based on weights or random chance
    //     // return enemyRegistry.GetRandomID();
    //     return "enemy_basic_id";
    // }

    // Logic to check if any enemies are currently in the scene
    private bool AreEnemiesAlive()
    {
        return EnemyCompendium.Instance.ActiveEnemies.Count > 0;
    }

    #endregion
}