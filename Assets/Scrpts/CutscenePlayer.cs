using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutscenePlayer : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public string nextSceneName = "lvl1";
    
    [Header("Skip Settings")]
    public bool canSkip = true;
    public KeyCode skipKey = KeyCode.Space;
    public float skipHoldTime = 0f;  // 0 = instant skip, >0 = hold to skip
    
    [Header("Fade Settings")]
    public bool fadeOut = true;
    public float fadeDuration = 1f;
    public CanvasGroup fadeCanvas;  // Optional black overlay for fade
    
    private float skipTimer = 0f;
    private bool isTransitioning = false;
    
    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
        
        if (videoPlayer != null)
        {
            // Subscribe to video end event
            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("No VideoPlayer assigned!");
        }
        
        // Start with fade canvas invisible
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
        }
    }
    
    void Update()
    {
        if (isTransitioning) return;
        
        HandleSkip();
    }
    
    void HandleSkip()
    {
        if (!canSkip) return;
        
        if (Input.GetKey(skipKey))
        {
            skipTimer += Time.deltaTime;
            
            if (skipTimer >= skipHoldTime)
            {
                SkipCutscene();
            }
        }
        else
        {
            skipTimer = 0f;
        }
        
        // Also skip on mouse click
        if (Input.GetMouseButtonDown(0))
        {
            if (skipHoldTime <= 0f)
            {
                SkipCutscene();
            }
        }
    }
    
    void OnVideoEnd(VideoPlayer vp)
    {
        LoadNextScene();
    }
    
    public void SkipCutscene()
    {
        if (isTransitioning) return;
        
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        
        LoadNextScene();
    }
    
    void LoadNextScene()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        
        if (fadeOut && fadeCanvas != null)
        {
            StartCoroutine(FadeAndLoad());
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
    
    System.Collections.IEnumerator FadeAndLoad()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (fadeCanvas != null)
            {
                fadeCanvas.alpha = elapsed / fadeDuration;
            }
            yield return null;
        }
        
        SceneManager.LoadScene(nextSceneName);
    }
    
    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}

