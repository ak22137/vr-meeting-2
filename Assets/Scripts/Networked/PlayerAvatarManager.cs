using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerAvatarManager : MonoBehaviour
{
    [SerializeField] private GameObject[] avatarPrefabs;
    [SerializeField] private GameObject defaultPlayerPrefab;
    
    private Dictionary<ulong, int> playerAvatarSelections = new Dictionary<ulong, int>();
    
    private void Start()
    {
        // Register all avatar prefabs with the NetworkManager
        RegisterNetworkPrefabs();
        
        // Register for the client connected event to synchronize avatar selections
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
    
    private void RegisterNetworkPrefabs()
    {
        // Set the default player prefab
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = defaultPlayerPrefab;
        
        // Make sure all avatar prefabs are registered
        foreach (GameObject prefab in avatarPrefabs)
        {
            // Ensure the prefab has a NetworkObject component
            if (prefab.GetComponent<NetworkObject>() == null)
            {
                Debug.LogError($"Avatar prefab {prefab.name} does not have a NetworkObject component!");
                continue;
            }
        }
    }
    
    private void OnClientConnected(ulong clientId)
    {
        // When a client connects, send them our avatar selection
        if (NetworkManager.Singleton.IsClient && clientId == NetworkManager.Singleton.LocalClientId)
        {
            int avatarIndex = PlayerPrefs.GetInt("SelectedAvatarIndex", 0);
            SendAvatarSelectionServerRpc(clientId, avatarIndex);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void SendAvatarSelectionServerRpc(ulong clientId, int avatarIndex)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Update the server's record of this client's avatar
            playerAvatarSelections[clientId] = avatarIndex;
            
            // Broadcast to all clients
            SyncAvatarSelectionsClientRpc(clientId, avatarIndex);
        }
    }
    
    [ClientRpc]
    public void SyncAvatarSelectionsClientRpc(ulong clientId, int avatarIndex)
    {
        // Update the client's record of this player's avatar
        playerAvatarSelections[clientId] = avatarIndex;
    }
    
    public int GetAvatarIndexForClient(ulong clientId)
    {
        if (playerAvatarSelections.TryGetValue(clientId, out int avatarIndex))
        {
            return avatarIndex;
        }
        return 0; // Default avatar index
    }
    
    public GameObject GetAvatarPrefabByIndex(int index)
    {
        if (index >= 0 && index < avatarPrefabs.Length)
        {
            return avatarPrefabs[index];
        }
        return defaultPlayerPrefab;
    }
    
    // Called by UI to select an avatar
    public void SelectAvatar(int avatarIndex)
    {
        if (avatarIndex < 0 || avatarIndex >= avatarPrefabs.Length)
        {
            Debug.LogError("Avatar index out of range!");
            return;
        }
        
        // Save the selection locally
        PlayerPrefs.SetInt("SelectedAvatarIndex", avatarIndex);
        PlayerPrefs.Save();
        
        // If we're connected to a server, inform it of our selection
        if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient)
        {
            ulong localClientId = NetworkManager.Singleton.LocalClientId;
            SendAvatarSelectionServerRpc(localClientId, avatarIndex);
        }
    }
}