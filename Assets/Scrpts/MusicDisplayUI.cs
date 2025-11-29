using UnityEngine;
using TMPro;

public class MusicDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI musicText;
    
    void Update()
    {
        if (MusicManager.Instance != null && musicText != null)
        {
            string trackName = MusicManager.Instance.GetCurrentTrackName();
            musicText.text = trackName;
        }
        else if (musicText != null)
        {
            musicText.text = "No music playing";
        }
    }
}