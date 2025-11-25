using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Death Effects")]
    public GameObject deathEffectPrefab;
    public AudioClip deathSound;
    public float destroyDelay = 0.1f;
    
    private AudioSource audioSource;
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        Debug.Log($"{gameObject.name} took {damage} damage! Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log($"{gameObject.name} died!");
        
        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, transform.rotation);
            Destroy(effect, 2f);
        }
        
        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        
        // Destroy enemy
        Destroy(gameObject, destroyDelay);
    }
    
    public bool IsDead()
    {
        return isDead;
    }
}

