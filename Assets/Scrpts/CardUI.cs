using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform cardContainer;       // Parent for card stack display
    public GameObject cardPrefab;             // Prefab for a single card
    
    [Header("Stack Display Settings")]
    public Vector2 cardSize = new Vector2(80, 120);
    public float stackOffset = 5f;            // Offset between stacked cards
    public float stackSpacing = 100f;         // Space between different stacks
    public Vector2 anchorPosition = new Vector2(100, 100);  // Bottom-right offset
    
    [Header("Current Card Highlight")]
    public float currentCardScale = 1.2f;
    public Color highlightColor = Color.white;
    public Color dimColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
    
    [Header("Ammo Display")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI cardNameText;
    
    [Header("Animation")]
    public float animationSpeed = 10f;
    
    private List<GameObject> cardObjects = new List<GameObject>();
    
    public void UpdateCardDisplay(List<List<Card>> stacks, int currentStackIndex)
    {
        // Clear existing cards
        ClearCards();
        
        if (stacks.Count == 0)
        {
            if (ammoText != null) ammoText.text = "";
            if (cardNameText != null) cardNameText.text = "No Cards";
            return;
        }
        
        // Create card visuals for each stack
        float xOffset = 0;
        
        for (int stackIndex = 0; stackIndex < stacks.Count; stackIndex++)
        {
            List<Card> stack = stacks[stackIndex];
            bool isCurrentStack = (stackIndex == currentStackIndex);
            
            // Create cards in stack (show stacked effect)
            for (int cardIndex = 0; cardIndex < stack.Count; cardIndex++)
            {
                Card card = stack[cardIndex];
                bool isTopCard = (cardIndex == stack.Count - 1);
                
                GameObject cardObj = CreateCardVisual(card, isCurrentStack && isTopCard);
                
                // Position the card
                RectTransform rt = cardObj.GetComponent<RectTransform>();
                
                // Stack offset (cards behind are slightly up and left)
                float yStackOffset = cardIndex * stackOffset;
                float xStackOffset = cardIndex * (stackOffset * 0.5f);
                
                rt.anchoredPosition = new Vector2(xOffset + xStackOffset, yStackOffset);
                
                // Scale and color based on current selection
                if (isCurrentStack)
                {
                    if (isTopCard)
                    {
                        rt.localScale = Vector3.one * currentCardScale;
                        SetCardColor(cardObj, highlightColor);
                    }
                    else
                    {
                        rt.localScale = Vector3.one;
                        SetCardColor(cardObj, dimColor);
                    }
                }
                else
                {
                    rt.localScale = Vector3.one * 0.8f;
                    SetCardColor(cardObj, dimColor);
                }
                
                cardObjects.Add(cardObj);
            }
            
            xOffset += stackSpacing;
        }
        
        // Update ammo and name display
        Card currentCard = stacks[currentStackIndex][stacks[currentStackIndex].Count - 1];
        
        if (ammoText != null)
        {
            ammoText.text = $"{currentCard.ammo}/{currentCard.maxAmmo}";
        }
        
        if (cardNameText != null)
        {
            cardNameText.text = currentCard.cardName;
        }
    }
    
    GameObject CreateCardVisual(Card card, bool isActive)
    {
        GameObject cardObj;
        
        if (cardPrefab != null)
        {
            cardObj = Instantiate(cardPrefab, cardContainer);
        }
        else
        {
            // Create simple card visual if no prefab
            cardObj = new GameObject("Card");
            cardObj.transform.SetParent(cardContainer);
            
            Image img = cardObj.AddComponent<Image>();
            img.sprite = card.cardSprite;
            img.preserveAspect = true;
            
            RectTransform rt = cardObj.GetComponent<RectTransform>();
            rt.sizeDelta = cardSize;
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(1, 0);
        }
        
        // Set sprite if using prefab with Image
        Image cardImage = cardObj.GetComponent<Image>();
        if (cardImage == null)
        {
            cardImage = cardObj.GetComponentInChildren<Image>();
        }
        
        if (cardImage != null && card.cardSprite != null)
        {
            cardImage.sprite = card.cardSprite;
        }
        
        // Setup RectTransform
        RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = cardSize;
        rectTransform.anchorMin = new Vector2(1, 0);
        rectTransform.anchorMax = new Vector2(1, 0);
        rectTransform.pivot = new Vector2(1, 0);
        
        return cardObj;
    }
    
    void SetCardColor(GameObject cardObj, Color color)
    {
        Image img = cardObj.GetComponent<Image>();
        if (img == null)
        {
            img = cardObj.GetComponentInChildren<Image>();
        }
        
        if (img != null)
        {
            img.color = color;
        }
    }
    
    void ClearCards()
    {
        foreach (GameObject card in cardObjects)
        {
            Destroy(card);
        }
        cardObjects.Clear();
    }
    
    // Animate card pickup
    public void AnimatePickup(Card card)
    {
        // Can add pickup animation here
        // Scale up then down, slide in from side, etc.
    }
    
    // Animate card discard
    public void AnimateDiscard()
    {
        // Can add discard animation here
        // Fly off screen, fade out, etc.
    }
}

