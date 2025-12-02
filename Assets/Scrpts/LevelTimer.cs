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
    
    void Start()
    {
        startTime = Time.time;
        
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
                // Check if all enemies are dead
                if (AllEnemiesDefeated())
                {
                    finished = true;
                    PlayerPrefs.SetFloat("Level1_Time", elapsed);
                    PlayerPrefs.Save();
                    Debug.Log($"Level Complete! Time: {elapsed:F2}s");
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
    
    void LoadLevelComplete()
    {
        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        SceneManager.LoadScene("LevelComplete");
    }
}
