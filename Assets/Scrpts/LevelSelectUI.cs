using UnityEngine;
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Level Info")]
    public string levelName = "Level1"; // e.g., "Level1", "Level2", etc.
    
    [Header("UI References")]
    public TextMeshProUGUI bestTimeText;
    
    void Start()
    {
        DisplayBestTime();
    }
    
    void DisplayBestTime()
    {
        float bestTime = PlayerPrefs.GetFloat($"{levelName}_Time", -1f);
        
        if (bestTimeText != null)
        {
            if (bestTime < 0)
            {
                // No time recorded yet
                bestTimeText.text = "--:--.--";
            }
            else
            {
                // Format time as MM:SS.MS
                int minutes = (int)(bestTime / 60f);
                float seconds = bestTime % 60f;
                bestTimeText.text = $"{minutes}:{seconds:00.00}";
            }
        }
    }
    
    // Optional: Call this to refresh the display
    public void RefreshBestTime()
    {
        DisplayBestTime();
    }
}
