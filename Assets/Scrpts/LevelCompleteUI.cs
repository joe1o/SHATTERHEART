using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum Grade { S, A, B, C }

public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image gradeImage;
    [SerializeField] private string levelName = "Level1";
    
    [Header("Grade Times (in seconds)")]
    [SerializeField] private float sGradeTime = 45f;    // S grade: under 45 seconds
    [SerializeField] private float aGradeTime = 60f;    // A grade: under 1 minute
    [SerializeField] private float bGradeTime = 90f;    // B grade: under 1:30
    [SerializeField] private float cGradeTime = 120f;   // C grade: under 2 minutes
    
    [Header("Grade Sprites")]
    [SerializeField] private Sprite sGradeSprite;
    [SerializeField] private Sprite aGradeSprite;
    [SerializeField] private Sprite bGradeSprite;
    [SerializeField] private Sprite cGradeSprite;
    
    void Start()
    {
        if (timerText == null)
            timerText = FindObjectOfType<TextMeshProUGUI>();
        
        DisplayTime();
        DisplayGrade();
    }
    
    void DisplayTime()
    {
        float time = PlayerPrefs.GetFloat($"{levelName}_Time", 0f);
        int minutes = (int)(time / 60f);
        float seconds = time % 60f;
        
        timerText.text = $"{minutes}:{seconds:00.00}";
    }
    
    void DisplayGrade()
    {
        float time = PlayerPrefs.GetFloat($"{levelName}_Time", 0f);
        Grade grade = GetGrade(time);
        
        Sprite gradeSprite = GetGradeSprite(grade);
        if (gradeImage != null && gradeSprite != null)
        {
            gradeImage.sprite = gradeSprite;
        }
        
        Debug.Log($"Grade: {grade} for time {time:F2}s");
    }
    
    Grade GetGrade(float time)
    {
        if (time <= sGradeTime)
            return Grade.S;
        else if (time <= aGradeTime)
            return Grade.A;
        else if (time <= bGradeTime)
            return Grade.B;
        else
            return Grade.C;
    }
    
    Sprite GetGradeSprite(Grade grade)
    {
        switch (grade)
        {
            case Grade.S:
                return sGradeSprite;
            case Grade.A:
                return aGradeSprite;
            case Grade.B:
                return bGradeSprite;
            case Grade.C:
                return cGradeSprite;
            default:
                return null;
        }
    }
}
