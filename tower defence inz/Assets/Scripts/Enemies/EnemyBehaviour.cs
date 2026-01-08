using TDPG.Templates.Enemies;
using TDPG.Templates.Pathfinding;
using TDPG.Templates.Turret;
using UnityEngine;

[RequireComponent(typeof(EnemyPathFollower))]
public class EnemyBehavior : EnemyBaseBehaviour
{
    public Enemy Logic { get; private set; }
    [SerializeField] private HPBarVisualisation hpBarVisualiation;
    
    private SpriteRenderer _renderer;
    private EnemyPathFollower _enemyPathFollower;
    
    private Vector2 direction = Vector2.zero;

    private bool isDestroyingBuilding = false;
    private GameObject buildingToDestroy;
    
    //Attack Cooldown
    private float passedTime = 0;

    public void Initialize(Enemy logic)
    {
        base.Initialize(logic);
        
        Logic = logic;
        _renderer = GetComponent<SpriteRenderer>();
        _enemyPathFollower = GetComponent<EnemyPathFollower>();
        _enemyPathFollower.attackBuilding.AddListener(StartDestroyBuilding);

        // 1. Setup Visuals
        _renderer.sprite = Logic.Data.EnemySprite;
        gameObject.name = $"{Logic.Data.EnemyName}_{Logic.GetHashCode()}";

        // 2. Setup Initial Position
        transform.position = Logic.Position;

        // 3. Trigger Creation Logic
        Logic.OnCreation();
        
        // 4. Set HP Bar Value
        hpBarVisualiation.Init(logic.Data.MaxHealth);
    }

    void Update()
    {
        if (isDestroyingBuilding)
        {
            if (buildingToDestroy == null)
            {
                FinishDestroyingBuilding();
                return;
            }
            passedTime += Time.deltaTime;
            if (passedTime > Logic.GetCurrentAttackSpeed())
            {
                AttackTurret();
                passedTime = 0;
            }
            return;
        }
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
        ResourceSystem.Instance.mana.Grant(Logic.GetReward());
        base.Die();
    }

    public override void DealDamage(int damage)
    {
        Debug.Log("NEW DAMAGE");
        base.DealDamage(damage);
        hpBarVisualiation.SetValue(GetCurrentHealth());
    }

    public void AttackTurret()
    {
        //_enemyPathFollower.
        
        if (buildingToDestroy == null)
        {
            FinishDestroyingBuilding();
            return;
        }

        TurretBase turret = buildingToDestroy.GetComponent<TurretBase>();
        if (turret != null)
        {
            turret.DealDamage(1);
            Debug.Log($"ENEMY attack turret {Logic.GetCurrentDamage()}");
            return;
        }
        FinishDestroyingBuilding();
    }
    
    public override void SetCurrentSpeed(float speed)
    {
        Logic.SetCurrentSpeed(speed);
    }
    
    public float GetCurrentHealth()
    {
        return Logic.GetCurrentHealth();
    }

    private void StartDestroyBuilding()
    {
        buildingToDestroy = _enemyPathFollower.GetBuildingToDestroy();
        Debug.Log($"ENEMY START {buildingToDestroy}");
        isDestroyingBuilding = true;
    }

    private void FinishDestroyingBuilding()
    {
        isDestroyingBuilding = false;
        buildingToDestroy = null;
        passedTime = 0;
        _enemyPathFollower.SetIsDestroyingBuilding(false);
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