using UnityEngine;

[CreateAssetMenu(fileName = "Assets", menuName = "ScriptableObjects/TurretList")]
public class TurretList : ScriptableObject
{
    public GameObject[] turretPrefab;
}
