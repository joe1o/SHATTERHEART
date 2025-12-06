using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteReaction : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public RectTransform characterSprite;
    public FirstPersonController playerController; // Add reference to check grounding


    [Header("Movement Reaction")]
    public float maxHorizontalOffset = 15f;
    public float maxVerticalOffset = 8f;
    public float movementSmoothSpeed = 5f;

    [Header("Tilt Reaction")]
    public float maxTiltAngle = 5f;
    public float tiltSmoothSpeed = 4f;

    [Header("Idle Breathing Animation")]
    public bool enableBreathing = true;
    public float breathingSpeed = 1.5f;
    public float breathingAmount = 3f;

    [Header("Landing Shake")]
    public bool enableLandingShake = true;
    public float landingShakeAmount = 20f;
    public float landingShakeSpeed = 15f;
    public float minFallSpeedForShake = 5f; // Minimum fall speed to trigger shake

    private Vector3 lastPlayerPosition;
    private Vector2 targetOffset;
    private float targetTilt;
    private Vector2 basePosition;
    private float breathingTimer;

    private bool wasGrounded = true;
    private float fallSpeed = 0f;
    private bool isShaking = false;
    private float shakeTimer = 0f;
    private float shakeOffset = 0f;

    void Start()
    {
        if (characterSprite != null)
        {
            basePosition = characterSprite.anchoredPosition;
        }

        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
        }

        if (playerController != null)
        {
            wasGrounded = playerController.IsGrounded();
        }
    }

    void Update()
    {
        if (playerTransform == null || characterSprite == null) return;

        // Calculate player velocity
        Vector3 playerVelocity = (playerTransform.position - lastPlayerPosition) / Time.deltaTime;
        lastPlayerPosition = playerTransform.position;

        if (playerController != null)
        {
            bool isGrounded = playerController.IsGrounded();

            // Track fall speed
            if (!isGrounded)
            {
                fallSpeed = Mathf.Abs(playerVelocity.y);
            }

            // Trigger landing shake when landing from a fall
            if (!wasGrounded && isGrounded && fallSpeed > minFallSpeedForShake)
            {
                TriggerLandingShake();
            }

            wasGrounded = isGrounded;
        }
        if (isShaking)
        {
            ProcessLandingShake();
        }
        else
        {
            // Normal movement reaction (only when not shaking)
            ProcessNormalMovement(playerVelocity);
        }

        // Determine target offset based on movement
        // float horizontalInput = playerVelocity.x;
        // float verticalInput = playerVelocity.y;

        // Map velocity to sprite offset(inverse direction for reactive feel)
        //     targetOffset.x = Mathf.Clamp(-horizontalInput * 2f, -maxHorizontalOffset, maxHorizontalOffset);
        // targetOffset.y = Mathf.Clamp(verticalInput * 1.5f, -maxVerticalOffset, maxVerticalOffset);

        // Calculate tilt based on horizontal movement
        // targetTilt = Mathf.Clamp(-horizontalInput * 3f, -maxTiltAngle, maxTiltAngle);

        // Breathing animation when idle
        // Vector2 breathingOffset = Vector2.zero;
        // if (enableBreathing && Mathf.Abs(playerVelocity.x) < 0.1f && Mathf.Abs(playerVelocity.y) < 0.1f)
        // {
        //     breathingTimer += Time.deltaTime * breathingSpeed;
        //     breathingOffset.y = Mathf.Sin(breathingTimer) * breathingAmount;
        // }
        // else
        // {
        //     breathingTimer = 0f;
        // }

        // Smoothly lerp to target values
        //Vector2 currentOffset = characterSprite.anchoredPosition - basePosition;
        // currentOffset = Vector2.Lerp(currentOffset, targetOffset, Time.deltaTime * movementSmoothSpeed);

        // float currentTilt = characterSprite.localRotation.eulerAngles.z;
        // if (currentTilt > 180f) currentTilt -= 360f;
        // currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmoothSpeed);

        // Apply final position and rotation
        // characterSprite.anchoredPosition = basePosition + currentOffset + breathingOffset;
        // characterSprite.localRotation = Quaternion.Euler(0, 0, currentTilt);
    }

    void ProcessNormalMovement(Vector3 playerVelocity)
    {
        // Determine target offset based on movement
        float horizontalInput = playerVelocity.x;
        float verticalInput = playerVelocity.y;

        // Map velocity to sprite offset (inverse direction for reactive feel)
        targetOffset.x = Mathf.Clamp(-horizontalInput * 2f, -maxHorizontalOffset, maxHorizontalOffset);
        targetOffset.y = Mathf.Clamp(verticalInput * 1.5f, -maxVerticalOffset, maxVerticalOffset);

        // Calculate tilt based on horizontal movement
        targetTilt = Mathf.Clamp(-horizontalInput * 3f, -maxTiltAngle, maxTiltAngle);

        // Breathing animation when idle
        Vector2 breathingOffset = Vector2.zero;
        if (enableBreathing && Mathf.Abs(playerVelocity.x) < 0.1f && Mathf.Abs(playerVelocity.y) < 0.1f)
        {
            breathingTimer += Time.deltaTime * breathingSpeed;
            breathingOffset.y = Mathf.Sin(breathingTimer) * breathingAmount;
        }
        else
        {
            breathingTimer = 0f;
        }

        // Smoothly lerp to target values
        Vector2 currentOffset = characterSprite.anchoredPosition - basePosition;
        currentOffset = Vector2.Lerp(currentOffset, targetOffset, Time.deltaTime * movementSmoothSpeed);

        float currentTilt = characterSprite.localRotation.eulerAngles.z;
        if (currentTilt > 180f) currentTilt -= 360f;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmoothSpeed);

        // Apply final position and rotation
        characterSprite.anchoredPosition = basePosition + currentOffset + breathingOffset;
        characterSprite.localRotation = Quaternion.Euler(0, 0, currentTilt);
    }

    void TriggerLandingShake()
    {
        if (!enableLandingShake) return;

        isShaking = true;
        shakeTimer = 0f;
        shakeOffset = 0f;
    }

    void ProcessLandingShake()
    {
        shakeTimer += Time.deltaTime * landingShakeSpeed;

        // Shake pattern: quick down, then bounce back up
        // Using sine wave that goes negative first then positive
        float shakeProgress = shakeTimer;

        if (shakeProgress < Mathf.PI) // One full shake cycle
        {
            // Sine wave creates smooth down-up motion
            shakeOffset = -Mathf.Sin(shakeProgress) * landingShakeAmount;

            // Apply shake
            characterSprite.anchoredPosition = basePosition + new Vector2(0f, shakeOffset);
            characterSprite.localRotation = Quaternion.identity;
        }
        else
        {
            // Shake complete
            isShaking = false;
            shakeOffset = 0f;
            characterSprite.anchoredPosition = basePosition;
            characterSprite.localRotation = Quaternion.identity;
        }
    }

    // Optional: Reset sprite to base position
    public void ResetSprite()
    {
        if (characterSprite != null)
        {
            characterSprite.anchoredPosition = basePosition;
            characterSprite.localRotation = Quaternion.identity;
        }
    }
}
