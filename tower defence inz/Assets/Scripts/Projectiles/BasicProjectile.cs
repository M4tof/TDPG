using UnityEngine;
using TDPG.AudioModulation;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BasicProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    [SerializeField] private float lifeTime = 2;
    private int damage = 1;

    private Rigidbody2D rb;
    private ProceduralAudioController _audioController; 
    private float timeRemaining = 0f;

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        timeRemaining = lifeTime; 
        
        _audioController = GetComponent<ProceduralAudioController>();
        
        if (_audioController != null)
        {
            _audioController.Play();
        }
    }
    
    void FixedUpdate()
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
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
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
