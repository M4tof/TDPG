using UnityEngine;
using System.Collections.Generic;
using TDPG.Templates.Enemies;
using TDPG.Templates.Grid;
using TDPG.Templates.Pathfinding;
using TDPG.AudioModulation;
using TDPG.VideoGeneration;
using System.Security.Cryptography;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject EnemyPrefabWalking;
    public GameObject EnemyPrefabFlying;
    public GameObject EnemyPrefabSwimming;
    public Transform EndPoint;

    [Header("Runtime")]
    private EnemyFactory _factory;

    void Start()
    {
        EndPoint = GridManager.Instance.GetDestinationObject().transform;
        InitializeFactory();
    }

    void Update()
    {
        
    }

    private void InitializeFactory()
    {
        _factory = new EnemyFactory(GameManager.Instance.GSeed, GameManager.Instance.Slot, (data, overrides) =>
        {
            return new Enemy(data, overrides);
        });
    }
    public void SpawnEnemy(string enemyID, int waveDifficulty, bool debug = false)
    {
        if (EnemyRegistry.Instance == null) { Debug.LogError("Registry is NULL"); return; }

        EnemyData data = EnemyRegistry.Instance.Get(enemyID);
        if (data == null) { Debug.LogError($"Enemy Data '{enemyID}' not found!"); return; }

        if (_factory == null) { Debug.LogError("Factory is NULL! InitializeFactory didn't run."); return; }

        Enemy logicalEnemy = (Enemy)_factory.GenerateNextEnemy(data, waveDifficulty);

        logicalEnemy.Position = transform.position;

        GameObject Prefab;
        if (data.CanFly)
        {
            Prefab = EnemyPrefabFlying;
        }
        else if (data.CanSwim)
        {
            Prefab = EnemyPrefabSwimming;
        }
        else
        {
            Prefab = EnemyPrefabWalking;
        }

        if (EnemyCompendium.Instance == null) { Debug.LogError("EnemyCompendium is NULL! Missing GameObject in scene?"); return; }

        EnemyCompendium.Instance.RegisterEnemy(logicalEnemy);

        GameObject go = Instantiate(Prefab, transform.position, Quaternion.identity);
        float cellSize = GridManager.Instance.CellSize;
        if (go.TryGetComponent(out BoxCollider2D col))
        {
            col.size *= cellSize;
        }
        go.GetComponent<EnemyBehavior>().Initialize(logicalEnemy);
        go.GetComponent<EnemyPathFollower>().Initialize(GridManager.Instance, EndPoint.gameObject);

        var cs = go.GetComponentInChildren<BaseColorSwapController>();
        cs.SetSeed(GameManager.Instance.CSSeed);


        var ac = go.GetComponentInChildren<ProceduralAudioController>();
        ac.selectionSeed = GameManager.Instance.ACSeed1.GetBaseValue();
        ac.modulationSeed = GameManager.Instance.ACSeed2.GetBaseValue();
    }

    public void DebugSpawn()
    {
        SpawnEnemy("Walker", 1, true);
    }

    public void ForceSpawnEnemy(EnemySaveData save)
    {
        EnemyData data = EnemyRegistry.Instance.Get(save.EnemyID);
        if (data == null) return;
        GameObject Prefab;
        if (data.CanFly)
        {
            Prefab = EnemyPrefabFlying;
        }
        else if (data.CanSwim)
        {
            Prefab = EnemyPrefabSwimming;
        }
        else
        {
            Prefab = EnemyPrefabWalking;
        }
        var logic = new Enemy(data, save.Ov);
        logic.CurrentHealth = save.Health;
        logic.CurrentDamage = save.Damage;
        logic.CurrentAttackSpeed = save.AttackSpeed;
        logic.Position = save.Position;
        Debug.Log($"[ForceSpawn] Restoring Enemy {save.EnemyID} at {save.Position}");

        GameObject go = Instantiate(Prefab, save.Position, Quaternion.identity);

        go.transform.position = save.Position;

        if (TDPG.Templates.Grid.GridManager.Instance != null && go.TryGetComponent(out BoxCollider2D col))
        {
            col.size *= TDPG.Templates.Grid.GridManager.Instance.CellSize;
        }

        if (go.TryGetComponent(out EnemyBehavior behavior))
        {
            behavior.Initialize(logic);
        }

        if (go.TryGetComponent(out TDPG.Templates.Pathfinding.EnemyPathFollower follower))
        {
            if (EndPoint != null && TDPG.Templates.Grid.GridManager.Instance != null)
            {
                // Triggers calculation from CURRENT position to EndPoint
                follower.Initialize(TDPG.Templates.Grid.GridManager.Instance, EndPoint.gameObject);
                follower.ComputeNewPath();
            }
        }

        var cs = go.GetComponentInChildren<BaseColorSwapController>();
        cs.SetSeed(GameManager.Instance.CSSeed);


        var ac = go.GetComponentInChildren<ProceduralAudioController>();
        ac.selectionSeed = GameManager.Instance.ACSeed1.GetBaseValue();
        ac.modulationSeed = GameManager.Instance.ACSeed2.GetBaseValue();
        ac.GenerateAndPlay();
        EnemyCompendium.Instance.RegisterEnemy(logic);
    }

    public void SetEndPoint(Transform transform)
    {
        EndPoint = transform;
    }
}