using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public Enemy Logic { get; private set; }
    private SpriteRenderer _renderer;

    public void Initialize(Enemy logic)
    {
        Logic = logic;
        _renderer = GetComponent<SpriteRenderer>();

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
        if (Logic == null) return;

        // 1. Run Logic (Movement, Status Effects)
        Logic.OnUpdate();

        // 2. Sync Unity Transform to Logic Position
        transform.position = Logic.Position;
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
}