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
    
    private int selectedAvatarIndex;
    private NetworkManager networkManager;
    
    private void Start()
    {
        networkManager = NetworkManager.Singleton;
        
        if (networkManager == null)
        {
            Debug.LogError("Network Manager not found!");
            return;
        }
        
        // Set default avatar
        selectedAvatarIndex = defaultAvatarIndex;
        
        // Set up avatar buttons
        for (int i = 0; i < avatarButtons.Length; i++)
        {
            int index = i; // Capture the index for the lambda
            avatarButtons[i].onClick.AddListener(() => SelectAvatar(index));
        }
        
        // Load previously selected avatar if it exists
        if (PlayerPrefs.HasKey("SelectedAvatarIndex"))
        {
            int savedIndex = PlayerPrefs.GetInt("SelectedAvatarIndex", defaultAvatarIndex);
            if (savedIndex >= 0 && savedIndex < avatarPrefabs.Length)
            {
                SelectAvatar(savedIndex);
            }
        }
    }
    
    public void SelectAvatar(int index)
    {
        if (index < 0 || index >= avatarPrefabs.Length)
        {
            Debug.LogError("Avatar index out of range!");
            return;
        }
        
        selectedAvatarIndex = index;
        
        // For Netcode for GameObjects, we modify the NetworkPrefab instead of playerPrefab
        SetNetworkPrefab(avatarPrefabs[selectedAvatarIndex]);
        
        // Visual feedback for selection
        HighlightSelectedButton(index);
        
        PlayerPrefs.SetInt("SelectedAvatarIndex", selectedAvatarIndex);
        PlayerPrefs.Save();
    }
    
    private void SetNetworkPrefab(GameObject prefab)
    {
        // In Netcode for GameObjects, we typically use NetworkPrefab list
        // This is a more complex approach since we need to modify the NetworkConfig
        
        // Use NetworkPrefabHandler to override the player prefab
        // This is one approach - might need to be adjusted based on your specific setup
        NetworkConfig config = networkManager.NetworkConfig;
        
        // Store the prefab reference for later use when joining/hosting
        PlayerPrefs.SetString("SelectedAvatarPrefabPath", prefab.name);
        PlayerPrefs.Save();
        
        // Note: You'll need to have your NetworkManager handle this prefab selection
        // when starting the connection
    }
    
    private void HighlightSelectedButton(int index)
    {
        // Reset all buttons
        foreach (Button button in avatarButtons)
        {
            button.GetComponent<Image>().color = Color.white;
        }
        
        // Highlight selected button
        avatarButtons[index].GetComponent<Image>().color = Color.green;
    }
    
    public void HideAvatarSelection()
    {
        avatarSelectionPanel.SetActive(false);
    }
}