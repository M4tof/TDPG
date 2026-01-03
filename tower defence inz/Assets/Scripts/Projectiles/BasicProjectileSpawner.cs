using UnityEngine;

public class BasicProjectileSpawner : MonoBehaviour
{
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private GameObject projectile;

    private float timer;
    private bool reloading = false;

    private void Update()
    {
        if (reloading)
        {
            timer += Time.deltaTime;
            if(timer > fireRate)
            {
                reloading = false;
                timer = 0;
            }
        }
    }
    
    public void Shoot(Vector2 spawnPosition, Vector2 mousePosition)
    {
        if (reloading)
        {
            return;
        }
        Vector3 rotationPosition = (Vector2) mousePosition - (Vector2) transform.position;
        float rotZ = Mathf.Atan2(rotationPosition.y, rotationPosition.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(projectile, spawnPosition, Quaternion.Euler(0f, 0f, rotZ));
        bullet.GetComponent<BasicProjectile>().SetDamage(projectileDamage);
        reloading = true;
    }
}
