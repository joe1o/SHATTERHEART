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
                finished = true;
                PlayerPrefs.SetFloat("Level1_Time", elapsed);
                PlayerPrefs.Save();
                Debug.Log($"Level Complete! Time: {elapsed:F2}s");
                Invoke("RestartScene", 1f);
            }
        }
    }
    
    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
