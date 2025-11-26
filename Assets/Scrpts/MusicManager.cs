using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    
    [Header("Music Tracks")]
    public AudioClip[] musicTracks;
    
    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    public bool playOnStart = true;
    public bool shuffle = true;
    public bool loop = true;
    public bool persistAcrossScenes = false;  // If false, music stops on scene change
    public bool stopOnSceneChange = true;     // Stop music when scene changes
    
    [Header("Fade Settings")]
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;
    
    private AudioSource audioSource;
    private List<int> playlist = new List<int>();
    private int currentTrackIndex = 0;
    private bool isFading = false;
    
    void Awake()
    {
        if (persistAcrossScenes)
        {
            // Singleton pattern - persist across scenes
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Subscribe to scene change event
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            // Each scene has its own MusicManager
            Instance = this;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from scene change event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (stopOnSceneChange && persistAcrossScenes)
        {
            Stop();
        }
    }
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = musicVolume;
        
        GeneratePlaylist();
        
        if (playOnStart && musicTracks.Length > 0)
        {
            PlayCurrentTrack();
        }
    }
    
    void Update()
    {
        // Auto-play next track when current one ends
        if (loop && !audioSource.isPlaying && !isFading && musicTracks.Length > 0)
        {
            NextTrack();
        }
        
        // Update volume in real-time
        if (!isFading)
        {
            audioSource.volume = musicVolume;
        }
    }
    
    void GeneratePlaylist()
    {
        playlist.Clear();
        
        for (int i = 0; i < musicTracks.Length; i++)
        {
            playlist.Add(i);
        }
        
        if (shuffle)
        {
            ShufflePlaylist();
        }
    }
    
    void ShufflePlaylist()
    {
        // Fisher-Yates shuffle
        for (int i = playlist.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = playlist[i];
            playlist[i] = playlist[randomIndex];
            playlist[randomIndex] = temp;
        }
    }
    
    void PlayCurrentTrack()
    {
        if (playlist.Count == 0) return;
        
        int trackIndex = playlist[currentTrackIndex];
        AudioClip track = musicTracks[trackIndex];
        
        if (track != null)
        {
            audioSource.clip = track;
            audioSource.Play();
        }
    }
    
    public void NextTrack()
    {
        currentTrackIndex++;
        
        // Loop back to start and reshuffle
        if (currentTrackIndex >= playlist.Count)
        {
            currentTrackIndex = 0;
            if (shuffle)
            {
                ShufflePlaylist();
            }
        }
        
        PlayCurrentTrack();
    }
    
    public void PreviousTrack()
    {
        currentTrackIndex--;
        
        if (currentTrackIndex < 0)
        {
            currentTrackIndex = playlist.Count - 1;
        }
        
        PlayCurrentTrack();
    }
    
    public void PlayRandomTrack()
    {
        if (musicTracks.Length == 0) return;
        
        int randomIndex = Random.Range(0, musicTracks.Length);
        audioSource.clip = musicTracks[randomIndex];
        audioSource.Play();
    }
    
    public void PlayTrack(int index)
    {
        if (index < 0 || index >= musicTracks.Length) return;
        
        audioSource.clip = musicTracks[index];
        audioSource.Play();
    }
    
    public void Pause()
    {
        audioSource.Pause();
    }
    
    public void Resume()
    {
        audioSource.UnPause();
    }
    
    public void Stop()
    {
        audioSource.Stop();
    }
    
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        audioSource.volume = musicVolume;
    }
    
    public void FadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }
    
    public void FadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }
    
    System.Collections.IEnumerator FadeOutCoroutine()
    {
        isFading = true;
        float startVolume = audioSource.volume;
        float elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        
        audioSource.volume = 0f;
        audioSource.Pause();
        isFading = false;
    }
    
    System.Collections.IEnumerator FadeInCoroutine()
    {
        isFading = true;
        audioSource.volume = 0f;
        audioSource.UnPause();
        
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / fadeInDuration);
            yield return null;
        }
        
        audioSource.volume = musicVolume;
        isFading = false;
    }
    
    // Get current track name
    public string GetCurrentTrackName()
    {
        if (audioSource.clip != null)
        {
            return audioSource.clip.name;
        }
        return "No track playing";
    }
    
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }
}

