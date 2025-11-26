using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelect : MonoBehaviour
{
    [Header("Character Panels")]
    public GameObject[] characterPanels;  // Assign all 3 panels (Cyan, Green, Orange)
    
    private int currentIndex = 0;
    
    void Start()
    {
        // Show first character
        ShowCharacter(0);
    }
    
    void Update()
    {
        // Keyboard navigation
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            NextCharacter();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            PreviousCharacter();
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            SelectCharacter();
        }
    }
    
    public void NextCharacter()
    {
        currentIndex++;
        if (currentIndex >= characterPanels.Length)
        {
            currentIndex = 0; // Loop back to first
        }
        ShowCharacter(currentIndex);
    }
    
    public void PreviousCharacter()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = characterPanels.Length - 1; // Loop to last
        }
        ShowCharacter(currentIndex);
    }
    
    void ShowCharacter(int index)
    {
        // Hide all panels
        for (int i = 0; i < characterPanels.Length; i++)
        {
            characterPanels[i].SetActive(i == index);
        }
    }
    
    public void SelectCharacter()
    {
        // Store selected character
        SelectedCharacter.character = (CharacterType)currentIndex;
        
        // Load game scene
        SceneManager.LoadScene("lvl1");
    }
    
    // Call this from the SELECT button
    public void OnSelectButtonPressed()
    {
        SelectCharacter();
    }
    
    // Call this from the arrow buttons
    public void OnNextPressed()
    {
        NextCharacter();
    }
    
    public void OnPreviousPressed()
    {
        PreviousCharacter();
    }
}

