using UnityEngine;

public class BarrelCollision : MonoBehaviour
{
    [Header("Particle Effect")]
    public GameObject explosionPrefab;
    
    [Header("Settings")]
    public bool destroyBarrelOnHit = true;
    public float destroyDelay = 0f;
    
    [Header("Explosion Settings")]
    public float explosionRadius = 10f;
    public float explosionForce = 20f;
    public LayerMask enemyLayer;
    
    private bool hasExploded = false;
    
    void OnTriggerEnter(Collider other)
    {
        // Check if collided with player
        if (other.CompareTag("Player") && !hasExploded)
        {
            hasExploded = true;
            Explode(other.transform);
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (!hasExploded)
        {
            hasExploded = true;
            Explode(null);
        }
    }
    
    void Explode(Transform playerTransform)
    {
        // Spawn particle effect at barrel position
        if (explosionPrefab != null)
        {
            Vector3 spawnPosition = transform.position;
            GameObject effect = Instantiate(explosionPrefab, spawnPosition, Quaternion.identity);
            
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(effect, 2f);
            }
        }
        
        // Apply knockback to player only if hit by player
        if (playerTransform != null)
        {
            FirstPersonController playerController = playerTransform.GetComponent<FirstPersonController>();
            if (playerController != null)
            {
                Vector3 knockbackDirection = (playerTransform.position - transform.position).normalized;
                knockbackDirection.y = 1f;
                playerController.SetVelocity(knockbackDirection * explosionForce);
            }
        }
        
        // Kill enemies in explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);
        foreach (Collider col in hitColliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Die();
            }
        }
        
        // Destroy barrel if enabled
        if (destroyBarrelOnHit)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}