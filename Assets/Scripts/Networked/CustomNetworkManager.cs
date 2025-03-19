using Unity.Netcode;
using UnityEngine;

public class CustomNetworkManager : MonoBehaviour
{
    [SerializeField] private PlayerAvatarManager avatarManager;
    
    private void Start()
    {
        // Add event listener for server started
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        
        // Add event listener for client connected
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    
    private void OnDestroy()
    {
        // Remove event listeners when destroyed
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
    
    private void OnServerStarted()
    {
        Debug.Log("Server started");
    }
    
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
        
        // If we're the server, spawn the correct player prefab for this client
        if (NetworkManager.Singleton.IsServer)
        {
            // Get the selected avatar for this client
            int avatarIndex = PlayerPrefs.GetInt("SelectedAvatarIndex", 0);
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                // For remote clients, we need to get their selection from network message
                // This will be implemented in PlayerAvatarManager
                avatarIndex = avatarManager.GetAvatarIndexForClient(clientId);
            }
            
            // Get the prefab and spawn it
            GameObject playerPrefab = avatarManager.GetAvatarPrefabByIndex(avatarIndex);
            Vector3 spawnPos = new Vector3(0, 1, 0); // Default spawn position
            GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            
            // Get the NetworkObject and spawn it
            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);
        }
    }
    
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
}