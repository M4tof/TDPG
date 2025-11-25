using UnityEngine;

public abstract class StatusComponent : ScriptableObject
{
    public string StatusName;
    public Sprite StatusIcon;
    public abstract void Apply(EnemyBase enemy);
}