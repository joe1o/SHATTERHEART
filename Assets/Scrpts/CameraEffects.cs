using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [Header("FOV")]
    public Camera cam;
    public float baseFOV = 90f;
    public float sprintFOVIncrease = 10f;
    public float fovTransitionSpeed = 10f;
    
    [Header("Camera Tilt")]
    public float tiltAngle = 2f;
    public float tiltSpeed = 5f;
    
    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public float bobFrequency = 2f;
    public float bobHorizontalAmplitude = 0.05f;
    public float bobVerticalAmplitude = 0.08f;
    
    [Header("Landing")]
    public float landingCameraKick = 0.2f;
    public float landingRecoverySpeed = 5f;
    
    [Header("References")]
    public FirstPersonController playerController;
    
    private float targetFOV;
    private float currentTilt;
    private Vector3 originalPosition;
    private float bobTimer;
    private float landingOffset;
    private bool wasGrounded;
    
    void Start()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }
        
        if (playerController == null)
        {
            playerController = GetComponentInParent<FirstPersonController>();
        }
        
        originalPosition = transform.localPosition;
        targetFOV = baseFOV;
        cam.fieldOfView = baseFOV;
        wasGrounded = true;
    }
    
    void Update()
    {
        HandleFOV();
        HandleCameraTilt();
        HandleHeadBob();
        HandleLanding();
    }
    
    void HandleFOV()
    {
        // Increase FOV based on speed for that sense of velocity
        float speed = playerController.GetHorizontalSpeed();
        float speedFactor = Mathf.Clamp01(speed / 20f); // Normalize to 0-1
        
        // Sprint FOV boost
        if (Input.GetKey(KeyCode.LeftShift))
        {
            targetFOV = baseFOV + sprintFOVIncrease;
        }
        else
        {
            targetFOV = baseFOV + (speedFactor * 5f); // Slight boost based on speed
        }
        
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
    }
    
    void HandleCameraTilt()
    {
        // Tilt camera when strafing
        float horizontal = Input.GetAxisRaw("Horizontal");
        float targetTilt = -horizontal * tiltAngle;
        
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
        
        // Apply tilt to camera Z rotation
        Vector3 currentRotation = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, currentTilt);
    }
    
    void HandleHeadBob()
    {
        if (!enableHeadBob || !playerController.IsGrounded())
        {
            // Smoothly return to original position when not bobbing
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, 
                originalPosition + Vector3.down * landingOffset, 
                Time.deltaTime * 5f
            );
            bobTimer = 0f;
            return;
        }
        
        // Only bob when moving
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            
            float bobX = Mathf.Cos(bobTimer) * bobHorizontalAmplitude;
            float bobY = Mathf.Sin(bobTimer * 2) * bobVerticalAmplitude;
            
            Vector3 targetPosition = originalPosition + new Vector3(bobX, bobY, 0) + Vector3.down * landingOffset;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 10f);
        }
        else
        {
            // Return to center when not moving
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, 
                originalPosition + Vector3.down * landingOffset, 
                Time.deltaTime * 5f
            );
        }
    }
    
    void HandleLanding()
    {
        bool isGrounded = playerController.IsGrounded();
        
        // Detect landing (wasn't grounded, now is)
        if (isGrounded && !wasGrounded)
        {
            landingOffset = landingCameraKick;
        }
        
        // Recover from landing
        landingOffset = Mathf.Lerp(landingOffset, 0f, landingRecoverySpeed * Time.deltaTime);
        
        wasGrounded = isGrounded;
    }
    
    // Call this from weapon abilities for screen shake
    public void ApplyCameraShake(float intensity, float duration)
    {
        StartCoroutine(CameraShake(intensity, duration));
    }
    
    private System.Collections.IEnumerator CameraShake(float intensity, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = originalPosition;
    }
}