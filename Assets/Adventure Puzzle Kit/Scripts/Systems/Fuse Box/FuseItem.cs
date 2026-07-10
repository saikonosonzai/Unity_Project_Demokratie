/// REMEMBER: This script has a custom editor called "FuseItemEditor", found in the "Editor" folder. 
/// You will need to add new properties to this if you create new variables / fields in this script.

using UnityEngine;

namespace AdventurePuzzleKit.FuseSystem
{
    // Allows interaction with fuse pickup items or fuseboxes
    public class FuseItem : MonoBehaviour, IInteractable
    {
        // Defines the behaviour type of this object
        public enum ItemType { None, Fusebox, Fuse }

        [SerializeField] private ItemType _itemType = ItemType.None; // Set in inspector to define function

        [Tooltip("Sound Effect Scriptables - Only for the fuse object")]
        [SerializeField] private Sound pickupSound = null; // Optional sound for fuse pickup

        private FuseboxController _fuseboxController; // Cached reference if object is a fusebox

        private void Awake()
        {
            // If this is a fusebox, try to get the controller component
            if (_itemType == ItemType.Fusebox)
            {
                if (!TryGetComponent(out _fuseboxController))
                {
                    Debug.LogWarning($"FuseItem '{gameObject.name}' is set to Fusebox but has no FuseboxController attached.");
                }
            }
        }

        public void StartLooking() { } // Optional: called when player looks at the object

        public void StopInteraction() { } // Optional: called when interaction ends

        public void HandleInputClick()
        {
            // Main interaction entry point
            switch (_itemType)
            {
                case ItemType.Fusebox: HandleFuseboxInteraction(); break;
                case ItemType.Fuse: HandleFuseInteraction(); break;
            }
        }

        public void HandleInputHold() { } // Optional: holding interact key

        public void HandleInputStop() { } // Optional: released interact key

        // Handles logic for interacting with a fusebox
        private void HandleFuseboxInteraction()
        {
            if (_fuseboxController != null)
            {
                _fuseboxController.CheckFuseBox(); // Attempt to insert fuse
            }
            else if (DebugSettings.EnableDebugLogs)
            {
                Debug.LogWarning($"Attempted to interact with Fusebox on '{gameObject.name}', but no FuseboxController is assigned.");
            }
        }

        // Handles logic for picking up a fuse
        private void HandleFuseInteraction()
        {
            FuseInventory.instance.AddFuse(); // Add fuse to inventory

            if (pickupSound != null)
            {
                AKAudioManager.instance.Play(pickupSound); // Play pickup sound
            }
            else if (DebugSettings.EnableDebugLogs)
            {
                Debug.LogWarning($"Pickup sound is missing for FuseItem '{gameObject.name}'.");
            }

            gameObject.SetActive(false); // Disable the object after pickup
        }
    }
}
