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
    
    [Header("Effects")]
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
    public AudioClip shootSound;
    [Range(0f, 1f)] public float shootVolume = 0.6f;
    
    [Header("Fire Point")]
    public Transform firePoint;           // Where bullets come from (assign in inspector)
    
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
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }
        else
        {
            // Try finding by name
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
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
    
    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
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
        
        // Check if in field of view
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer.normalized);
        if (angleToPlayer > fieldOfView / 2f) return false;
        
        // Check line of sight (raycast)
        RaycastHit hit;
        Vector3 rayStart = firePoint != null ? firePoint.position : transform.position;
        // Adjust ray start height to be at eye level (adjust Y offset as needed)
        rayStart.y += 1f; // Assuming enemy is roughly human-sized
        
        Vector3 playerPosition = playerTarget.position;
        playerPosition.y += 1f; // Aim at player's center/head
        
        Vector3 direction = (playerPosition - rayStart).normalized;
        
        if (Physics.Raycast(rayStart, direction, out hit, distanceToPlayer))
        {
            // Check if we hit the player
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
            // Hit something else (wall/obstacle), can't see player
            return false;
        }
        
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
        Vector3 directionToPlayer = (playerTarget.position - firePoint.position);
        directionToPlayer.y += 1f; // Aim slightly up (at player center)
        
        Vector3 spreadDirection = ApplySpread(directionToPlayer.normalized, spreadAngle);
        
        // Visual/audio effects
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.1f);
        }
        
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound, shootVolume);
        }
        
        // Raycast to hit player
        RaycastHit hit;
        Vector3 rayStart = firePoint != null ? firePoint.position : transform.position;
        rayStart.y += 1f; // Eye level
        
        if (Physics.Raycast(rayStart, spreadDirection, out hit, range))
        {
            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(effect, 2f);
            }
            
            // Deal damage to player
            if (hit.collider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth == null)
                {
                    // Try to find PlayerHealth on parent or anywhere on player
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
        
        // Debug visualization (remove in production)
        Debug.DrawRay(rayStart, spreadDirection * range, Color.red, 0.2f);
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

