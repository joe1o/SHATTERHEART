using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class CardUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform cardContainer;
    public GameObject cardPrefab;
    
    [Header("Card Size Settings")]
    public Vector2 currentCardSize = new Vector2(120, 170);
    public Vector2 secondaryCardSize = new Vector2(90, 130);
    
    [Header("Card Positioning")]
    public Vector2 currentCardPosition = new Vector2(-180, 50);  // Current card position from bottom-right
    public float secondaryCardOffsetX = -100f;                    // How far LEFT of current card
    public float secondaryCardOffsetY = 10f;                      // Slight vertical offset
    
    [Header("Stack Offset (Multiple Same Cards)")]
    public float currentStackOffsetX = -5f;                       // X offset between stacked cards (current)
    public float currentStackOffsetY = 6f;                        // Y offset between stacked cards (current)
    public float secondaryStackOffsetX = -5f;                     // X offset between stacked cards (secondary)
    public float secondaryStackOffsetY = 6f;                      // Y offset between stacked cards (secondary)
    
    [Header("Card Rotation")]
    public float currentCardTilt = -3f;
    public float secondaryCardTilt = -8f;
    
    [Header("Card Appearance")]
    public Color currentCardColor = Color.white;
    public Color secondaryCardColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    
    [Header("Katana Card (Always on Right)")]
    public Sprite katanaSprite;
    public Vector2 katanaCardSize = new Vector2(80, 115);
    public Vector2 katanaPosition = new Vector2(-50, 50);        // Position from bottom-right
    public float katanaTilt = -5f;
    public Color katanaColor = new Color(0.95f, 0.9f, 0.85f, 1f);

    [Header("Shake Settings")]
    public float shakeIntensity = 8f;
    public float shakeDuration = 0.15f;

    [Header("Passive Weapon Icons (Far Right)")]
    public Sprite[] passiveWeaponIcons;
    public Vector2 passiveIconSize = new Vector2(45, 45);
    public Vector2 passiveIconsStartPos = new Vector2(-15, 55);
    public float passiveIconSpacing = 50f;
    public Color passiveIconColor = new Color(0.55f, 0.45f, 0.35f, 1f);
    
    [Header("Ammo Display")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI cardNameText;
    
    [Header("Animation Settings")]
    public float animationSpeed = 12f;
    public float pickupSlideDistance = 200f;
    
    private class CardDisplay
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public Image image;
        public Vector2 targetPosition;
        public Vector3 targetScale;
        public Quaternion targetRotation;
        public Color targetColor;
        public bool isCurrent;
    }
    
    private List<CardDisplay> cardDisplays = new List<CardDisplay>();
    private GameObject katanaCard = null;
    private List<GameObject> passiveIcons = new List<GameObject>();
    private bool isAnimatingSwap = false;
    private bool isShaking = false;
    private Dictionary<CardDisplay, Vector2> originalPositions = new Dictionary<CardDisplay, Vector2>();


    void Start()
    {
        CreateKatanaCard();
        CreatePassiveIcons();
    }
    
    void Update()
    {
        if (isAnimatingSwap) return;
        foreach (CardDisplay cd in cardDisplays)
        {
            if (cd.rectTransform != null)
            {
                cd.rectTransform.anchoredPosition = Vector2.Lerp(
                    cd.rectTransform.anchoredPosition,
                    cd.targetPosition,
                    Time.deltaTime * animationSpeed
                );
                
                cd.rectTransform.localScale = Vector3.Lerp(
                    cd.rectTransform.localScale,
                    cd.targetScale,
                    Time.deltaTime * animationSpeed
                );
                
                cd.rectTransform.localRotation = Quaternion.Lerp(
                    cd.rectTransform.localRotation,
                    cd.targetRotation,
                    Time.deltaTime * animationSpeed
                );
                
                if (cd.image != null)
                {
                    cd.image.color = Color.Lerp(
                        cd.image.color,
                        cd.targetColor,
                        Time.deltaTime * animationSpeed
                    );
                }
            }
        }
    }

    public void ShakeCards()
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCardsCoroutine());
        }
    }

    private System.Collections.IEnumerator ShakeCardsCoroutine()
    {
        isShaking = true;

        // Store original target positions ONLY for current cards
        originalPositions.Clear();
        foreach (var cd in cardDisplays)
        {
            if (cd.rectTransform != null && cd.isCurrent) // Only shake current cards
            {
                originalPositions[cd] = cd.targetPosition;
            }
        }

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shakeDuration;

            // Smooth curve: goes down quickly, comes back up smoothly
            // Using a bounce-back easing function
            float curve = Mathf.Sin(t * Mathf.PI); // Creates smooth arc from 0 to 1 back to 0

            // Apply downward offset based on curve
            Vector2 offset = new Vector2(0, -shakeIntensity * curve);

            foreach (var cd in cardDisplays)
            {
                if (cd.rectTransform != null && originalPositions.ContainsKey(cd))
                {
                    cd.rectTransform.anchoredPosition = originalPositions[cd] + offset;
                }
            }

            yield return null;
        }

        // Reset to original positions
        foreach (var cd in cardDisplays)
        {
            if (cd.rectTransform != null && originalPositions.ContainsKey(cd))
            {
                cd.rectTransform.anchoredPosition = originalPositions[cd];
            }
        }

        originalPositions.Clear();
        isShaking = false;
    }

    // ====================================
    // CIRCULAR SLOT SWAP ANIMATION
    // ====================================
    public void AnimateCircularSwap(int currentIndex)
    {
        StartCoroutine(SwapAnimation(currentIndex));
    }

    private IEnumerator SwapAnimation(int currentIndex)
    {
        if (cardDisplays.Count < 2) yield break;

        isAnimatingSwap = true;

        // Identify current stack & previous stack
        List<CardDisplay> currentCards = new List<CardDisplay>();
        List<CardDisplay> otherCards = new List<CardDisplay>();

        foreach (var cd in cardDisplays)
        {
            if (cd.isCurrent) currentCards.Add(cd);
            else otherCards.Add(cd);
        }

        if (currentCards.Count == 0 || otherCards.Count == 0)
        {
            isAnimatingSwap = false;
            yield break;
        }

        // Store start/end data for ALL cards in both stacks
        List<Vector2> currentStartPositions = new List<Vector2>();
        List<Vector2> currentEndPositions = new List<Vector2>();
        List<Vector3> currentStartScales = new List<Vector3>();
        List<Vector3> currentEndScales = new List<Vector3>();

        List<Vector2> otherStartPositions = new List<Vector2>();
        List<Vector2> otherEndPositions = new List<Vector2>();
        List<Vector3> otherStartScales = new List<Vector3>();
        List<Vector3> otherEndScales = new List<Vector3>();

        // Target scales
        float currentToSecondaryRatio = secondaryCardSize.x / currentCardSize.x;
        float secondaryToCurrentRatio = currentCardSize.x / secondaryCardSize.x;

        // Capture data for ALL current cards
        for (int i = 0; i < currentCards.Count; i++)
        {
            CardDisplay cd = currentCards[i];
            currentStartPositions.Add(cd.rectTransform.anchoredPosition);
            currentStartScales.Add(cd.rectTransform.localScale);
            

            // Find corresponding target position in otherCards
            if (i < otherCards.Count)
            {
                currentEndPositions.Add(otherCards[i].targetPosition);
            }
            else
            {
                currentEndPositions.Add(otherCards[0].targetPosition);
            }

            currentEndScales.Add(cd.rectTransform.localScale * currentToSecondaryRatio);
        }

        // Capture data for ALL other cards
        for (int i = 0; i < otherCards.Count; i++)
        {
            CardDisplay cd = otherCards[i];
            otherStartPositions.Add(cd.rectTransform.anchoredPosition);
            otherStartScales.Add(cd.rectTransform.localScale);
            

            // Find corresponding target position in currentCards
            if (i < currentCards.Count)
            {
                otherEndPositions.Add(currentCards[i].targetPosition);
            }
            else
            {
                otherEndPositions.Add(currentCards[0].targetPosition);
            }

            otherEndScales.Add(cd.rectTransform.localScale * secondaryToCurrentRatio);
        }

        float duration = 0.2f;
        float elapsed = 0f;
        float radius = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = Mathf.SmoothStep(0f, 1f, t);

            float angle = Mathf.Lerp(0f, 180f, t);
            float rad = angle * Mathf.Deg2Rad;

            float xOffset = Mathf.Cos(rad) * radius;
            float yOffset = Mathf.Sin(rad) * radius;

            // Animate ALL current cards going to secondary
            for (int i = 0; i < currentCards.Count; i++)
            {
                CardDisplay cd = currentCards[i];

                cd.rectTransform.anchoredPosition =
                    Vector2.Lerp(currentStartPositions[i], currentEndPositions[i], easeT) +
                    new Vector2(xOffset, -yOffset);

                cd.rectTransform.localScale = Vector3.Lerp(currentStartScales[i], currentEndScales[i], easeT);

            }

            // Animate ALL other cards coming to current
            for (int i = 0; i < otherCards.Count; i++)
            {
                CardDisplay cd = otherCards[i];
                bool isTopCard = (i == otherCards.Count - 1);

                cd.rectTransform.anchoredPosition =
                    Vector2.Lerp(otherStartPositions[i], otherEndPositions[i], easeT) +
                    new Vector2(xOffset, yOffset);

                cd.rectTransform.localScale = Vector3.Lerp(otherStartScales[i], otherEndScales[i], easeT);

            }

            yield return null;
        }

        // Force a full rebuild WITHOUT slide-in animation
        ClearCards();
        RebuildCards(
            CardManager.Instance.GetAllStacks(),
            CardManager.Instance.GetCurrentStackIndex()
            
        );

        isAnimatingSwap = false;
    }


    void CreateKatanaCard()
    {
        if (katanaSprite == null) return;
        
        katanaCard = new GameObject("KatanaCard");
        katanaCard.transform.SetParent(cardContainer);
        
        Image img = katanaCard.AddComponent<Image>();
        img.sprite = katanaSprite;
        img.preserveAspect = true;
        img.color = katanaColor;
        
        RectTransform rt = katanaCard.GetComponent<RectTransform>();
        rt.sizeDelta = katanaCardSize;
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = katanaPosition;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.Euler(0, 0, katanaTilt);
        
        // Katana should be behind the collectible cards
        rt.SetAsFirstSibling();
    }
    
    void CreatePassiveIcons()
    {
        if (passiveWeaponIcons == null || passiveWeaponIcons.Length == 0) return;
        
        for (int i = 0; i < passiveWeaponIcons.Length; i++)
        {
            if (passiveWeaponIcons[i] == null) continue;
            
            GameObject iconObj = new GameObject($"PassiveIcon_{i}");
            iconObj.transform.SetParent(cardContainer);
            
            Image img = iconObj.AddComponent<Image>();
            img.sprite = passiveWeaponIcons[i];
            img.preserveAspect = true;
            img.color = passiveIconColor;
            
            RectTransform rt = iconObj.GetComponent<RectTransform>();
            rt.sizeDelta = passiveIconSize;
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(1, 0.5f);
            
            // Stack vertically on far right
            float yPos = passiveIconsStartPos.y + (i * passiveIconSpacing);
            rt.anchoredPosition = new Vector2(passiveIconsStartPos.x, yPos);
            rt.localScale = Vector3.one;
            
            // Set behind everything
            rt.SetAsFirstSibling();
            
            passiveIcons.Add(iconObj);
        }
    }
    
    public void UpdateCardDisplay(List<List<Card>> stacks, int currentStackIndex)
    {
        if (stacks.Count == 0)
        {
            ClearCards();
            if (ammoText != null) ammoText.text = "";
            if (cardNameText != null) cardNameText.text = "";
            return;
        }
        
        int totalCards = 0;
        foreach (var stack in stacks) totalCards += stack.Count;
        
        if (cardDisplays.Count != totalCards)
        {
            RebuildCards(stacks, currentStackIndex);
        }
        else
        {
            UpdateCardPositions(stacks, currentStackIndex);
        }
        
        // Update ammo
        if (currentStackIndex >= 0 && currentStackIndex < stacks.Count && stacks[currentStackIndex].Count > 0)
        {
            Card currentCard = stacks[currentStackIndex][stacks[currentStackIndex].Count - 1];
            if (ammoText != null) ammoText.text = currentCard.ammo.ToString();
            if (cardNameText != null) cardNameText.text = currentCard.cardName;
        }
    }
    
    void RebuildCards(List<List<Card>> stacks, int currentStackIndex)
    {
        ClearCards();
        
        // Layout:
        // [Secondary cards LEFT] [CURRENT CARD] [KATANA RIGHT] [Passive icons FAR RIGHT]
        
        // First, create secondary stacks (to the LEFT of current)
        int secondaryPosition = 0;
        for (int stackIdx = 0; stackIdx < stacks.Count; stackIdx++)
        {
            if (stackIdx == currentStackIndex) continue;
            
            List<Card> stack = stacks[stackIdx];
            secondaryPosition++;
            
            for (int cardIdx = 0; cardIdx < stack.Count; cardIdx++)
            {
                Card card = stack[cardIdx];
                
                CardDisplay cd = CreateCardDisplay(card, false);
                
                // Position to the LEFT of current card
                float xPos = currentCardPosition.x + (secondaryCardOffsetX * secondaryPosition) + (cardIdx * secondaryStackOffsetX);
                float yPos = currentCardPosition.y + secondaryCardOffsetY + (cardIdx * secondaryStackOffsetY);
                
                cd.targetPosition = new Vector2(xPos, yPos);
                cd.rectTransform.anchoredPosition = cd.targetPosition;
                
                cd.targetScale = Vector3.one;
                cd.targetRotation = Quaternion.Euler(0, 0, secondaryCardTilt);
                cd.targetColor = secondaryCardColor;
                cd.isCurrent = false;
                
                cardDisplays.Add(cd);
            }
        }
        
        // Then, create current stack (main position)
        if (currentStackIndex >= 0 && currentStackIndex < stacks.Count)
        {
            List<Card> currentStack = stacks[currentStackIndex];
            
            for (int cardIdx = 0; cardIdx < currentStack.Count; cardIdx++)
            {
                Card card = currentStack[cardIdx];
                bool isTopCard = (cardIdx == currentStack.Count - 1);
                
                CardDisplay cd = CreateCardDisplay(card, true);
                
                // Stack offset for multiple of same card
                float xPos = currentCardPosition.x + (cardIdx * currentStackOffsetX);
                float yPos = currentCardPosition.y + (cardIdx * currentStackOffsetY);
                
                cd.targetPosition = new Vector2(xPos, yPos);

                // Only apply slide-in animation if requested
                if (isAnimatingSwap == false)
                {
                    cd.rectTransform.anchoredPosition = cd.targetPosition + Vector2.right * pickupSlideDistance;
                }
                else
                {
                    cd.rectTransform.anchoredPosition = cd.targetPosition;
                }

                //cd.rectTransform.anchoredPosition = cd.targetPosition + Vector2.right * pickupSlideDistance;

                cd.targetScale = Vector3.one;
                cd.targetRotation = Quaternion.Euler(0, 0, currentCardTilt);
                cd.targetColor = isTopCard ? currentCardColor : new Color(0.95f, 0.95f, 0.95f, 1f);
                cd.isCurrent = true;
                
                cardDisplays.Add(cd);
            }
        }
        
        // Ensure proper layering: secondary behind, current in front
        ReorderCards();
    }
    
    void UpdateCardPositions(List<List<Card>> stacks, int currentStackIndex)
    {
        int displayIndex = 0;
        
        // Update secondary stacks (LEFT)
        int secondaryPosition = 0;
        for (int stackIdx = 0; stackIdx < stacks.Count; stackIdx++)
        {
            if (stackIdx == currentStackIndex) continue;
            
            List<Card> stack = stacks[stackIdx];
            secondaryPosition++;
            
            for (int cardIdx = 0; cardIdx < stack.Count; cardIdx++)
            {
                if (displayIndex >= cardDisplays.Count) break;
                
                CardDisplay cd = cardDisplays[displayIndex];
                Card card = stack[cardIdx];
                
                if (cd.image != null && card.cardSprite != null)
                    cd.image.sprite = card.cardSprite;
                
                float xPos = currentCardPosition.x + (secondaryCardOffsetX * secondaryPosition) + (cardIdx * secondaryStackOffsetX);
                float yPos = currentCardPosition.y + secondaryCardOffsetY + (cardIdx * secondaryStackOffsetY);
                
                cd.targetPosition = new Vector2(xPos, yPos);
                cd.targetScale = Vector3.one;
                cd.targetRotation = Quaternion.Euler(0, 0, secondaryCardTilt);
                cd.targetColor = secondaryCardColor;
                cd.isCurrent = false;
                
                displayIndex++;
            }
        }
        
        // Update current stack
        if (currentStackIndex >= 0 && currentStackIndex < stacks.Count)
        {
            List<Card> currentStack = stacks[currentStackIndex];
            
            for (int cardIdx = 0; cardIdx < currentStack.Count; cardIdx++)
            {
                if (displayIndex >= cardDisplays.Count) break;
                
                CardDisplay cd = cardDisplays[displayIndex];
                Card card = currentStack[cardIdx];
                bool isTopCard = (cardIdx == currentStack.Count - 1);
                
                if (cd.image != null && card.cardSprite != null)
                    cd.image.sprite = card.cardSprite;
                
                float xPos = currentCardPosition.x + (cardIdx * currentStackOffsetX);
                float yPos = currentCardPosition.y + (cardIdx * currentStackOffsetY);
                
                cd.targetPosition = new Vector2(xPos, yPos);
                cd.targetScale = Vector3.one;
                cd.targetRotation = Quaternion.Euler(0, 0, currentCardTilt);
                cd.targetColor = isTopCard ? currentCardColor : new Color(0.95f, 0.95f, 0.95f, 1f);
                cd.isCurrent = true;
                
                displayIndex++;
            }
        }
        
        ReorderCards();
    }
    
    void ReorderCards()
    {
        // Put secondary cards behind, current cards in front
        int siblingIndex = 0;
        
        // Passive icons and katana first (furthest back)
        foreach (var icon in passiveIcons)
        {
            if (icon != null) icon.transform.SetSiblingIndex(siblingIndex++);
        }
        if (katanaCard != null) katanaCard.transform.SetSiblingIndex(siblingIndex++);
        
        // Secondary cards next
        foreach (var cd in cardDisplays)
        {
            if (!cd.isCurrent && cd.rectTransform != null)
            {
                cd.rectTransform.SetSiblingIndex(siblingIndex++);
            }
        }
        
        // Current cards last (front)
        foreach (var cd in cardDisplays)
        {
            if (cd.isCurrent && cd.rectTransform != null)
            {
                cd.rectTransform.SetSiblingIndex(siblingIndex++);
            }
        }
    }
    
    CardDisplay CreateCardDisplay(Card card, bool isCurrent)
    {
        GameObject cardObj;
        
        if (cardPrefab != null)
        {
            cardObj = Instantiate(cardPrefab, cardContainer);
        }
        else
        {
            cardObj = new GameObject("Card");
            cardObj.transform.SetParent(cardContainer);
            cardObj.AddComponent<Image>();
        }
        
        RectTransform rt = cardObj.GetComponent<RectTransform>();
        rt.sizeDelta = isCurrent ? currentCardSize : secondaryCardSize;
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        
        Image img = cardObj.GetComponent<Image>();
        if (img == null) img = cardObj.GetComponentInChildren<Image>();
        
        if (img != null && card.cardSprite != null)
        {
            img.sprite = card.cardSprite;
            img.preserveAspect = true;
        }
        
        return new CardDisplay
        {
            gameObject = cardObj,
            rectTransform = rt,
            image = img,
            targetPosition = Vector2.zero,
            targetScale = Vector3.one,
            targetRotation = Quaternion.identity,
            targetColor = Color.white,
            isCurrent = isCurrent
        };
    }
    
    void ClearCards()
    {
        foreach (CardDisplay cd in cardDisplays)
        {
            if (cd.gameObject != null)
                Destroy(cd.gameObject);
        }
        cardDisplays.Clear();
    }
    
    public void AnimatePickup()
    {
        if (cardDisplays.Count > 0)
        {
            CardDisplay newest = cardDisplays[cardDisplays.Count - 1];
            newest.rectTransform.anchoredPosition = newest.targetPosition + Vector2.right * pickupSlideDistance;
        }
    }
    
    public void AnimateDiscard()
    {
        // Find current top card and animate it
        foreach (var cd in cardDisplays)
        {
            if (cd.isCurrent)
            {
                StartCoroutine(DiscardAnimation(cd));
                break;
            }
        }
    }
    
    System.Collections.IEnumerator DiscardAnimation(CardDisplay cd)
    {
        if (cd.rectTransform == null) yield break;
        
        float elapsed = 0f;
        float duration = 0.2f;
        Vector2 startPos = cd.rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(300f, 80f);
        Vector3 startScale = cd.rectTransform.localScale;
        Color startColor = cd.image != null ? cd.image.color : Color.white;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            cd.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t * t);
            cd.rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            cd.rectTransform.localRotation *= Quaternion.Euler(0, 0, Time.deltaTime * 720f);
            
            if (cd.image != null)
            {
                Color c = startColor;
                c.a = 1f - t;
                cd.image.color = c;
            }
            
            yield return null;
        }
    }
}
