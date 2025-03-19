using Unity.Netcode;
using UnityEngine;

public class CustomNetworkManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs;

    public void StartHost()
    {
        // Get the selected avatar index from PlayerPrefs
        int selectedAvatarIndex = PlayerPrefs.GetInt("SelectedAvatarIndex", 0);

        // Set the player prefab before starting the host
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = playerPrefabs[selectedAvatarIndex];

        // Start host
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        // Get the selected avatar index from PlayerPrefs
        int selectedAvatarIndex = PlayerPrefs.GetInt("SelectedAvatarIndex", 0);

        // Set the player prefab before starting the client
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = playerPrefabs[selectedAvatarIndex];

        // Start client
        NetworkManager.Singleton.StartClient();
    }

    public void StartServer()
    {
        // For a dedicated server, you might not need to set the player prefab
        NetworkManager.Singleton.StartServer();
    }
}