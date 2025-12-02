using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class PlayerAbilities : MonoBehaviour
{
    [Header("References")]
    public FirstPersonController playerController;
    public Camera playerCamera;
    public CharacterController characterController;
    
    [Header("Dash Settings")]
    public float dashDistance = 15f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;
    public float dashDamageRadius = 1.5f;
    public LayerMask enemyLayer;
    public GameObject dashTrailEffect;
    public AudioClip dashSound;
    [Range(0f, 1f)] public float dashVolume = 0.7f;
    
    [Header("Updraft Settings")]
    public float updraftForce = 20f;
    public float updraftCooldown = 0.3f;
    public GameObject updraftEffect;
    public AudioClip updraftSound;
    [Range(0f, 1f)] public float updraftVolume = 0.6f;
    
    [Header("Stomp Settings")]
    public float stompSpeed = 50f;
    public float stompDamageRadius = 5f;
    public float stompCooldown = 0.5f;
    public GameObject stompImpactEffect;
    public AudioClip stompSound;
    [Range(0f, 1f)] public float stompVolume = 0.8f;

    [Header("Baloon Settings")]
    [Range(0f, 1f)] public float BaloonVolume = 0.6f;
    public AudioClip BaloonSound;

    [Header("Input Keys")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public KeyCode updraftKey = KeyCode.E;
    public KeyCode stompKey = KeyCode.Q;
    
    // State tracking
    private bool canDash = true;
    private bool canUpdraft = true;
    private bool canStomp = true;
    private bool isDashing = false;
    private bool isStomping = false;
    
    private AudioSource audioSource;
    private Vector3 dashDirection;
    private float dashTimeRemaining;
    public SpeedLinesEffect speedLinesEffect;
    void Start()
    {
        if (playerController == null)
            playerController = GetComponent<FirstPersonController>();
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    void Update()
    {
        HandleInput();
        
        if (isDashing)
        {
            ProcessDash();
        }
        
        if (isStomping)
        {
            ProcessStomp();
        }
    }
    
    void HandleInput()
    {
        // Dash - horizontal burst
        if (Input.GetKeyDown(dashKey) && canDash && !isDashing)
        {
            StartDash();
        }
        
        // Updraft - vertical boost
        if (Input.GetKeyDown(updraftKey) && canUpdraft)
        {
            PerformUpdraft();
        }
        
        // Stomp - downward slam (only in air)
        if (Input.GetKeyDown(stompKey) && canStomp && !playerController.IsGrounded())
        {
            StartStomp();
        }
    }

    // ==================== DASH ====================

   
    public void StartDash()
    {
        
        isDashing = true;
        canDash = false;
        dashTimeRemaining = dashDuration;
        if (speedLinesEffect != null)
        {
            // Debug.Log("AWODKSODJASIODJASIODFSIOPHASUIFHASUIOFHASIFBASIFS");
            speedLinesEffect.StartEffect();
        }

        // Pause player movement so it doesn't stack
        playerController.PauseMovement(true);
        
        // Get dash direction from camera (where player is looking)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            // Dash in input direction relative to camera view
            dashDirection = (playerCamera.transform.right * h + playerCamera.transform.forward * v).normalized;
        }
        else
        {
            // Dash in camera forward direction
            dashDirection = playerCamera.transform.forward;
        }
        
        // Spawn trail effect
        if (dashTrailEffect != null)
        {
            GameObject trail = Instantiate(dashTrailEffect, transform.position, transform.rotation, transform);
            Destroy(trail, dashDuration + 0.5f);
        }
        
        PlaySound(dashSound, dashVolume);
    }
    
    void ProcessDash()
    {
        playerController.SetVerticalVelocity(0f);
        dashTimeRemaining -= Time.deltaTime;
        
        if (dashTimeRemaining <= 0)
        {
            EndDash();
            return;
        }
        
        // Calculate dash movement
        float dashSpeed = dashDistance / dashDuration;
        Vector3 dashMove = dashDirection * dashSpeed * Time.deltaTime;
        //dashMove.y = 0f;
        
       
        // Move character
        characterController.Move(dashMove);
        
        // Kill enemies in path
        KillEnemiesInRadius(transform.position, dashDamageRadius);
    }
    
    public void EndDash()
    {
        //playerController.SetVerticalVelocity(0f);
        if (speedLinesEffect != null)
        {
            speedLinesEffect.StopEffect();
        }
        isDashing = false;
        
        // Resume player movement
        playerController.PauseMovement(false);
        
        StartCoroutine(DashCooldownRoutine());
        
    }
    
    IEnumerator DashCooldownRoutine()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    
    // ==================== UPDRAFT ====================
    public void PerformUpdraft()
    {
        canUpdraft = false;
        
        // Set vertical velocity directly (doesn't stack with bounce)
        playerController.SetVerticalVelocity(updraftForce);
        
        // Spawn effect
        if (updraftEffect != null)
        {
            GameObject effect = Instantiate(updraftEffect, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        PlaySound(updraftSound, updraftVolume);
        
        StartCoroutine(UpdraftCooldownRoutine());
    }
    
    IEnumerator UpdraftCooldownRoutine()
    {
        yield return new WaitForSeconds(updraftCooldown);
        canUpdraft = true;
    }
    
    // ==================== STOMP ====================
    public void StartStomp()
    {
        isStomping = true;
        canStomp = false;
        
        PlaySound(stompSound, stompVolume);
    }
    
    void ProcessStomp()
    {
        // Fast downward movement
        Vector3 stompMove = Vector3.down * stompSpeed * Time.deltaTime;
        characterController.Move(stompMove);
        
        // Check if landed
        if (playerController.IsGrounded())
        {
            PerformStompImpact();
        }
    }
    
    void PerformStompImpact()
    {
        isStomping = false;
        
        // Spawn impact effect
        if (stompImpactEffect != null)
        {
            GameObject effect = Instantiate(stompImpactEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Kill all enemies in radius
        KillEnemiesInRadius(transform.position, stompDamageRadius);
        
        // Camera shake
        CameraEffects camFX = playerCamera.GetComponent<CameraEffects>();
        if (camFX != null)
        {
            camFX.ApplyCameraShake(0.3f, 0.2f);
        }
        
        StartCoroutine(StompCooldownRoutine());
    }
    
    IEnumerator StompCooldownRoutine()
    {
        yield return new WaitForSeconds(stompCooldown);
        canStomp = true;
    }
    
    // ==================== SHARED ====================
    void KillEnemiesInRadius(Vector3 center, float radius)
    {
        Collider[] enemies = Physics.OverlapSphere(center, radius, enemyLayer);
        
        foreach (Collider col in enemies)
        {
            if (col.CompareTag("enemy"))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null && !enemy.IsDead())
                {
                    // One-shot kill with massive damage
                    enemy.TakeDamage(9999f);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Baloon"))
        {
            PlaySound(BaloonSound, BaloonVolume);

        }
    }

    void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    // ==================== PUBLIC METHODS ====================
    public void setDashing(bool flag)
    {
        isDashing = flag;

    }
    public bool IsDashing() => isDashing;
    public bool IsStomping() => isStomping;
    
    public bool CanDash() => canDash;
    public bool CanUpdraft() => canUpdraft;
    public bool CanStomp() => canStomp && !playerController.IsGrounded();
    
    // Reset abilities (useful for respawn)
    public void ResetAbilities()
    {
        canDash = true;
        canUpdraft = true;
        canStomp = true;
        isDashing = false;
        isStomping = false;
    }
}

