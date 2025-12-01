using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BasicProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    [SerializeField] private float lifeTime = 2;

    private Rigidbody2D rb;
    private float timeRemaining = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        timeRemaining = lifeTime; 
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
    
    private void OnTriggerEnter(Collider other)
    {
        // Kod wykonywany przy wejściu w trigger
        Debug.Log("Obiekt " + other.gameObject.name + " wszedł w trigger");
    }
}
