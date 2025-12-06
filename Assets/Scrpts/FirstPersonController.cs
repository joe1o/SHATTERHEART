using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    private PlayerAbilities abilities;
    [Header("Movement")]
    public float moveSpeed = 12f;
    public float waterSpeedMultiplier = 1.5f;
    public float airControl = 0.8f;
    public float groundDrag = 8f;
    public float airDrag = 2f;
    
    private bool isInWater = false;
    
    [Header("Jumping")]
    public float jumpForce = 15f;
    public float gravity = 25f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.2f;
    
    [Header("Ground Check")]
    public float groundCheckDistance = 0.1f;  // Reduced for snappier landing
    public LayerMask groundMask;
    
    [Header("Sounds")]
    public AudioClip[] footstepSounds;
    public AudioClip[] waterFootstepSounds;
    public AudioClip landSound;
    public AudioClip waterLandSound;
    public float footstepInterval = 0.35f;
    public float waterFootstepInterval = 0.25f;
    
    [Header("Sound Volumes")]
    [Range(0f, 1f)] public float footstepVolume = 0.5f;
    [Range(0f, 1f)] public float waterFootstepVolume = 0.6f;
    [Range(0f, 1f)] public float landVolume = 0.5f;
    [Range(0f, 1f)] public float waterLandVolume = 0.7f;
    
    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float minPitch = -89f;
    public float maxPitch = 89f;
    
    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 moveDirection;
    private float xRotation = 0f;
    
    private bool isGrounded;
    private bool wasGrounded;
    private float lastGroundedTime;
    private float lastJumpPressTime;
    private float nextFootstepTime;
    private AudioSource audioSource;
    private bool movementPaused = false;

    [SerializeField]private Animator anim;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (cameraTransform == null)
        {
            Debug.LogError("Camera Transform not assigned!");
        }
        
        wasGrounded = true;
    }

    //public void SetVerticalVelocity(float v)
    //{
    //    velocity.y = v;
    //}
    
    void Update()
    {
        HandleMouseLook();
        HandleGroundCheck();
        CheckWaterStatus();
        HandleJump();
        HandleMovement();
        ApplyGravity();
        HandleAnimations();
        HandleFootsteps();
        HandleLanding();
        
        // Move the character
        controller.Move(velocity * Time.deltaTime);
        
        wasGrounded = isGrounded;

        //reset level
        if (Input.GetKeyDown(KeyCode.F))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void AddVelocity(Vector3 addedVelocity)
    {
        // Horizontal only — leave vertical movement to your gravity/jump logic
        addedVelocity.y = 0f;

        // Combine with current velocity
        velocity += addedVelocity;

        // Optional clamp to avoid insane speeds
        float maxSpeed = 30f;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);

        if (horizontal.magnitude > maxSpeed)
        {
            horizontal = horizontal.normalized * maxSpeed;
            velocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
        }
    }

    public bool isMoving()
    {
        return controller.velocity.magnitude > 0.1f;

    }

    void HandleAnimations()
    {
        if(isMoving())
        {
            anim.SetBool("running", true);
        }
        else
        {
            anim.SetBool("running", false);
        }
    }
    public void Bounce(float bounceStrength)
    {
        velocity.y = bounceStrength;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Rotate camera vertically
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);
    }
    
    void HandleGroundCheck()
    {
        // Tighter ground check for snappier landing
        isGrounded = Physics.CheckSphere(
            transform.position - new Vector3(0, controller.height / 2, 0), 
            groundCheckDistance, 
            groundMask
        );
        
        // Also use CharacterController's built-in ground check
        isGrounded = isGrounded || controller.isGrounded;
        
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }
    
    void HandleJump()
    {
        // Jump buffer - remember jump press
        if (Input.GetButtonDown("Jump"))
        {
            lastJumpPressTime = Time.time;
        }
        
        // Coyote time + jump buffer
        bool canJump = (Time.time - lastGroundedTime < coyoteTime) && 
                       (Time.time - lastJumpPressTime < jumpBufferTime);
        
        if (canJump && velocity.y <= 0)
        {
            velocity.y = jumpForce;
            lastJumpPressTime = -1f; // Reset buffer
        }
    }
    
    public float getSpeed()
    {
        return moveSpeed;
    }
    void HandleMovement()
    {
        // Skip movement when paused (during dash, etc.)
        if (movementPaused) return;
        
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // Calculate move direction relative to player rotation
        Vector3 inputDir = transform.right * horizontal + transform.forward * vertical;
        inputDir = inputDir.normalized;
        
        // Apply water speed multiplier only when grounded and in water
        float currentSpeed = moveSpeed;
        if (isInWater && isGrounded)
        {
            currentSpeed *= waterSpeedMultiplier; 
        }
        
        if (isGrounded)
        {
            // Ground movement - snappy and responsive
            moveDirection = Vector3.Lerp(moveDirection, inputDir * currentSpeed, groundDrag * Time.deltaTime);
        }
        else
        {
            // Air movement - high air control for Neon White feel
            moveDirection = Vector3.Lerp(moveDirection, inputDir * currentSpeed, airControl * Time.deltaTime);
        }
        
        // Preserve vertical velocity, apply horizontal movement
        velocity.x = moveDirection.x;
        velocity.z = moveDirection.z;
        
        // Bunny hop - preserve horizontal speed when jumping
        if (isGrounded && Input.GetButton("Jump"))
        {
            // Maintain momentum through jumps
            float currentHorizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            if (currentHorizontalSpeed > moveSpeed)
            {
                // Keep the extra speed from previous movements
                Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z).normalized * currentHorizontalSpeed;
                velocity.x = horizontalVel.x;
                velocity.z = horizontalVel.z;
            }
        }
    }
    
    void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to keep grounded
        }
        else
        {
            
            velocity.y -= gravity * Time.deltaTime;
            
        }
    }
    
    // Public method for abilities to add forces (dashes, etc)
    public void AddForce(Vector3 force)
    {
        velocity += force;
    }
    
    // Set vertical velocity directly (doesn't stack)
    public void SetVerticalVelocity(float yVelocity)
    {
        velocity.y = yVelocity;
    }
    
    // Pause/unpause movement (for abilities like dash)
    public void PauseMovement(bool pause)
    {
        movementPaused = pause;
        if (pause)
        {
            // Reset horizontal velocity when pausing
            velocity.x = 0;
            velocity.z = 0;
            moveDirection = Vector3.zero;
        }
    }
    
    // Public method to check if player is grounded (for abilities)
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    // Get current horizontal velocity (for speed tracking)
    public float GetHorizontalSpeed()
    {
        return new Vector3(velocity.x, 0, velocity.z).magnitude;
    }
    
    void CheckWaterStatus()
    {
        // Reset water status when in air
        if (!isGrounded)
        {
            isInWater = false;
            return;
        }
        
        // Raycast down to check what surface we're standing on
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, controller.height))
        {
            isInWater = hit.collider.CompareTag("water");
        }
        else
        {
            isInWater = false;
        }
    }
    
    // Check if player is in water
    public bool IsInWater()
    {
        return isInWater;
    }
    
    void HandleFootsteps()
    {
        // Only play footsteps when grounded and moving
        if (!isGrounded) return;
        
        float speed = GetHorizontalSpeed();
        if (speed < 0.5f) return;
        
        float interval = isInWater ? waterFootstepInterval : footstepInterval;
        
        if (Time.time >= nextFootstepTime)
        {
            nextFootstepTime = Time.time + interval;
            PlayFootstep();
        }
    }
    
    void PlayFootstep()
    {
        AudioClip[] clips = isInWater ? waterFootstepSounds : footstepSounds;
        float volume = isInWater ? waterFootstepVolume : footstepVolume;
        
        if (clips == null || clips.Length == 0) return;
        
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
    
    void HandleLanding()
    {
        // Play land sound when hitting ground
        if (isGrounded && !wasGrounded)
        {
            AudioClip clipToPlay = isInWater ? waterLandSound : landSound;
            float volume = isInWater ? waterLandVolume : landVolume;
            
            if (clipToPlay != null && audioSource != null)
            {
                audioSource.PlayOneShot(clipToPlay, volume);
            }
        }
    }
    
    public void SetVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }
}