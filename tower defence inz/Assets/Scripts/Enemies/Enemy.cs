using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class Enemy : EnemyBase
{
    // Saved Data
    public string EnemyID; // "Goblin"
    public EnemyStatsOverride Overrides;

    // Runtime Only (Re-assigned on Load)
    [System.NonSerialized] private EnemyData _baseData;

    // Movement State
    private Queue<Vector2> _path;
    private Vector2? _currentTarget;

    public Vector2? CurrentTarget => _currentTarget;

    public float CurrentHealth;
    public float CurrentSpeed;

    public Enemy(EnemyData baseData, EnemyStatsOverride overrides) : base(baseData)
    {
        _baseData = baseData;
        EnemyID = baseData.EnemyName;
        Overrides = overrides;

        // Initialize State
        CurrentHealth = baseData.MaxHealth * overrides.HealthMultiplier;
        CurrentSpeed = baseData.Speed * overrides.SpeedMultiplier;
        // Position set by Spawner...
    }

    // Called after loading from JSON to reconnect the SO
    public void RestoreReference(EnemyData data)
    {
        _baseData = data;
    }

    // TODO: double check
    public void SetPath(IEnumerable<Vector2> pathPoints)
    {
        _path = new Queue<Vector2>(pathPoints);
        if (_path.Count > 0)
        {
            Position = _path.Peek(); // Snap to start
            GetNextTarget();
        }
    }

    public override void OnUpdate()
    {
        // DeltaTime handling: Logic usually assumes a fixed step or needs DT passed in.
        // If OnUpdate() doesn't take float dt, we assume Time.deltaTime (which breaks strict lib separation)
        // OR we change the signature in EnemyBase to OnUpdate(float deltaTime).
        Move(Time.deltaTime);
    }

    private void Move(float deltaTime)
    {
        if (_currentTarget == null) return;

        // Move towards target
        float step = CurrentSpeed * deltaTime;
        Position = Vector2.MoveTowards(Position, _currentTarget.Value, step);

        // Check if reached
        if (Vector2.Distance(Position, _currentTarget.Value) < 0.01f)
        {
            GetNextTarget();
        }
    }
    private void GetNextTarget()
    {
        if (_path != null && _path.Count > 0)
            _currentTarget = _path.Dequeue();
        else
            _currentTarget = null; // Reached end of path
    }

    public void ApplyDebugPath(Vector2 startOrigin)
    {
        // Create a hardcoded square path relative to spawn
        var debugPoints = new List<Vector2>
        {
        startOrigin,                         // Start
        startOrigin + new Vector2(2, 0),     // Right 2
        startOrigin + new Vector2(2, -2),    // Down 2
        startOrigin + new Vector2(0, -2),    // Left 2
        startOrigin + new Vector2(0, 0)      // Back to Start
        };

        // Reuse the existing SetPath logic
        SetPath(debugPoints);
        Debug.Log($"[Enemy] Debug path applied with {debugPoints.Count} nodes.");
    }
}
