using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;
    
    [Header("Health Settings")]
    public int maxHearts = 3;
    private int currentHearts;
    
    [Header("Death Settings")]
    public float respawnDelay = 2f;
    
    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;
    public AudioClip deathSound;
    [Range(0f, 1f)] public float hitVolume = 0.7f;
    
    [Header("References")]
    public PlayerHealthUI healthUI;
    
    private AudioSource audioSource;
    private bool isDead = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        currentHearts = maxHearts;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Update UI on start
        UpdateUI();
    }
    
    public void TakeDamage(int damage = 1)
    {
        if (isDead) return;
        
        currentHearts -= damage;
        
        // Clamp health
        if (currentHearts < 0) currentHearts = 0;
        
        // Visual/audio feedback
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound, hitVolume);
        }
        
        // Camera shake
        CameraEffects camFX = Camera.main.GetComponent<CameraEffects>();
        if (camFX != null)
        {
            camFX.ApplyCameraShake(0.3f, 0.15f);
        }
        
        // Update UI
        UpdateUI();
        
        if (currentHearts <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Disable player controls
        FirstPersonController controller = GetComponent<FirstPersonController>();
        if (controller != null)
        {
            controller.PauseMovement(true);
        }
        
        // Disable shooting
        Shooting shooting = GetComponent<Shooting>();
        if (shooting != null)
        {
            shooting.enabled = false;
        }
        
        // Reload current scene after delay
        Invoke(nameof(ReloadScene), respawnDelay);
    }
    
    void ReloadScene()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    void UpdateUI()
    {
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHearts, maxHearts);
        }
    }
    
    public int GetCurrentHearts() => currentHearts;
    public int GetMaxHearts() => maxHearts;
    public bool IsDead() => isDead;
    
    // Heal (optional, for pickups or checkpoints)
    public void Heal(int amount = 1)
    {
        if (isDead) return;
        currentHearts = Mathf.Min(currentHearts + amount, maxHearts);
        UpdateUI();
    }
}

