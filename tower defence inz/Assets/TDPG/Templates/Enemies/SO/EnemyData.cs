using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "EnemyData", order = 0)]
public class EnemyData : ScriptableObject
{
    [Header("Podstawowe dane")]
    public string EnemyName;
    public float MaxHealth;
    public float Speed;

    [Header("Wygląd")]
    public Sprite EnemySprite;
}