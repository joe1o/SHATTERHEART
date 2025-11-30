using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpeedLinesEffect : MonoBehaviour
{
    public FirstPersonController fpc;
    [Header("References")]
    public Canvas canvas;
    public Transform player;

    [Header("Line Settings")]
    public Color lineColor = new Color(1f, 1f, 1f, 0.3f);
    public float lineWidth = 3f;
    public int maxLines = 15;

    [Header("Spawn Settings")]
    public float spawnRate = 0.05f; // Time between spawns
    public float spawnRadius = 600f; // Distance from center to spawn

    [Header("Animation")]
    public float lineSpeed = 1200f;
    public float lineFadeSpeed = 2f;
    public float lineLifetime = 1f;

    [Header("Trigger Conditions")]
    public bool triggerOnDash = true;
    public bool triggerOnRun = true;
    public float runSpeedThreshold = 5f; // Minimum speed to trigger when running

    private List<SpeedLine> activeLines = new List<SpeedLine>();
    private float spawnTimer = 0f;
    private bool isActive = false;
    private RectTransform canvasRect;
    private bool isDashing = false;

    private class SpeedLine
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public Image image;
        public float lifetime;
        public Vector2 direction;
        public float speed;
    }

    void Start()
    {
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();

        canvasRect = canvas.GetComponent<RectTransform>();
        

        // Debug: Force start effect to test
       // Debug.Log("SpeedLinesEffect Started");
        //StartEffect();
    }

    void Update()
    {
        //if (!isActive)
        //{
        //    StopEffect();
        //    return;
        //}

        if (isActive)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnLine();
                spawnTimer = spawnRate;
            }
        }

        // Spawn new lines
        //spawnTimer -= Time.deltaTime;
        //if (spawnTimer <= 0f)
        //{
        //    SpawnLine();
        //    spawnTimer = spawnRate;
        //}

        // Update existing lines
        for (int i = activeLines.Count - 1; i >= 0; i--)
        {
            SpeedLine line = activeLines[i];

            if (line.rectTransform == null)
            {
                activeLines.RemoveAt(i);
                continue;
            }

            line.lifetime -= Time.deltaTime;

            // Move line toward center
            line.rectTransform.anchoredPosition += line.direction * line.speed * Time.deltaTime;

            // Fade out
            if (line.image != null)
            {
                Color col = line.image.color;
                col.a = Mathf.Lerp(col.a, 0f, Time.deltaTime * lineFadeSpeed);
                line.image.color = col;
            }

            // Remove if dead
            if (line.lifetime <= 0f || line.image.color.a < 0.01f)
            {
                Destroy(line.gameObject);
                activeLines.RemoveAt(i);
            }
        }
    }

    void SpawnLine()
    {
        if (activeLines.Count >= maxLines) return;

        Debug.Log("Spawning speed line");

        // Create line GameObject
        GameObject lineObj = new GameObject("SpeedLine");
        lineObj.transform.SetParent(canvas.transform, false);

        RectTransform rt = lineObj.AddComponent<RectTransform>();
        Image img = lineObj.AddComponent<Image>();

        // Use white sprite (Unity's default)
        img.sprite = null;
        img.color = lineColor;

        // Random angle around screen edge
        float angle = Random.Range(0f, 360f);
        float rad = angle * Mathf.Deg2Rad;

        // Spawn position at edge of screen
        Vector2 spawnPos = new Vector2(
            Mathf.Cos(rad) * spawnRadius,
            Mathf.Sin(rad) * spawnRadius
        );

        rt.anchoredPosition = spawnPos;

        // Direction toward center
        Vector2 direction = -spawnPos.normalized;

        // Rotate line to point toward center
        float rotationAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0, 0, rotationAngle);

        // Line appearance
        rt.sizeDelta = new Vector2(Random.Range(60f, 150f), lineWidth); // Random length
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;

        // Add slight random variation to speed
        float speedVariation = Random.Range(0.8f, 1.2f);
        

        // Create speed line data
        SpeedLine speedLine = new SpeedLine
        {
            gameObject = lineObj,
            rectTransform = rt,
            image = img,
            lifetime = lineLifetime,
            direction = direction,
            speed = lineSpeed * speedVariation
        };

        activeLines.Add(speedLine);

        Debug.Log($"Line spawned at {spawnPos}, moving toward center. Total lines: {activeLines.Count}");
    }

    public void StartEffect()
    {
        isActive = true;
        isDashing = true;
        spawnTimer = 0f;
    }

    public void StopEffect()
    {
        isActive = false;
        isDashing = false;

        //// Immediately destroy all existing lines
        //for (int i = activeLines.Count - 1; i >= 0; i--)
        //{
        //    if (activeLines[i].gameObject != null)
        //    {
        //        Destroy(activeLines[i].gameObject);
        //    }
        //}
        //activeLines.Clear();
    }

    public void CheckPlayerSpeed()
    {
        if (!triggerOnRun || player == null) return;

        if(isDashing)
        {
            return;
        }

        float currentSpeed = fpc.moveSpeed;
        if (fpc.IsInWater())
        {
            currentSpeed *= fpc.waterSpeedMultiplier;
        }
        else
        {
            currentSpeed = fpc.moveSpeed;
        }
        if(currentSpeed > runSpeedThreshold && !isActive && fpc.isMoving())
        {
            isActive = true;
            spawnTimer = 0f;
        }
        else if (currentSpeed < runSpeedThreshold && isActive || !fpc.isMoving())
        {
            isActive = false;
            for (int i = activeLines.Count - 1; i >= 0; i--)
            {
                if (activeLines[i].gameObject != null)
                {
                    Destroy(activeLines[i].gameObject);
                }
            }
            activeLines.Clear();
        }

    }

    void LateUpdate()
    {
        // Auto-check player speed if enabled
        if (triggerOnRun)
        {
            CheckPlayerSpeed();
        }
    }

    public void ClearAllLines()
    {
        foreach (var line in activeLines)
        {
            if (line.gameObject != null)
                Destroy(line.gameObject);
        }
        activeLines.Clear();
    }

    void OnDestroy()
    {
        ClearAllLines();
    }
}