using UnityEngine;

public enum CardType
{
    ReapersKiss,    // Yellow - Pistol
    WidowMaker,     // Green - Machine Gun
    Damnation,      // Red - Shotgun
    Heartbreaker,   // White - Katana
    Anguish         // Dark - Special
}

[System.Serializable]
public class Card
{
    public CardType cardType;
    public string cardName;
    public Sprite cardSprite;
    public int ammo;
    public int maxAmmo;
    
    // Weapon stats
    public float damage;
    public float fireRate;
    public float range;
    
    // Ability when discarded
    public AbilityType discardAbility;
    
    public Card(CardType type)
    {
        cardType = type;
        SetDefaultStats();
    }
    
    public Card Clone()
    {
        Card clone = new Card(cardType);
        clone.cardName = cardName;
        clone.cardSprite = cardSprite;
        clone.ammo = ammo;
        clone.maxAmmo = maxAmmo;
        clone.damage = damage;
        clone.fireRate = fireRate;
        clone.range = range;
        clone.discardAbility = discardAbility;
        return clone;
    }
    
    void SetDefaultStats()
    {
        switch (cardType)
        {
            case CardType.ReapersKiss:
                cardName = "Reaper's Kiss";
                maxAmmo = 6;
                ammo = 6;
                damage = 25f;
                fireRate = 4f;
                range = 100f;
                discardAbility = AbilityType.Dash;
                break;
                
            case CardType.WidowMaker:
                cardName = "Widow Maker";
                maxAmmo = 20;
                ammo = 20;
                damage = 10f;
                fireRate = 12f;
                range = 80f;
                discardAbility = AbilityType.Updraft;
                break;
                
            case CardType.Damnation:
                cardName = "Damnation";
                maxAmmo = 4;
                ammo = 4;
                damage = 15f;
                fireRate = 1.2f;
                range = 30f;
                discardAbility = AbilityType.Stomp;
                break;
                
            case CardType.Heartbreaker:
                cardName = "Heartbreaker";
                maxAmmo = 3;
                ammo = 3;
                damage = 50f;
                fireRate = 2f;
                range = 3f;
                discardAbility = AbilityType.Dash;
                break;
                
            case CardType.Anguish:
                cardName = "Anguish";
                maxAmmo = 1;
                ammo = 1;
                damage = 100f;
                fireRate = 1f;
                range = 50f;
                discardAbility = AbilityType.Stomp;
                break;
        }
    }
}

public enum AbilityType
{
    None,
    Dash,
    Updraft,
    Stomp
}

