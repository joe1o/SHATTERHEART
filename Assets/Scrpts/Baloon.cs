using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baloon : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float bounceStrength = 15f;
    [SerializeField] private GameObject popVFX;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Get player movement script
            FirstPersonController pm = other.GetComponent<FirstPersonController>();

            if (pm != null)
            {
                pm.Bounce(bounceStrength);
            }

            // Spawn pop effect if assigned
            if (popVFX != null)
            {
                Instantiate(popVFX, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }

}
