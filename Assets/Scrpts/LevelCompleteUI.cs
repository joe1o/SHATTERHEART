using UnityEngine;
using TMPro;

public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private string levelName = "Level1";
    
    void Start()
    {
        if (timerText == null)
            timerText = FindObjectOfType<TextMeshProUGUI>();
        
        DisplayTime();
    }
    
    void DisplayTime()
    {
        float time = PlayerPrefs.GetFloat($"{levelName}_Time", 0f);
        int minutes = (int)(time / 60f);
        float seconds = time % 60f;
        
        timerText.text = $"{minutes}:{seconds:00.00}";
    }
}
