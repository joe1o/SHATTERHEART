using UnityEngine;
using System.Collections;

public class EnemyShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float fireRate = 2f;           // Shots per second
    public int damage = 1;                // Damage per shot (1 heart = 1 damage)
    public float range = 50f;
    public float spreadAngle = 2f;        // Accuracy spread
    
    [Header("Detection Settings")]
    public float detectionRange = 40f;
    public float fieldOfView = 60f;       // Cone of vision
    
    [Header("Bullet Settings")]
    public GameObject bulletPrefab; // Bullet GameObject with EnemyBullet component
    public GameObject bulletParticleEffect; // Particle effect to attach to bullet
    public float bulletSpeed = 30f;
    
    [Header("Effects")]
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
    public AudioClip shootSound;
    [Range(0f, 1f)] public float shootVolume = 0.6f;
    
    [Header("Fire Point")]
    public Transform firePoint;           // Where bullets come from (assign in inspector)
    
    [Header("Aim Settings")]
    public float playerAimHeight = 0.5f;  // How high to aim on player (0 = feet, 1 = head, 0.5 = chest)
    
    [Header("Rotation")]
    public bool facePlayer = true;
    public float rotationSpeed = 2f;
    
    private Transform playerTarget;
    private float nextFireTime = 0f;
    private AudioSource audioSource;
    private Enemy enemyScript;
    private LayerMask playerLayer;
    private LayerMask obstacleLayer;
    
    void Start()
    {
        // Find player - try multiple methods
        FindPlayer();
        
        if (playerTarget == null)
        {
            Debug.LogWarning($"EnemyShooting on {gameObject.name} could not find Player! Make sure Player has 'Player' tag.");
        }
        
        enemyScript = GetComponent<Enemy>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Set up fire point if not assigned
        if (firePoint == null)
        {
            // Try to find a child named "FirePoint" or use transform
            Transform fp = transform.Find("FirePoint");
            if (fp != null)
            {
                firePoint = fp;
            }
            else
            {
                firePoint = transform; // Default to enemy position
            }
        }
        
        // Set up layers - adjust based on your setup
        obstacleLayer = LayerMask.GetMask("Default"); // Adjust to your obstacle layers
    }
    
    void FindPlayer()
    {
        // Try finding by tag first
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
            return;
        }
        
        // Try finding by name
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            return;
        }
        
        // Try finding PlayerHealth singleton
        if (PlayerHealth.Instance != null)
        {
            playerTarget = PlayerHealth.Instance.transform;
        }
    }
    
    void Update()
    {
        // Don't shoot if dead
        if (enemyScript != null && enemyScript.IsDead()) return;
        if (playerTarget == null) 
        {
            // Try to find player again
            FindPlayer();
            return;
        }
        
        // Check if player is in range and visible
        if (CanSeePlayer())
        {
            // Face player (optional)
            if (facePlayer)
            {
                FacePlayer();
            }
            
            // Shoot
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + (1f / fireRate);
            }
        }
    }
    
    bool CanSeePlayer()
    {
        if (playerTarget == null) return false;
        if (PlayerHealth.Instance != null && PlayerHealth.Instance.IsDead()) return false;
        
        Vector3 directionToPlayer = (playerTarget.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;
        
        // Check if in range
        if (distanceToPlayer > detectionRange) return false;
        
        // Check if in field of view (more lenient check - just angle, not strict)
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer.normalized);
        if (angleToPlayer > fieldOfView / 2f) return false;
        
        // Check line of sight (raycast) - but don't require perfect hit
        RaycastHit hit;
        Vector3 rayStart = firePoint != null ? firePoint.position : transform.position;
        rayStart.y += 1f; // Eye level
        
        Vector3 playerPosition = playerTarget.position;
        playerPosition.y += playerAimHeight; // Aim at specified height on player
        
        Vector3 direction = (playerPosition - rayStart).normalized;
        
        // Use a longer raycast to be more lenient
        if (Physics.Raycast(rayStart, direction, out hit, distanceToPlayer + 5f))
        {
            // Check if we hit the player or player's collider
            if (hit.collider.CompareTag("Player") || hit.collider.transform.root.CompareTag("Player"))
            {
                return true;
            }
            // Hit something else - can't see player directly
            return false;
        }
        
        // If raycast didn't hit anything, assume we can see player
        return true;
    }
    
    void FacePlayer()
    {
        if (playerTarget == null) return;
        
        // Rotate to face player (only horizontally)
        Vector3 directionToPlayer = (playerTarget.position - transform.position);
        directionToPlayer.y = 0; // Only rotate horizontally
        
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
    
    void Shoot()
    {
        if (playerTarget == null) return;
        
        // Calculate direction with spread
        Vector3 rayStart = firePoint != null ? firePoint.position : transform.position;
        rayStart.y += 1f; // Enemy eye level
        
        Vector3 playerPosition = playerTarget.position;
        playerPosition.y += 0.5f; // Aim at player's body/chest (lower than head/camera)
        
        Vector3 directionToPlayer = (playerPosition - rayStart);
        Vector3 spreadDirection = ApplySpread(directionToPlayer.normalized, spreadAngle);
        
        // Visual/audio effects
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, rayStart, Quaternion.LookRotation(spreadDirection));
            Destroy(flash, 0.1f);
        }
        
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound, shootVolume);
        }
        
        // Spawn visible bullet projectile
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, rayStart, Quaternion.LookRotation(spreadDirection));
            
            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript == null)
            {
                bulletScript = bullet.AddComponent<EnemyBullet>();
            }
            
            // Configure bullet
            bulletScript.SetDirection(spreadDirection);
            bulletScript.damage = damage;
            bulletScript.speed = bulletSpeed;
            bulletScript.hitEffectPrefab = hitEffectPrefab;
            bulletScript.SetShooter(transform); // Ignore collisions with the shooter
            
            // Attach particle effect to bullet if provided
            if (bulletParticleEffect != null)
            {
                GameObject particleObj = Instantiate(bulletParticleEffect, bullet.transform);
                bulletScript.trailEffect = particleObj;
            }
        }
        else
        {
            // Fallback: hitscan if no bullet prefab
            RaycastHit hit;
            if (Physics.Raycast(rayStart, spreadDirection, out hit, range))
            {
                if (hitEffectPrefab != null)
                {
                    GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(effect, 2f);
                }
                
                if (hit.collider.CompareTag("Player") || hit.collider.transform.root.CompareTag("Player"))
                {
                    PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                    if (playerHealth == null)
                    {
                        playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();
                    }
                    if (playerHealth == null && PlayerHealth.Instance != null)
                    {
                        playerHealth = PlayerHealth.Instance;
                    }
                    
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage);
                    }
                }
            }
        }
    }
    
    Vector3 ApplySpread(Vector3 direction, float spreadAngle)
    {
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);
        
        Quaternion spreadRotation = Quaternion.Euler(spreadY, spreadX, 0f);
        return spreadRotation * direction;
    }
    
    // Visualize detection range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw field of view cone
        Gizmos.color = Color.red;
        Vector3 forward = transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * forward;
        
        Gizmos.DrawRay(transform.position, rightBoundary);
        Gizmos.DrawRay(transform.position, leftBoundary);
    }
}

