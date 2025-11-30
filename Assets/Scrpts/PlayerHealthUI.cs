using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Heart GameObjects")]
    public GameObject[] hearts = new GameObject[3]; // Drag your 3 heart GameObjects here
    
    public void UpdateHearts(int currentHearts, int maxHearts)
    {
        if (hearts == null) return;
        
        // Destroy hearts from right to left (last heart first)
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            
            if (i < currentHearts)
            {
                // Keep the heart if we still have health
                if (!hearts[i].activeSelf)
                {
                    hearts[i].SetActive(true);
                }
            }
            else
            {
                // Destroy the heart GameObject when health is lost
                if (hearts[i].activeSelf)
                {
                    Destroy(hearts[i]);
                }
            }
        }
    }
}
