using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarSelector : MonoBehaviour
{
    [SerializeField] private PlayerAvatarManager avatarManager; // This reference might be null
    [SerializeField] private Button[] avatarButtons;
    [SerializeField] private GameObject avatarSelectionPanel;
    [SerializeField] private Button confirmButton;
    
    private int selectedAvatarIndex = -1;
    
    private void Start()
    {
        // Set up avatar buttons
        for (int i = 0; i < avatarButtons.Length; i++)
        {
            int index = i;
            avatarButtons[i].onClick.AddListener(() => HighlightAvatar(index));
        }
        
        // Disable confirm button until avatar is selected
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
            confirmButton.onClick.AddListener(ConfirmAvatarSelection);
        }
        
        // Load previous selection
        if (PlayerPrefs.HasKey("SelectedAvatarIndex"))
        {
            int savedIndex = PlayerPrefs.GetInt("SelectedAvatarIndex", 0);
            HighlightAvatar(savedIndex);
        }
    }
    
    public void HighlightAvatar(int index)
    {
        selectedAvatarIndex = index;
        
        // Visual feedback
        foreach (Button button in avatarButtons)
        {
            button.GetComponent<Image>().color = Color.white;
        }
        
        avatarButtons[index].GetComponent<Image>().color = Color.green;
        
        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }
    }
    
    public void ConfirmAvatarSelection()
    {
        if (selectedAvatarIndex >= 0)
        {
            // This line is likely causing the error if avatarManager is null
            if (avatarManager != null) // Add this null check
            {
                avatarManager.SelectAvatar(selectedAvatarIndex);
            }
            else
            {
                // Just save locally if avatarManager is not available
                PlayerPrefs.SetInt("SelectedAvatarIndex", selectedAvatarIndex);
                PlayerPrefs.Save();
                Debug.LogWarning("AvatarManager reference is missing. Avatar selection saved locally only.");
            }
            
            HideAvatarSelection();
        }
    }
    
    public void HideAvatarSelection()
    {
        avatarSelectionPanel.SetActive(false);
    }
}