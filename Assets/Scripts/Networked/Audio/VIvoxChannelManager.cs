using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Vivox;
using UnityEngine;

public class VivoxChannelManager : NetworkBehaviour
{
    private const string VIVOX_CHANNEL_KEY = "VivoxChannel";
    private bool isChannelJoined = false;
    
    private void Start()
    {
        // Make sure this runs after VivoxVoiceManager is initialized
        if (VivoxVoiceManager.Instance != null)
        {
            // Subscribe to events
            VivoxService.Instance.LoggedIn += OnVivoxLoggedIn;
            VivoxService.Instance.LoggedOut += OnVivoxLoggedOut;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (VivoxService.Instance != null)
        {
            VivoxService.Instance.LoggedIn -= OnVivoxLoggedIn;
            VivoxService.Instance.LoggedOut -= OnVivoxLoggedOut;
        }
    }
    
    private void OnVivoxLoggedIn()
    {
        Debug.Log("Logged in to Vivox");
    }
    
    private void OnVivoxLoggedOut()
    {
        Debug.Log("Logged out from Vivox");
        isChannelJoined = false;
    }
    
    public void SetupForLobby(string lobbyId)
    {
        // Generate a unique channel name based on the lobby ID
        string channelName = "game_" + lobbyId.Replace("-", "").Substring(0, 10);
        
        // Store this for later use
        PlayerPrefs.SetString(VIVOX_CHANNEL_KEY, channelName);
    }
    
    public async void LoginAndJoinChannel(string playerName)
    {
        if (VivoxService.Instance == null)
        {
            Debug.LogError("Vivox service not initialized");
            return;
        }
        
        try
        {
            // Initialize Vivox with player name
            await VivoxVoiceManager.Instance.InitializeAsync(playerName);
            
            // Login to Vivox
            var loginOptions = new LoginOptions()
            {
                DisplayName = playerName,
                ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.FivePerSecond
            };
            
            await VivoxService.Instance.LoginAsync(loginOptions);
            
            // Join the channel stored in PlayerPrefs
            string channelName = PlayerPrefs.GetString(VIVOX_CHANNEL_KEY, "default");
            await JoinGameChannel(channelName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error logging in to Vivox: {e.Message}");
        }
    }
    
    private async System.Threading.Tasks.Task JoinGameChannel(string channelName)
    {
        if (isChannelJoined)
        {
            return;
        }
        
        try
        {
            await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.TextAndAudio);
            isChannelJoined = true;
            Debug.Log($"Joined Vivox channel: {channelName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to join Vivox channel: {e.Message}");
        }
    }
    
    public void LeaveChannel()
    {
        if (VivoxService.Instance != null && isChannelJoined)
        {
            var channels = VivoxService.Instance.ActiveChannels;
            foreach (var channel in channels)
            {
                VivoxService.Instance.LeaveChannelAsync(channel.Key);
            }
            isChannelJoined = false;
        }
    }
}