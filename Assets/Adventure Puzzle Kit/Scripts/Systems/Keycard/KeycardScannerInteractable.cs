using UnityEngine;
using UnityEngine.Events;

namespace AdventurePuzzleKit.KeycardSystem
{
    // Handles interaction with a keycard scanner in the game
    public class KeycardScannerInteractable : MonoBehaviour
    {
        // Configuration for the keycard required to unlock the scanner
        [Header("Key ScriptableObject")]
        [SerializeField] private bool removeKeycardAfterUse = false; // Determines if the keycard is removed from inventory after use
        [SerializeField] private Keycard keyScriptable = null; // Reference to the specific keycard ScriptableObject required

        // Audio clips for scanner feedback
        [Header("Scanner - Audio Clips")]
        [SerializeField] private Sound deniedScannerSound = null; // Sound played when access is denied
        [SerializeField] private Sound acceptedScannerSound = null; // Sound played when access is granted

        // Event triggered when the scanner is successfully unlocked
        [Space(10)][SerializeField] private UnityEvent onUnlock = null;

        // Checks if the player has the required keycard and processes the interaction
        public void CheckScanner()
        {
            // Verify if the required keycard is in the player's inventory
            if (KeycardInventory.instance.Keycards.Contains(keyScriptable))
            {
                if (removeKeycardAfterUse)
                {
                    KeycardInventory.instance.RemoveKey(keyScriptable); // Remove keycard from inventory if configured
                }

                ScannerInteraction(); // Proceed with successful scanner interaction
            }
            else
            {
                DeniedScannerSound(); // Play denied sound if keycard is missing
            }
        }

        // Handles successful scanner interaction
        private void ScannerInteraction()
        {
            gameObject.tag = "Untagged"; // Remove tag to prevent further interactions
            onUnlock.Invoke(); // Trigger the unlock event (e.g., open door, activate object)
            AcceptedScannerSound(); // Play accepted sound
        }

        // Plays the denied access sound
        void DeniedScannerSound()
        {
            AKAudioManager.instance.Play(deniedScannerSound);
        }

        // Plays the accepted access sound
        void AcceptedScannerSound()
        {
            AKAudioManager.instance.Play(acceptedScannerSound);
        }
    }
}