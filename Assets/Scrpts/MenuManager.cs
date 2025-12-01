using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void NewGame()
    {
        Time.timeScale = 1f;
        LoadScene("CharacterSelect");
    }
    
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
    
    public void NextLevel()
    {
        Time.timeScale = 1f;
        LoadScene("Level2");
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
