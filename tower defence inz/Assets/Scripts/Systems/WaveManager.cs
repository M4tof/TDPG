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
    [SerializeField] private List<EnemySpawner> spawners;
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Duplicate WaveManager destroyed. Only one instance allowed.");
        }
        else
        {
            Instance = this;
            Debug.Log("WaveManager created.");
        }
    }

    private void Start()
    {
        _cooldownTimer = cooldownPeriod;
        WaveSeed = GameManager.Instance.GSeed.NextSubSeed(InitializerFromDate.QuickGenerate(GameManager.Instance.Slot).ToString());
        spawners = new List<EnemySpawner>(FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None));
        if (enemyIdsToSpawn.Count > 0)
        {
            ForceEarlySpawn();
        }
    }

    private void Update()
    {
        if (_isWaveActive && !_isSpawning)
        {
            if (!AreEnemiesAlive())
            {
                EndWave();
            }
            return;
        }

        // Cooldown Logic
        if (!_isWaveActive && _cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;

            if (_cooldownTimer <= 0)
            {
                StartNextWave();
            }
        }
    }
    public void StartNextWave()
    {
        currentWaveNumber++;
        _cooldownTimer = 0;
        _isWaveActive = true;

        enemyIdsToSpawn = GenerateWaveData(currentWaveNumber);

        if (spawners.Count <= 0)
        {
            Debug.Log("[WaveManager] spawners empty. Searching again");
            spawners = new List<EnemySpawner>(FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None));
        }
        StartCoroutine(SpawnWaveRoutine(enemyIdsToSpawn));

        Debug.Log($"Wave {currentWaveNumber} started!");
    }

    private IEnumerator SpawnWaveRoutine(Queue<string> enemyQueue)
    {
        _isSpawning = true;
        int spawnerIndex = 0;

        while (enemyQueue.Count > 0)
        {
            Debug.Log("[WaveManager] Pushing enemy to the spawner");
            string enemyId = enemyQueue.Dequeue();

            if (spawners.Count > 0)
            {
                var targetSpawner = spawners[spawnerIndex];
                if (targetSpawner != null)
                {
                    targetSpawner.SpawnEnemy(enemyId, currentWaveNumber);
                }

                spawnerIndex = (spawnerIndex + 1) % spawners.Count;
            }

            yield return new WaitForSeconds(spawnStaggerDelay);
        }

        _isSpawning = false;
    }

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

        return waveIds;
    }

    private int CalculateEnemyCount(int wave)
    {
        Gen = new IntGenerator { min = minEnemyTypeCountPerWaveNumber * wave, max = maxEnemyTypeCountPerWaveNumber * wave };
        return Gen.Generate(WaveSeed);
    }

    private bool AreEnemiesAlive()
    {
        return EnemyCompendium.Instance.ActiveEnemies.Count > 0;
    }

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

        if (enemyIdsToSpawn != null && enemyIdsToSpawn.Count > 0)
        {
            data.RemainingEnemyQueue = new Queue<string>(enemyIdsToSpawn);
        }

        return data;
    }

    public void LoadFromData(WaveSaveData data)
    {
        // Restore Scalars
        currentWaveNumber = data.CurrentWaveNumber;
        _cooldownTimer = data.CooldownTimer;
        _isWaveActive = data.IsWaveActive;
        
        // _isSpawning logic
        Debug.Log($"[WaveManager] Restored Wave {currentWaveNumber}. Timer: {_cooldownTimer:F1}s. Active: {_isWaveActive}");

        // Restore Queue
        if (data.RemainingEnemyQueue != null)
        {
            enemyIdsToSpawn = new Queue<string>(data.RemainingEnemyQueue);
        }
        else
        {
            enemyIdsToSpawn = new Queue<string>();
        }

        // Restore State Logic
        // Case A: We were in the middle of spawning enemies
        if (data.IsWaveActive && data.IsSpawning && enemyIdsToSpawn.Count > 0)
        {
            Debug.Log("[WaveManager] Resuming mid-wave spawn routine...");
            StartCoroutine(SpawnWaveRoutine(enemyIdsToSpawn));
        }
        // Case B: Wave is active, but spawning finished (Waiting for player to kill enemies)
        else if (data.IsWaveActive)
        {
            _isSpawning = false;
        }
        // Case C: Cooldown phase
        else
        {
            _isSpawning = false;
        }
        
        if (spawners == null || spawners.Count == 0)
        {
            spawners = new List<EnemySpawner>(FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None));
        }
    }
}