using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Vivox;
using System.Collections.Generic;
public class RoomJoinOrCreate : NetworkBehaviour
{

    private VivoxChannelManager vivoxChannelManager;
    public Button joinOrCreateButton;
    public string targetSceneName = "YourTargetScene";
    private const string LOBBY_CODE_KEY = "LobbyCode";

    private async void Start()
    {
        // Initialize Unity Gaming Services
        await Unity.Services.Core.UnityServices.InitializeAsync();

        // Sign in anonymously
        await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
        if (vivoxChannelManager == null)
        {
            vivoxChannelManager = gameObject.AddComponent<VivoxChannelManager>();
        }
        joinOrCreateButton.onClick.AddListener(JoinOrCreateRoom);

        // vivoxVoiceManager = VivoxVoiceManager.Instance;
        // vivoxVoiceManager.Initialize();
    }

    // public async void JoinOrCreateRoom()
    // {
    //     Debug.Log("Starting Creating or joining room");
    //     try
    //     {
    //         // Check if a lobby exists by querying for lobbies
    //         var lobbies = await LobbyService.Instance.QueryLobbiesAsync();
    //         if (lobbies.Results.Count > 0)
    //         {
    //             // Join the first available lobby
    //             var lobby = lobbies.Results[0];
    //             Debug.Log("Joining existing lobby...");
    //             await JoinLobby(lobby.Id, lobby.Data[LOBBY_CODE_KEY].Value);
    //         }
    //         else
    //         {
    //             // No lobby exists, so create a new one
    //             Debug.Log("Creating new lobby...");
    //             await CreateLobby();
    //         }
    //     }
    //     catch (System.Exception e)
    //     {
    //         Debug.LogError($"Failed to join or create lobby: {e.Message}");
    //     }
    // }

    public async void JoinOrCreateRoom()
    {
        Debug.Log("Starting Creating or joining room");
        try
        {
            // Create a proper query with options to find all public lobbies
            QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
            {
                Count = 25, // Increase the number of results returned
                Filters = new List<QueryFilter>()
                // You can add filters if needed, but for now we'll search for all lobbies
            };

            var lobbies = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);

            Debug.Log($"Found {lobbies.Results.Count} lobbies");
            foreach (var lobby in lobbies.Results)
            {
                Debug.Log($"Found lobby: {lobby.Id} with {lobby.Players.Count} players");
            }

            if (lobbies.Results.Count > 0)
            {
                // Join the first available lobby
                var lobby = lobbies.Results[0];
                Debug.Log($"Joining existing lobby with ID: {lobby.Id}");

                // Check if the lobby has the required data
                if (lobby.Data != null && lobby.Data.ContainsKey(LOBBY_CODE_KEY))
                {
                    await JoinLobby(lobby.Id, lobby.Data[LOBBY_CODE_KEY].Value);
                }
                else
                {
                    Debug.LogError("Lobby found but missing relay code data");
                    await CreateLobby(); // Create a new lobby as fallback
                }
            }
            else
            {
                // No lobby exists, so create a new one
                Debug.Log("No lobbies found. Creating new lobby...");
                await CreateLobby();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to join or create lobby: {e.Message}");
        }
    }

    private async System.Threading.Tasks.Task CreateLobby()
    {
        try
        {
            // Create a Relay allocation
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

            // Get the Relay join code
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Create a lobby with the Relay join code as custom data
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Data = new System.Collections.Generic.Dictionary<string, DataObject>
                {
                    { LOBBY_CODE_KEY, new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
                }
            };

            var lobby = await LobbyService.Instance.CreateLobbyAsync("MyLobby", 4, options);
            Debug.Log($"Lobby created with ID: {lobby.Id}");

            vivoxChannelManager.SetupForLobby(lobby.Id);

            // Set up the Relay transport for the host
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // Start the host
            NetworkManager.Singleton.StartHost();

            // Load the scene from the host
            NetworkManager.Singleton.SceneManager.LoadScene(targetSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
        }
    }

    private async System.Threading.Tasks.Task JoinLobby(string lobbyId, string relayJoinCode)
    {
        try
        {
            vivoxChannelManager.SetupForLobby(lobbyId);
            // Join the Relay using the join code
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            // Set up the Relay transport for the client
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

            // Get player name 
            string playerName = "Player_" + System.Guid.NewGuid().ToString().Substring(0, 5);

            //  login to Vivox and join the channel
            vivoxChannelManager.LoginAndJoinChannel(playerName);


            // Start the client - the host will handle scene loading
            NetworkManager.Singleton.StartClient();

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
        }
    }
}