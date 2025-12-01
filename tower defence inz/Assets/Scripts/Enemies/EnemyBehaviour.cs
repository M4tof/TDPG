using TDPG.Templates.Pathfinding;
using UnityEngine;

[RequireComponent(typeof(EnemyPathFollower))]
public class EnemyBehavior : MonoBehaviour
{
    public Enemy Logic { get; private set; }
    private SpriteRenderer _renderer;
    private EnemyPathFollower _enemyPathFollower;
    
    private Vector2 direction = Vector2.zero;

    public void Initialize(Enemy logic)
    {
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
        //Debug.Log($"NEXT TARGET: {target}");
        direction = Vector2.MoveTowards(transform.position, target, 10 * Time.deltaTime);
        transform.position = direction;
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

    private void Die()
    {
        Logic.OnDeath();
        EnemyCompendium.Instance.UnregisterEnemy(Logic);
        Destroy(gameObject); // TODO: Replace with Object Pooling
    }

    public void DealDamage(float damage)
    {
        Logic.DealDamage(damage);
        if (Logic.CurrentHealth <= 0)
        {
            Die();
        }
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