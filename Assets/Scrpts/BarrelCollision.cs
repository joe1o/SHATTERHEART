using UnityEngine;

public class BarrelCollision : MonoBehaviour
{
    [Header("Particle Effect")]
    public GameObject explosionPrefab;
    
    [Header("Settings")]
    public bool destroyBarrelOnHit = true;
    public float destroyDelay = 0f;
    
    private bool hasExploded = false; // Prevent multiple explosions
    
    void OnTriggerEnter(Collider other)
    {
        // Check if collided with player
        if (other.CompareTag("Player") && !hasExploded)
        {
            hasExploded = true;
            
            // Spawn particle effect at barrel position
            if (explosionPrefab != null)
            {
                Vector3 spawnPosition = transform.position;
                
                // Instantiate the particle effect
                GameObject effect = Instantiate(explosionPrefab, spawnPosition, Quaternion.identity);
                
                // Destroy the effect after it finishes playing
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
            
            // Destroy barrel if enabled
            if (destroyBarrelOnHit)
            {
                Destroy(gameObject, destroyDelay);
            }
        }
    }
}