using UnityEngine;

namespace AdventurePuzzleKit.GasMaskSystem
{
    // Defines interaction logic for gas mask or filter pickup objects
    public class GasMaskItem : MonoBehaviour, IInteractable
    {
        // Type of item this represents: gas mask or filter
        private enum ItemType { None, GasMask, Filter }

        [SerializeField] private ItemType _itemType = ItemType.None; // Assigned via Inspector to control behavior

        public void StartLooking() { } // Called when the player starts looking at the item (e.g. show UI prompt)

        public void StopInteraction() { } // Called when the player looks away or cancels interaction


        public void HandleInputClick()
        {
            // Called when the player presses the interact button
            switch (_itemType)
            {
                case ItemType.GasMask: HandleGasMaskPickup(); break; // Process gas mask pickup
                case ItemType.Filter: HandleFilterPickup(); break; // Process filter pickup
            }
        }

        public void HandleInputHold() { } // Called while holding the interact button (not used here)


        public void HandleInputStop() { } // Called when the interact button is released (not used here)


        // Pick up a gas mask and disable the object
        private void HandleGasMaskPickup()
        {
            if (GasMaskController.instance != null)
            {
                GasMaskController.instance.PickupGasMask(); // Add mask to inventory
                gameObject.SetActive(false); // Remove from scene
            }
            else
            {
                Debug.LogWarning($"GasMaskController instance is missing. Cannot pickup GasMask on '{gameObject.name}'.");
            }
        }

        // Pick up a filter and disable the object
        private void HandleFilterPickup()
        {
            if (GasMaskController.instance != null)
            {
                GasMaskController.instance.PickupFilter(); // Add filter to inventory
                gameObject.SetActive(false); // Remove from scene
            }
            else
            {
                Debug.LogWarning($"GasMaskController instance is missing. Cannot pickup Filter on '{gameObject.name}'.");
            }
        }
    }
}