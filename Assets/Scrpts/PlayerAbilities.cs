using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class PlayerAbilities : MonoBehaviour
{
    [Header("References")]
    public FirstPersonController playerController;
    public Camera playerCamera;
    public CharacterController characterController;

    [Header("Fireball Settings")]
    public float fireballSpeed = 30f;
    public float fireballDuration = 1.5f;
    public float fireballMinSpeed = 5f;
    public float fireballGroundDrag = 0.92f;
    public float fireballAirDrag = 0.98f;
    public float fireballSlopeFriction = 0.95f;
    public bool fireballAllowAirControl = true;
    public float fireballAirControlStrength = 2f;
    public bool fireballAllowGroundControl = true;
    public float fireballGroundControlStrength = 5f;
    public bool fireballCanCancelEarly = true;
    public ParticleSystem fireballParticles;
    public TrailRenderer fireballTrail;
    public AudioClip fireballLaunchSound;
    [Range(0f, 1f)] public float fireballVolume = 0.8f;
    private bool isFireballing = false;
    private Vector3 fireballVelocity;
    private float fireballTimer = 0f;


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
    public KeyCode fireballKey = KeyCode.Z;

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
        if (isFireballing)
        {
            ProcessFireball();
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

        if (Input.GetKeyDown(fireballKey) && CanFireball())
        {
            ActivateFireball();
        }
    }


    public void ActivateFireball()
    {
        if (isFireballing) return;

        // Cancel other abilities
        if (isDashing) EndDash();
        if (isStomping) isStomping = false;

        isFireballing = true;
        fireballTimer = fireballDuration;
        // Reset vertical velocity to prevent falling momentum from affecting launch
        playerController.SetVerticalVelocity(0f);

        // Pause normal player movement
        playerController.PauseMovement(true);

        // Get launch direction from camera
        Vector3 launchDir = playerCamera.transform.forward;

        // Keep it mostly horizontal (optional - remove this for full 3D direction)
        //launchDir.y = Mathf.Max(launchDir.y, -0.2f); // Allow slight downward angle
        launchDir.Normalize();

        // Set fireball velocity
        fireballVelocity = launchDir * fireballSpeed;

        // Visual effects
        if (fireballParticles != null)
            fireballParticles.Play();

        if (fireballTrail != null)
            fireballTrail.emitting = true;

        // Speed lines effect
        if (speedLinesEffect != null)
            speedLinesEffect.StartEffect();

        // Audio
        PlaySound(fireballLaunchSound, fireballVolume);

        Debug.Log("Fireball activated!");
    }

    void ProcessFireball()
    {
        fireballTimer -= Time.deltaTime;

        // Check if should end fireball
        if (fireballTimer <= 0f || fireballVelocity.magnitude < fireballMinSpeed)
        {
            EndFireball();
            return;
        }

        // Optional: Cancel early with jump
        if (fireballCanCancelEarly && Input.GetButtonDown("Jump"))
        {
            EndFireball();
            return;
        }

        bool isGrounded = playerController.IsGrounded();

        // Get input for control
        float h = Input.GetAxis("Horizontal");
       // float v = Input.GetAxis("Vertical");

        // Apply control based on grounded state
        if (Mathf.Abs(h) > 0.1f) // || Mathf.Abs(v) > 0.1f)
        {
            Vector3 inputDir = (playerCamera.transform.right * h + playerCamera.transform.forward * 0).normalized;

            // Current dash direction
            Vector3 currentDir = fireballVelocity.normalized;

            // Select turn rate
            float turnRate = isGrounded ? fireballGroundControlStrength : fireballAirControlStrength;

            // Rotate current direction toward input direction
            Vector3 newDir = Vector3.RotateTowards(
                currentDir,
                inputDir,
                turnRate * Time.deltaTime,
                0f
            );

            // Reapply original speed (preserves magnitude!)
            float speed = fireballVelocity.magnitude;
            fireballVelocity = newDir * speed;

        }

        // Apply gravity when in air
        if (!isGrounded)
        {
            fireballVelocity += Physics.gravity * Time.deltaTime;
        }

        // Move character FIRST (before applying drag)
        characterController.Move(fireballVelocity * Time.deltaTime);

        // THEN apply drag for next frame
        float drag = isGrounded ? fireballGroundDrag : fireballAirDrag;
        fireballVelocity *= drag;

        // Optional: Kill enemies in path
        KillEnemiesInRadius(transform.position, dashDamageRadius);
    }

    public void EndFireball()
    {
        if (!isFireballing) return;

        isFireballing = false;

        Vector3 horizontal = fireballVelocity;
        //horizontal.y = 0f;
        playerController.AddVelocity(horizontal);

        // Vertical momentum (critical — fixes your instant drop)
        playerController.SetVerticalVelocity(fireballVelocity.y);

        // Resume normal movement
        playerController.PauseMovement(false);

        // Visual effects
        if (fireballParticles != null)
            fireballParticles.Stop();

        if (fireballTrail != null)
            fireballTrail.emitting = false;

        // Speed lines
        if (speedLinesEffect != null)
            speedLinesEffect.StopEffect();

        Debug.Log("Fireball ended");
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
        dashMove.y = 0f;
        
       
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

    public void PlaySound(AudioClip clip, float volume)
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

    public bool IsFireballing() => isFireballing;

    public bool CanDash() => canDash;
    public bool CanUpdraft() => canUpdraft;
    public bool CanStomp() => canStomp && !playerController.IsGrounded();
    public bool CanFireball() => !isFireballing && !isDashing && !isStomping;

    // Reset abilities (useful for respawn)
    public void ResetAbilities()
    {
        canDash = true;
        canUpdraft = true;
        canStomp = true;
        isDashing = false;
        isStomping = false;
        isFireballing = false;
    }
}

