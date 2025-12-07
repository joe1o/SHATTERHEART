using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void NewGame()
    {
        Time.timeScale = 1f;
        PlayerPrefs.DeleteKey("LastLevelPlayed"); // Reset progress for new game
        LoadScene("CharacterSelect");
    }
    
    public void Continue()
    {
        Time.timeScale = 1f;
        string lastLevel = PlayerPrefs.GetString("LastLevelPlayed", "");
        
        if (string.IsNullOrEmpty(lastLevel))
        {
            // No saved progress, start new game
            LoadScene("CharacterSelect");
        }
        else
        {
            // Load the last played level
            LoadScene(lastLevel);
        }
    }
    
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
    
    public void NextLevel()
    {
        Time.timeScale = 1f;
        
        // Get current level number from the saved level name
        string currentLevel = PlayerPrefs.GetString("CurrentLevel", "Level1");
        
        // Extract level number and increment
        if (currentLevel == "Level1")
            LoadScene("Level2");
        else if (currentLevel == "Level2")
            LoadScene("Level3");
        else if (currentLevel == "Level3")
            LoadScene("End");
        // Add more levels as needed
        else
            LoadScene("MainMenu"); // Loop back to menu if no next level
    }
    
    public void ExitGame()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
