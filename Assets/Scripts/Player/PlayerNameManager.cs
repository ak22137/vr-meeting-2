using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNameManager : MonoBehaviour
{
   [SerializeField] private GameObject namePanel;
   [SerializeField] private TMP_InputField nameInputField;
   [SerializeField] private Button confirmButton;

   public const string PlayerNameKey = "PlayerName";

   private void Start()
   {
       // Add listener to the confirm button
       confirmButton.onClick.AddListener(SavePlayerName);
       
       // Check if player name exists in PlayerPrefs
       CheckForPlayerName();
   }

   private void CheckForPlayerName()
   {
       string savedName = PlayerPrefs.GetString(PlayerNameKey, "");
       
       if (string.IsNullOrEmpty(savedName))
       {
           // Show the name panel if no name is saved
           namePanel.SetActive(true);
       }
       else
       {
           // Name already exists, hide the panel
           namePanel.SetActive(false);
       }
   }

   private void SavePlayerName()
   {
       string inputName = nameInputField.text.Trim();
       
       if (!string.IsNullOrEmpty(inputName))
       {
           // Save the name in PlayerPrefs
           PlayerPrefs.SetString(PlayerNameKey, inputName);
           PlayerPrefs.Save();
           
           // Hide the name panel
           namePanel.SetActive(false);
           
           Debug.Log("Player name saved: " + inputName);
       }
       else
       {
           // Optional: Show a warning that name cannot be empty
           Debug.Log("Name cannot be empty!");
       }
   }
   
   // Get the player name from PlayerPrefs
   public string GetPlayerName()
   {
       return PlayerPrefs.GetString(PlayerNameKey, "");
   }
}