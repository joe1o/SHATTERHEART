using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

public class Baloon : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float bounceStrength = 15f;
    [SerializeField] private GameObject popVFX;
    public AudioClip popSound;
    [Range(0f, 1f)] public float Volume = 0.6f;
    private AudioSource audioSource;
    private PlayerAbilities abilities;
    public float FBounce = 2;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Get player movement script
            abilities = other.GetComponent<PlayerAbilities>();
           //abilities.setDashing(false);
            abilities.EndDash();
            FirstPersonController pm = other.GetComponent<FirstPersonController>();

            if (pm != null)
            {
                // In your Balloon script, before applying the boost:

                if (abilities != null && abilities.IsFireballing())
                {
                    abilities.EndFireball();
                }
                
                pm.Bounce(bounceStrength);
                
            }

            // Spawn pop effect if assigned
            if (popSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(popSound, Volume);
            }

            Destroy(gameObject);
        }
    }

}
