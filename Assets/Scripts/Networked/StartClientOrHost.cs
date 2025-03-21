using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
   [SerializeField] private Button hostButton;
   [SerializeField] private Button clientButton;

   private void Start()
   {
       // Add button listeners if buttons are assigned
       if (hostButton != null)
           hostButton.onClick.AddListener(StartHost);
           
       if (clientButton != null)
           clientButton.onClick.AddListener(StartClient);
   }

   public void StartHost()
   {
       NetworkManager.Singleton.StartHost();
       Debug.Log("Started as Host");
   }

   public void StartClient()
   {
       NetworkManager.Singleton.StartClient();
       Debug.Log("Started as Client");
   }

   // Optional: Handle connection callbacks
   private void OnEnable()
   {
       if (NetworkManager.Singleton != null)
       {
           NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
           NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
       }
   }

   private void OnDisable() 
   {
       if (NetworkManager.Singleton != null)
       {
           NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
           NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
       }
   }

   private void OnClientConnected(ulong clientId)
   {
       Debug.Log($"Client connected: {clientId}");
   }

   private void OnClientDisconnected(ulong clientId)
   {
       Debug.Log($"Client disconnected: {clientId}");
   }
}