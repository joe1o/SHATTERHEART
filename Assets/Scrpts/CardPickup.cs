using UnityEngine;

public class CardPickup : MonoBehaviour
{
    [Header("Card Settings")]
    public CardType cardType;
    public bool respawns = false;
    public float respawnTime = 5f;
    
    [Header("Visual")]
    public GameObject cardVisual;       // The card model/sprite
    public float rotationSpeed = 50f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;
    
    [Header("Effects")]
    public GameObject pickupEffectPrefab;
    public AudioClip pickupSound;
    
    private Vector3 startPosition;
    private bool isPickedUp = false;
    private float respawnTimer = 0f;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        if (!isPickedUp)
        {
            // Rotate the card
            if (cardVisual != null)
            {
                cardVisual.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            }
            
            // Bob up and down
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        else if (respawns)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0)
            {
                Respawn();
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;
        
        if (other.CompareTag("Player"))
        {
            TryPickup();
        }
    }
    
    void TryPickup()
    {
        if (CardManager.Instance == null) return;
        
        if (CardManager.Instance.CanPickupCard(cardType))
        {
            CardManager.Instance.PickupCard(cardType);
            OnPickedUp();
        }
        else
        {
            // Can't pickup - inventory full or too many of this type
            // Could show feedback here
        }
    }
    
    void OnPickedUp()
    {
        isPickedUp = true;
        
        // Spawn pickup effect
        if (pickupEffectPrefab != null)
        {
            GameObject effect = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Hide the card
        if (cardVisual != null)
        {
            cardVisual.SetActive(false);
        }
        else
        {
            // Hide all renderers
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }
        }
        
        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        if (respawns)
        {
            respawnTimer = respawnTime;
        }
        else
        {
            Destroy(gameObject, 0.1f);
        }
    }
    
    void Respawn()
    {
        isPickedUp = false;
        
        // Show the card
        if (cardVisual != null)
        {
            cardVisual.SetActive(true);
        }
        else
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = true;
            }
        }
        
        // Enable collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
        
        transform.position = startPosition;
    }
}

