using TDPG.Templates.Enemies;
using TDPG.Templates.Pathfinding;
using UnityEngine;

[RequireComponent(typeof(EnemyPathFollower))]
public class EnemyBehavior : EnemyBaseBehaviour
{
    public Enemy Logic { get; private set; }
    private SpriteRenderer _renderer;
    private EnemyPathFollower _enemyPathFollower;
    
    private Vector2 direction = Vector2.zero;

    public void Initialize(Enemy logic)
    {
        base.Initialize(logic);
        
        Logic = logic;
        _renderer = GetComponent<SpriteRenderer>();
        _enemyPathFollower = GetComponent<EnemyPathFollower>();

        // 1. Setup Visuals
        _renderer.sprite = Logic.Data.EnemySprite;
        gameObject.name = $"{Logic.Data.EnemyName}_{Logic.GetHashCode()}";

        // 2. Setup Initial Position
        transform.position = Logic.Position;

        // 3. Trigger Creation Logic
        Logic.OnCreation();
    }

    void Update()
    {
        Vector3 target = _enemyPathFollower.GetTargetPosition();
        direction = Vector2.MoveTowards(transform.position, target, Logic.GetCurrentSpeed() * Time.deltaTime);
        transform.position = direction;
        Logic.Position = transform.position;
        MoveDirection();
    }

    void MoveDirection()
    {
        if (direction.x > 0)
        {
            _renderer.flipX = true;
        }
        else if (direction.x < 0)
        {
            _renderer.flipX = false;
        }
    }

    public override void Die()
    {
        EnemyCompendium.Instance.UnregisterEnemy(Logic);
        base.Die();
    }
    
    public override void SetCurrentSpeed(float speed)
    {
        Logic.SetCurrentSpeed(speed);
    }
    
    public float GetCurrentHealth()
    {
        return Logic.GetCurrentHealth();
    }
    
    void OnDrawGizmos()
    {
        // Only draw if we have a target
        if (Logic != null && Logic.CurrentTarget.HasValue)
        {
            Gizmos.color = Color.green;
            // Draw line to current target
            Gizmos.DrawLine(transform.position, Logic.CurrentTarget.Value);
            // Draw sphere at target
            Gizmos.DrawWireSphere(Logic.CurrentTarget.Value, 0.2f);
        }
    }
}