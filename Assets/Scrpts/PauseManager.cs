using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    
    [SerializeField] private GameObject pauseMenuUI;
    private bool isPaused = false;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        if (pauseMenuUI == null)
        {
            // Try to find by name first
            Transform pauseCanvas = transform.parent?.Find("PauseMenuCanvas");
            if (pauseCanvas == null)
            {
                pauseMenuUI = GameObject.Find("PauseMenuCanvas");
            }
            else
            {
                pauseMenuUI = pauseCanvas.gameObject;
            }
            
            if (pauseMenuUI == null)
            {
                Debug.LogError("PauseMenuCanvas not found!");
                return;
            }
        }
        Debug.Log("Pause menu UI found: " + pauseMenuUI.name);
        pauseMenuUI.SetActive(false);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }
    
    public void Pause()
    {
        Debug.Log("Pause called");
        isPaused = true;
        Time.timeScale = 0f; // Freeze game
        pauseMenuUI.SetActive(true);
        Debug.Log("Pause menu activated");
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume game
        pauseMenuUI.SetActive(false);
        Debug.Log("Game resumed");
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void Quit()
    {
        Time.timeScale = 1f; // Resume time before quitting
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public bool IsPaused()
    {
        return isPaused;
    }
}
