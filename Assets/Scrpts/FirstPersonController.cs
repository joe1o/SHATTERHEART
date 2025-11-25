using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
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
    public float groundCheckDistance = 0.3f;
    public LayerMask groundMask;
    
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
    private float lastGroundedTime;
    private float lastJumpPressTime;

    [SerializeField]private Animator anim;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
       
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (cameraTransform == null)
        {
            Debug.LogError("Camera Transform not assigned!");
        }
    }
    
    void Update()
    {
        HandleMouseLook();
        HandleGroundCheck();
        HandleJump();
        HandleMovement();
        ApplyGravity();
        HandleAnimations();
        
        // Move the character
        controller.Move(velocity * Time.deltaTime);
    }
    
    bool isMoving()
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
        // Spherecast for better ground detection
        isGrounded = Physics.CheckSphere(
            transform.position - new Vector3(0, controller.height / 2, 0), 
            controller.radius + groundCheckDistance, 
            groundMask
        );
        
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
    
    void HandleMovement()
    {
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
    
    // Detect water collision - CharacterController uses OnControllerColliderHit
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("water"))
        {
            isInWater = true;
        }
        else
        {
            isInWater = false;
        }
    }
}