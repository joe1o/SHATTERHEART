using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Heart Settings")]
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;
    public GameObject heartPrefab; // Optional: if you want to spawn hearts dynamically
    public Transform heartContainer; // Parent for heart images
    
    [Header("Layout Settings")]
    public float heartSpacing = 50f;
    public Vector2 heartSize = new Vector2(40f, 40f);
    public Vector2 startPosition = new Vector2(50f, -50f); // Top-left position
    
    private List<Image> heartImages = new List<Image>();
    
    void Start()
    {
        // Initialize hearts if container exists
        if (heartContainer != null)
        {
            InitializeHearts();
        }
    }
    
    void InitializeHearts()
    {
        // Clear existing hearts
        foreach (Image heart in heartImages)
        {
            if (heart != null) Destroy(heart.gameObject);
        }
        heartImages.Clear();
        
        // Create heart images
        for (int i = 0; i < 3; i++)
        {
            GameObject heartObj = new GameObject($"Heart_{i}");
            heartObj.transform.SetParent(heartContainer);
            
            Image heartImage = heartObj.AddComponent<Image>();
            heartImage.sprite = fullHeartSprite;
            heartImage.preserveAspect = true;
            
            RectTransform rectTransform = heartObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1); // Top-left anchor
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = heartSize;
            rectTransform.anchoredPosition = new Vector2(startPosition.x + (i * heartSpacing), startPosition.y);
            
            heartImages.Add(heartImage);
        }
    }
    
    // Alternative: If you manually assign heart images in inspector
    [Header("Manual Heart Images (Alternative)")]
    public Image[] heartImagesManual; // Drag 3 heart images here
    
    // Method to update manually assigned hearts
    public void UpdateManualHearts(int currentHearts)
    {
        if (heartImagesManual == null) return;
        
        for (int i = 0; i < heartImagesManual.Length; i++)
        {
            if (heartImagesManual[i] == null) continue;
            
            if (i < currentHearts)
            {
                // Full heart - show and make visible
                if (fullHeartSprite != null)
                {
                    heartImagesManual[i].sprite = fullHeartSprite;
                }
                heartImagesManual[i].color = Color.white;
                heartImagesManual[i].enabled = true;
                // Enable the GameObject itself
                heartImagesManual[i].gameObject.SetActive(true);
            }
            else
            {
                // Destroy/hide the heart completely when empty
                heartImagesManual[i].gameObject.SetActive(false);
            }
        }
    }
    
    public void UpdateHearts(int currentHearts, int maxHearts)
    {
        // If using manual heart images, update those
        if (heartImagesManual != null && heartImagesManual.Length > 0)
        {
            UpdateManualHearts(currentHearts);
            return;
        }
        
        // Otherwise use dynamic creation
        // If hearts haven't been initialized and we have a container, initialize them
        if (heartImages.Count == 0 && heartContainer != null)
        {
            InitializeHearts();
        }
        
        // Update heart sprites - disable GameObjects when empty
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentHearts)
            {
                // Full heart - show
                if (fullHeartSprite != null)
                {
                    heartImages[i].sprite = fullHeartSprite;
                }
                heartImages[i].color = Color.white;
                heartImages[i].gameObject.SetActive(true);
            }
            else
            {
                // Empty heart - hide completely
                heartImages[i].gameObject.SetActive(false);
            }
        }
    }
}

