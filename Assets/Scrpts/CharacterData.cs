using UnityEngine;

public enum CharacterType
{
    Cyan,   // 10% stronger discard effects
    Green,  // Extra health, 5% weaker abilities
    Orange  // 30% more ammo, 10% speed boost
}

// Static class to store selected character across scenes
public static class SelectedCharacter
{
    public static CharacterType character = CharacterType.Cyan;
    
    // Character bonuses
    public static float GetDiscardBonus()
    {
        return character == CharacterType.Cyan ? 1.1f : 
               character == CharacterType.Green ? 0.95f : 1f;
    }
    
    public static float GetHealthBonus()
    {
        return character == CharacterType.Green ? 1.25f : 1f; // Extra health
    }
    
    public static float GetAmmoBonus()
    {
        return character == CharacterType.Orange ? 1.3f : 1f; // 30% more ammo
    }
    
    public static float GetSpeedBonus()
    {
        return character == CharacterType.Orange ? 1.1f : 1f; // 10% speed boost
    }
}

