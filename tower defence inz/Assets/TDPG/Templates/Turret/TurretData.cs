using UnityEngine;

namespace TDPG.Templates.Turret
{
    [CreateAssetMenu(fileName = "NewTurret", menuName = "TD/Turret Data")]
    public class TurretData : ScriptableObject
    {
        public string TurretID; // Key for Serialization/Registry

        [Header("Visuals")]
        public Sprite BaseSprite;    // The static base
        public Sprite CrystalSprite; // The rotating/color-swapped part

        [Header("Building")]
        public float Cost = 50f;
        public Vector2 TileSize = new Vector2(1, 1);
        public float Multiplayer;

        [Header("Combat")]
        public float Range = 5f;
        public float FireRate = 1f; // Shots per second
        public float RotationSpeed = 10f; // For visual tracking

        [Header("Projectile")]
        // We reference the GameObject containing BasicProjectile script
        public GameObject ProjectilePrefab; 
    }
}