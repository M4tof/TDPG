using UnityEngine;

public abstract class EnemyBase
{
    public EnemyData Data {get; private set;}
    public float CurrentHealth {get; protected set;}
    public float CurrentSpeed {get; protected set;}
    public Vector2 Position {get; set;}
    public EnemyBase(EnemyData data)
    {
        Data = data;    
    }
    public virtual void OnCreation(){}
    public virtual void OnUpdate(){}
    public virtual void OnDeath(){}
    public void ApplyStatus(StatusComponent status)
    {
        
    }
    
    public void DealDamage(float damage)
    {
        CurrentHealth -= damage;
    }

}
