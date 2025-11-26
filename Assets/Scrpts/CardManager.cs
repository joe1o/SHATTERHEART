using UnityEngine;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    
    [Header("Card Limits")]
    public int maxDifferentCards = 2;      // Max 2 different card types
    public int maxSameCards = 3;           // Max 3 of same card type
    
    [Header("Card Sprites")]
    public Sprite reapersKissSprite;
    public Sprite widowMakerSprite;
    public Sprite damnationSprite;
    public Sprite heartbreakerSprite;
    public Sprite anguishSprite;
    
    [Header("References")]
    public CardUI cardUI;
    public PlayerAbilities playerAbilities;
    public Shooting shootingScript;
    
    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip discardSound;
    public AudioClip switchSound;
    [Range(0f, 1f)] public float cardSoundVolume = 0.6f;
    
    // Card stacks - each stack is a list of same card type
    private List<List<Card>> cardStacks = new List<List<Card>>();
    private int currentStackIndex = 0;
    
    private AudioSource audioSource;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Set default weapon to Katana when no cards
        ApplyDefaultWeapon();
        UpdateUI();
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // Switch cards with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SwitchToNextStack();
        }
        else if (scroll < 0f)
        {
            SwitchToPreviousStack();
        }
        
        // Discard card with right click
        if (Input.GetMouseButtonDown(1))
        {
            DiscardCurrentCard();
        }
        
        // Number keys to switch stacks
        if (Input.GetKeyDown(KeyCode.Alpha1) && cardStacks.Count > 0)
            SwitchToStack(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) && cardStacks.Count > 1)
            SwitchToStack(1);
    }
    
    public bool CanPickupCard(CardType type)
    {
        // Check if we already have this card type
        int existingStackIndex = FindStackIndex(type);
        
        if (existingStackIndex >= 0)
        {
            // Already have this type - check if we can add more
            return cardStacks[existingStackIndex].Count < maxSameCards;
        }
        else
        {
            // New card type - check if we have room
            return cardStacks.Count < maxDifferentCards;
        }
    }
    
    public bool PickupCard(CardType type)
    {
        if (!CanPickupCard(type)) return false;
        
        Card newCard = CreateCard(type);
        
        int existingStackIndex = FindStackIndex(type);
        
        if (existingStackIndex >= 0)
        {
            // Add to existing stack
            cardStacks[existingStackIndex].Add(newCard);
        }
        else
        {
            // Create new stack
            List<Card> newStack = new List<Card> { newCard };
            cardStacks.Add(newStack);
            
            // Auto-switch to new card if it's our first
            if (cardStacks.Count == 1)
            {
                currentStackIndex = 0;
                ApplyCurrentCard();
            }
        }
        
        PlaySound(pickupSound);
        UpdateUI();
        return true;
    }
    
    Card CreateCard(CardType type)
    {
        Card card = new Card(type);
        card.cardSprite = GetSpriteForType(type);
        return card;
    }
    
    Sprite GetSpriteForType(CardType type)
    {
        switch (type)
        {
            case CardType.ReapersKiss: return reapersKissSprite;
            case CardType.WidowMaker: return widowMakerSprite;
            case CardType.Damnation: return damnationSprite;
            case CardType.Heartbreaker: return heartbreakerSprite;
            case CardType.Anguish: return anguishSprite;
            default: return null;
        }
    }
    
    int FindStackIndex(CardType type)
    {
        for (int i = 0; i < cardStacks.Count; i++)
        {
            if (cardStacks[i].Count > 0 && cardStacks[i][0].cardType == type)
                return i;
        }
        return -1;
    }
    
    public void SwitchToNextStack()
    {
        if (cardStacks.Count <= 1) return;
        
        currentStackIndex = (currentStackIndex + 1) % cardStacks.Count;
        ApplyCurrentCard();
        PlaySound(switchSound);
        UpdateUI();
    }
    
    public void SwitchToPreviousStack()
    {
        if (cardStacks.Count <= 1) return;
        
        currentStackIndex = (currentStackIndex - 1 + cardStacks.Count) % cardStacks.Count;
        ApplyCurrentCard();
        PlaySound(switchSound);
        UpdateUI();
    }
    
    public void SwitchToStack(int index)
    {
        if (index < 0 || index >= cardStacks.Count) return;
        if (index == currentStackIndex) return;
        
        currentStackIndex = index;
        ApplyCurrentCard();
        PlaySound(switchSound);
        UpdateUI();
    }
    
    void ApplyCurrentCard()
    {
        Card current = GetCurrentCard();
        
        // No cards - use default Katana
        if (current == null)
        {
            ApplyDefaultWeapon();
            return;
        }
        
        // Apply weapon type to shooting script
        if (shootingScript != null)
        {
            switch (current.cardType)
            {
                case CardType.ReapersKiss:
                    shootingScript.SetWeapon(WeaponType.Pistol);
                    break;
                case CardType.WidowMaker:
                    shootingScript.SetWeapon(WeaponType.MachineGun);
                    break;
                case CardType.Damnation:
                    shootingScript.SetWeapon(WeaponType.Shotgun);
                    break;
                case CardType.Heartbreaker:
                case CardType.Anguish:
                    shootingScript.SetWeapon(WeaponType.Katana);
                    break;
            }
        }
    }
    
    void ApplyDefaultWeapon()
    {
        // Default weapon is Katana when no cards are collected
        if (shootingScript != null)
        {
            shootingScript.SetWeapon(WeaponType.Katana);
        }
    }
    
    public void DiscardCurrentCard()
    {
        if (cardStacks.Count == 0) return;
        
        Card current = GetCurrentCard();
        if (current == null) return;
        
        // Use the ability
        UseDiscardAbility(current.discardAbility);
        
        // Remove the card
        List<Card> currentStack = cardStacks[currentStackIndex];
        currentStack.RemoveAt(currentStack.Count - 1);
        
        // Remove empty stack
        if (currentStack.Count == 0)
        {
            cardStacks.RemoveAt(currentStackIndex);
            
            // Adjust current index
            if (cardStacks.Count > 0)
            {
                currentStackIndex = Mathf.Clamp(currentStackIndex, 0, cardStacks.Count - 1);
                ApplyCurrentCard();
            }
            else
            {
                // No cards left - use default Katana
                ApplyDefaultWeapon();
            }
        }
        
        PlaySound(discardSound);
        UpdateUI();
    }
    
    void UseDiscardAbility(AbilityType ability)
    {
        if (playerAbilities == null) return;
        
        switch (ability)
        {
            case AbilityType.Dash:
                if (playerAbilities.CanDash())
                    playerAbilities.StartDash();
                break;
            case AbilityType.Updraft:
                if (playerAbilities.CanUpdraft())
                    playerAbilities.PerformUpdraft();
                break;
            case AbilityType.Stomp:
                if (playerAbilities.CanStomp())
                    playerAbilities.StartStomp();
                break;
        }
    }
    
    public void UseAmmo()
    {
        Card current = GetCurrentCard();
        if (current == null) return;
        
        current.ammo--;
        
        // Auto-discard when out of ammo
        if (current.ammo <= 0)
        {
            DiscardCurrentCard();
        }
        else
        {
            UpdateUI();
        }
    }
    
    public Card GetCurrentCard()
    {
        if (cardStacks.Count == 0) return null;
        if (currentStackIndex >= cardStacks.Count) return null;
        
        List<Card> stack = cardStacks[currentStackIndex];
        if (stack.Count == 0) return null;
        
        return stack[stack.Count - 1]; // Top card
    }
    
    public List<List<Card>> GetAllStacks()
    {
        return cardStacks;
    }
    
    public int GetCurrentStackIndex()
    {
        return currentStackIndex;
    }
    
    public int GetCurrentStackCount()
    {
        if (currentStackIndex >= cardStacks.Count) return 0;
        return cardStacks[currentStackIndex].Count;
    }
    
    void UpdateUI()
    {
        if (cardUI != null)
        {
            cardUI.UpdateCardDisplay(cardStacks, currentStackIndex);
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, cardSoundVolume);
        }
    }
    
    // For testing - give player starting cards
    public void GiveStartingCards()
    {
        PickupCard(CardType.ReapersKiss);
    }
}

