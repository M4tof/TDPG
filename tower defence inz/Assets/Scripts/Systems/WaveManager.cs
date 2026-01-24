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
    [SerializeField] private float cooldownPeriod = 15f;
    [SerializeField] private float spawnStaggerDelay = 0.8f;

    [SerializeField] private int waveStepup = 3;
    [SerializeField] private int maxEnemyTypeCountPerWaveNumber = 3;
    [SerializeField] private int minEnemyTypeCountPerWaveNumber = 1;

    [Header("Rewards Settings")]
    [SerializeField] private int Reward;
    [SerializeField] private float WaveRewardMultiplier = 5;
    [SerializeField] private bool UpgradesOnEndWave = true;
    [SerializeField] private int numberOfUpgrades = 3;


    [Header("References")]
    // [SerializeField] private EnemyRegistry enemyRegistry;
    [SerializeField] private List<EnemySpawner> spawners; // Assuming a script called EnemySpawner exists
    [SerializeField] private CardSelectionMenu cardSelectionMenu;

    private float _cooldownTimer;
    private bool _isWaveActive = false;
    private bool _isSpawning = false;

    private IntGenerator Gen;
    private Seed WaveSeed;
    public Queue<string> enemyIdsToSpawn = new Queue<string>();

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
        spawners = new List<EnemySpawner>(FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None));
        if (enemyIdsToSpawn.Count > 0)
        {
            ForceEarlySpawn();
        }
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
        enemyIdsToSpawn = GenerateWaveData(currentWaveNumber);

        if (spawners.Count <= 0)
        {
            Debug.Log("[WaveManager] spawners empty. Searching again");
            spawners = new List<EnemySpawner>(FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None));
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
        if (UpgradesOnEndWave)
        {
            cardSelectionMenu.GetNewCard(numberOfUpgrades);
        }

        Debug.Log($"Wave {currentWaveNumber} cleared. Cooldown started.");
    }

    public void ForceSpawnQueue()
    {
        if (_isSpawning && enemyIdsToSpawn.Count > 0)
        {
            StartCoroutine(SpawnWaveRoutine(enemyIdsToSpawn));
        }
        else
        {
            enemyIdsToSpawn.Clear();
        }
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
            if (i * waveStepup < waveNumber)
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

    void OnValidate()
    {
        if (cardSelectionMenu == null)
        {
            Debug.LogWarning("cardSelectionMenu is null", this);
        }
    }

    public WaveSaveData GetSaveData()
    {
        WaveSaveData data = new WaveSaveData
        {
            CurrentWaveNumber = currentWaveNumber,
            CooldownTimer = _cooldownTimer,
            IsWaveActive = _isWaveActive,
            IsSpawning = _isSpawning,
            RemainingEnemyQueue = new Queue<string>()
        };

        // Serialize the Queue
        if (enemyIdsToSpawn != null && enemyIdsToSpawn.Count > 0)
        {
            data.RemainingEnemyQueue = new Queue<string>(enemyIdsToSpawn);
        }

        return data;
    }

    public void LoadFromData(WaveSaveData data)
    {
        // 1. Restore Scalars
        currentWaveNumber = data.CurrentWaveNumber;
        _cooldownTimer = data.CooldownTimer;
        _isWaveActive = data.IsWaveActive;
        // We handle _isSpawning logic below
        
        Debug.Log($"[WaveManager] Restored Wave {currentWaveNumber}. Timer: {_cooldownTimer:F1}s. Active: {_isWaveActive}");

        // 2. Restore Queue
        if (data.RemainingEnemyQueue != null)
        {
            enemyIdsToSpawn = new Queue<string>(data.RemainingEnemyQueue);
        }
        else
        {
            enemyIdsToSpawn = new Queue<string>();
        }

        // 3. Restore State Logic
        // Case A: We were in the middle of spawning enemies
        if (data.IsWaveActive && data.IsSpawning && enemyIdsToSpawn.Count > 0)
        {
            Debug.Log("[WaveManager] Resuming mid-wave spawn routine...");
            StartCoroutine(SpawnWaveRoutine(enemyIdsToSpawn));
        }
        // Case B: Wave is active, but spawning finished (Waiting for player to kill enemies)
        else if (data.IsWaveActive)
        {
            _isSpawning = false; // Ensure flag is correct
            // Update() loop will naturally check AreEnemiesAlive()
        }
        // Case C: Cooldown phase
        else
        {
            _isSpawning = false;
            // Update() loop will naturally decrement _cooldownTimer
        }
        
        // Ensure Spawners are cached (in case Load happened before Start)
        if (spawners == null || spawners.Count == 0)
        {
            spawners = new List<EnemySpawner>(FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None));
        }
    }

    public int GetCurrentWave()
    {
        return currentWaveNumber;
    }
}