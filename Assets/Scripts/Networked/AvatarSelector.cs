using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class AvatarSelector : MonoBehaviour
{
    [Header("Avatar Selection")]
    [SerializeField] private GameObject[] avatarPrefabs;
    [SerializeField] private Button[] avatarButtons;
    [SerializeField] private int defaultAvatarIndex = 0;
    
    [Header("UI References")]
    [SerializeField] private GameObject avatarSelectionPanel;
    [SerializeField] private Button confirmButton;
    
    private int selectedAvatarIndex = -1;
    
    private void Start()
    {
        // Set up avatar buttons
        for (int i = 0; i < avatarButtons.Length; i++)
        {
            int index = i; // Capture the index for the lambda
            avatarButtons[i].onClick.AddListener(() => HighlightAvatar(index));
        }
        
        // Disable confirm button until avatar is selected
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
            confirmButton.onClick.AddListener(ConfirmAvatarSelection);
        }
        
        // Just highlight the default avatar visually without selecting it
        if (PlayerPrefs.HasKey("SelectedAvatarIndex"))
        {
            int savedIndex = PlayerPrefs.GetInt("SelectedAvatarIndex", defaultAvatarIndex);
            if (savedIndex >= 0 && savedIndex < avatarPrefabs.Length)
            {
                HighlightAvatar(savedIndex);
            }
        }
    }
    
    public void HighlightAvatar(int index)
    {
        if (index < 0 || index >= avatarPrefabs.Length)
        {
            Debug.LogError("Avatar index out of range!");
            return;
        }
        
        selectedAvatarIndex = index;
        
        // Visual feedback for selection
        // foreach (Button button in avatarButtons)
        // {
        //     button.GetComponent<Image>().color = Color.white;
        // }
        
        // Highlight selected button
        // avatarButtons[index].GetComponent<Image>().color = Color.grey;
        
        // Enable confirm button now that an avatar is selected
        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }
    }
    
    public void ConfirmAvatarSelection()
    {
        if (selectedAvatarIndex < 0 || selectedAvatarIndex >= avatarPrefabs.Length)
        {
            Debug.LogError("No valid avatar selected!");
            return;
        }
        
        // Set the player prefab in the NetworkManager only when confirmed
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = avatarPrefabs[selectedAvatarIndex];
        
        // Save the selection
        PlayerPrefs.SetInt("SelectedAvatarIndex", selectedAvatarIndex);
        PlayerPrefs.Save();
        
        Debug.Log($"Avatar confirmed: {avatarPrefabs[selectedAvatarIndex].name}");
        
        // Hide avatar selection panel
        HideAvatarSelection();
    }
    
    public void HideAvatarSelection()
    {
        // avatarSelectionPanel.SetActive(false);
    }
}