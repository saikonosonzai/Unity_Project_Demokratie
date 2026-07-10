using System.Collections.Generic;
using UnityEngine;

namespace AdventurePuzzleKit.KeycardSystem
{
    // Manages the player's keycard inventory in the game
    public class KeycardInventory : MonoBehaviour
    {
        // List of keycards held by the player
        [Header("Inventory Keylist")]
        [SerializeField] private List<Keycard> _keycardsList = new List<Keycard>();

        // Determines if the inventory persists across scene changes
        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true;

        // Public read-only access to the keycard list
        public List<Keycard> Keycards
        {
            get { return _keycardsList; }
        }

        // Singleton instance of the KeycardInventory
        public static KeycardInventory instance;

        void Awake()
        {
            // Ensure only one instance of KeycardInventory exists
            if (instance != null)
            {
                Destroy(gameObject); // Destroy duplicate instances
            }
            else
            {
                instance = this; // Set this as the singleton instance
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject); // Prevent destruction when loading new scenes
                }
            }
        }

        // Adds a keycard to the inventory if not already present
        public void AddKey(Keycard key)
        {
            if (!_keycardsList.Contains(key))
            {
                _keycardsList.Add(key); // Add the keycard to the list
                AKUIManager.instance.KeycardCollected(); // Notify UI of keycard collection
                AKUIManager.instance.FillKeycardInventorySlot(); // Update UI to reflect new keycard
            }
        }

        // Removes a keycard from the inventory if present
        public void RemoveKey(Keycard key)
        {
            if (_keycardsList.Contains(key))
            {
                int currentCount = _keycardsList.Count; // Store current count for UI reset
                _keycardsList.Remove(key); // Remove the keycard from the list
                AKUIManager.instance.ResetKeycardInventorySlot(currentCount); // Update UI to reflect removal
            }
        }
    }
}