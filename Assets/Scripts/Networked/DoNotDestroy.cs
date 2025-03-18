using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private static PersistentObject instance;

    private void Awake()
    {
        // Check if another instance already exists
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        // Set the instance to this object
        instance = this;

        // Ensure this GameObject is not destroyed when loading a new scene
        DontDestroyOnLoad(this.gameObject);
    }
}