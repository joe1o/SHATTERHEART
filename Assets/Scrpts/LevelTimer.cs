using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    private float startTime;
    private bool finished = false;
    
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Collider levelEndCollider;
    [SerializeField] private Transform playerTransform;
    
    private string currentLevelName;
    
    void Start()
    {
        startTime = Time.time;
        
        // Get current scene name
        currentLevelName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("CurrentLevel", currentLevelName);
        
        // Save as last level played (for Continue button)
        if (currentLevelName.StartsWith("Level"))
        {
            PlayerPrefs.SetString("LastLevelPlayed", currentLevelName);
            PlayerPrefs.Save();
        }
        
        if (timerText == null)
            timerText = FindObjectOfType<TextMeshProUGUI>();
        
        if (playerTransform == null)
            playerTransform = FindObjectOfType<FirstPersonController>()?.transform;
        
        if (levelEndCollider == null)
        {
            foreach (Collider col in FindObjectsOfType<Collider>())
            {
                if (col.CompareTag("level end"))
                {
                    levelEndCollider = col;
                    break;
                }
            }
        }
    }
    
    void Update()
    {
        float elapsed = Time.time - startTime;
        int minutes = (int)(elapsed / 60f);
        float seconds = elapsed % 60f;
        timerText.text = $"{minutes}:{seconds:00.00}";
        
        if (!finished && playerTransform != null && levelEndCollider != null)
        {
            if (levelEndCollider.bounds.Contains(playerTransform.position))
            {
                // Check if all enemies and balloons are destroyed
                if (AllEnemiesDefeated() && AllBalloonsDestroyed())
                {
                    finished = true;
                    
                    // Only save if it's a new best time (or first time)
                    float currentBest = PlayerPrefs.GetFloat($"{currentLevelName}_Time", float.MaxValue);
                    if (elapsed < currentBest)
                    {
                        PlayerPrefs.SetFloat($"{currentLevelName}_Time", elapsed);
                        PlayerPrefs.Save();
                        Debug.Log($"New Best Time! {elapsed:F2}s");
                    }
                    else
                    {
                        Debug.Log($"Level Complete! Time: {elapsed:F2}s (Best: {currentBest:F2}s)");
                    }
                    
                    Invoke("LoadLevelComplete", 0.5f);
                }
            }
        }
    }
    
    bool AllEnemiesDefeated()
    {
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        
        // Filter only enemies with "enemy" tag
        System.Collections.Generic.List<Enemy> actualEnemies = new System.Collections.Generic.List<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy.CompareTag("enemy"))
            {
                actualEnemies.Add(enemy);
            }
        }
        
        Debug.Log($"Actual enemies found: {actualEnemies.Count}");
        
        if (actualEnemies.Count == 0)
        {
            Debug.Log("No enemies in scene - level can be completed!");
            return true;
        }
        
        foreach (Enemy enemy in actualEnemies)
        {
            Debug.Log($"Enemy {enemy.name} - Dead: {enemy.IsDead()}");
            if (!enemy.IsDead())
            {
                Debug.Log("Cannot complete level - enemies still alive!");
                return false;
            }
        }
        
        Debug.Log("All enemies defeated!");
        return true;
    }
    
    bool AllBalloonsDestroyed()
    {
        GameObject[] allBalloons = GameObject.FindGameObjectsWithTag("Baloon");
        
        Debug.Log($"Balloons found: {allBalloons.Length}");
        
        if (allBalloons.Length == 0)
        {
            Debug.Log("All balloons destroyed!");
            return true;
        }
        
        Debug.Log("Cannot complete level - balloons still exist!");
        return false;
    }
    
    void LoadLevelComplete()
    {
        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        SceneManager.LoadScene("LevelComplete");
    }
}
