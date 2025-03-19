using Unity.Netcode;
using UnityEngine;

public class CustomNetworkManager : MonoBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs;
    public NetworkManager NetworkManager;
    private void Awake()
    {
        // Initialize with default player prefab if none is set
        if (NetworkManager.Singleton.NetworkConfig.PlayerPrefab == null && playerPrefabs.Length > 0)
        {
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = playerPrefabs[0];
        }
    }
    
    public void StartHost()
    {
        // PlayerPrefab should already be set by AvatarSelector
        NetworkManager.Singleton.StartHost();
    }
    
    public void StartClient()
    {
        // PlayerPrefab should already be set by AvatarSelector
        NetworkManager.Singleton.StartClient();
    }
    
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
}