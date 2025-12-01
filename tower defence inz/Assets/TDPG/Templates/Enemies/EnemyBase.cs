using UnityEngine;

namespace TDPG.Templates.Enemies
{
    public abstract class EnemyBase
    {
        public EnemyData Data { get; private set; }
        public float CurrentHealth { get; set; }
        public float CurrentSpeed { get; set; }
        public Vector2 Position { get; set; }
        public EnemyBase(EnemyData data)
        {
            Data = data;
        }
        public virtual void OnCreation() { }
        public virtual void OnUpdate() { }
        public virtual void OnDeath() { }
        public void ApplyStatus(StatusComponent status)
        {

        }

    }
}