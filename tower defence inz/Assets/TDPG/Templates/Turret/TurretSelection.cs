using UnityEngine;

namespace TDPG.Templates.Turret
{
    public class TurretSelection : MonoBehaviour
    {
        [SerializeField] GameObject turretToSpawn;
        [SerializeField] TurretSpawner turretSpawner;

        public void SelectTurret()
        {
            if (turretToSpawn != null)
            {
                turretSpawner.SetTurretToSpawn(turretToSpawn);
            }
        }
    
        void OnValidate()
        {
            if (turretSpawner == null)
            {
                Debug.LogWarning("Turret Spawner is not assigned", this);
            }
            if (turretToSpawn == null)
            {
                Debug.LogWarning("Turret to spawn is null", this);
            }
        }
    }
}
