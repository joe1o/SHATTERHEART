using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 30f;
    public int damage = 1;
    public float lifetime = 5f;
    public GameObject hitEffectPrefab;
    public GameObject trailEffect; // Particle effect that follows bullet
    
    private Vector3 direction;
    private Transform target;
    private bool hasHit = false;
    private Transform shooter; // Enemy that shot this bullet
    
    void Start()
    {
        // Ensure bullet has necessary components for collision detection
        SetupBulletComponents();
        
        // Destroy bullet after lifetime expires
        Destroy(gameObject, lifetime);
    }
    
    void SetupBulletComponents()
    {
        // Add or ensure collider exists
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Add a small sphere collider if none exists
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 0.1f;
        }
        else
        {
            // Make sure collider is a trigger for OnTriggerEnter to work
            col.isTrigger = true;
        }
        
        // Add Rigidbody for better physics interaction (optional but recommended)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // We control movement manually
            rb.useGravity = false;
        }
    }
    
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
        // Rotate bullet to face direction
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }
    
    void Update()
    {
        if (hasHit) return;
        
        // Move bullet forward
        transform.position += direction * speed * Time.deltaTime;
    }
    
    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.collider);
    }
    
    public void SetShooter(Transform shooterTransform)
    {
        shooter = shooterTransform;
    }
    
    void HandleCollision(Collider other)
    {
        if (hasHit) return;
        
        // Don't hit the enemy that shot this bullet
        if (shooter != null && (other.transform == shooter || other.transform.IsChildOf(shooter)))
        {
            return;
        }
        
        // Don't hit other enemies
        if (other.CompareTag("enemy")) return;
        
        // Check if it's the player (multiple ways to detect)
        bool isPlayer = other.CompareTag("Player") || 
                        other.transform.root.CompareTag("Player") ||
                        other.GetComponent<PlayerHealth>() != null ||
                        other.GetComponentInParent<PlayerHealth>() != null;
        
        if (isPlayer)
        {
            hasHit = true;
            
            // Deal damage to player - try multiple methods to find PlayerHealth
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = other.GetComponentInParent<PlayerHealth>();
            }
            if (playerHealth == null)
            {
                playerHealth = other.transform.root.GetComponent<PlayerHealth>();
            }
            if (playerHealth == null && PlayerHealth.Instance != null)
            {
                playerHealth = PlayerHealth.Instance;
            }
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Bullet hit player! Player took {damage} damage. Health: {playerHealth.GetCurrentHearts()}");
            }
            else
            {
                Debug.LogWarning("Bullet hit player but couldn't find PlayerHealth component!");
            }
            
            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Destroy bullet
            DestroyBullet();
            return;
        }
        
        // Hit wall/obstacle (only if it's not a trigger)
        if (!other.isTrigger)
        {
            hasHit = true;
            
            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.LookRotation(-direction));
            }
            
            // Destroy bullet
            DestroyBullet();
        }
    }
    
    void DestroyBullet()
    {
        // Detach trail effect so it can fade out naturally
        if (trailEffect != null)
        {
            trailEffect.transform.SetParent(null);
            // Destroy trail after a delay
            ParticleSystem ps = trailEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
                Destroy(trailEffect, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(trailEffect, 1f);
            }
        }
        
        Destroy(gameObject);
    }
}

