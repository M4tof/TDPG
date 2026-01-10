using UnityEngine;
using TDPG.AudioModulation;
using TDPG.Templates.Turret;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BasicProjectile : Projectile
{
    private int damage = 1;

    
    private ProceduralAudioController _audioController; 
    private float timeRemaining = 0f;

    public void Start()
    {
        base.Start();
        
        _audioController = GetComponent<ProceduralAudioController>();
        
        if (_audioController != null)
        {
            _audioController.Play();
        }
    }
    
    /*void FixedUpdate()
    {
        MoveProjectile();
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            OnDestroy();
        }
    }

    public void MoveProjectile()
    {
        rb.linearVelocity = transform.right * speed;
    }

    public void OnDestroy()
    {
        Destroy(gameObject);
    }*/
    
    public override void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("TRIGGER ENTER: BASIC");
        EnemyBehavior enemyBehavior = other.gameObject.GetComponent<EnemyBehavior>();
        if (enemyBehavior != null)
        {
            enemyBehavior.DealDamage(damage);
        }
        Destroy(gameObject);
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }
    
    public int GetDamage()
    {
        return damage;
    }
}
