using TDPG.EffectSystem.ElementPlanner;
using UnityEngine;

namespace TDPG.Templates.Turret
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10;
        [SerializeField] private float lifeTime = 2;
        private ElementPlanner planner;
        
        
        private Rigidbody2D rb;
        private float timeRemaining = 0f;

        public void Start()
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
        
        public virtual void MoveProjectile()
        {
            rb.linearVelocity = transform.right * speed;
        }
        
        public virtual void AddElement(string ElementName)
        {
            planner.RegisterElement(ElementName);
            planner.BuildPlan();
        }
        
        public virtual void OnDestroy()
        {
            Destroy(gameObject);
        }
        
        public virtual void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("TRIGGER ENTER: PROJECTILE");
            Destroy(gameObject);
        }

        public ElementPlanner GetPlanner()
        {
            return planner;
        }
        
        public void SetPlanner(ElementPlanner  planner)
        {
            this.planner = planner;
        }
    }
}