using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteReaction : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public RectTransform characterSprite;

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

    private Vector3 lastPlayerPosition;
    private Vector2 targetOffset;
    private float targetTilt;
    private Vector2 basePosition;
    private float breathingTimer;

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
    }

    void Update()
    {
        if (playerTransform == null || characterSprite == null) return;

        // Calculate player velocity
        Vector3 playerVelocity = (playerTransform.position - lastPlayerPosition) / Time.deltaTime;
        lastPlayerPosition = playerTransform.position;

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
