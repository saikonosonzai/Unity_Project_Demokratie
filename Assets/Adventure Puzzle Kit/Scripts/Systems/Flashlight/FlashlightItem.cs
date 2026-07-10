using UnityEngine;

namespace AdventurePuzzleKit.FlashlightSystem
{
    // Handles player interaction with flashlight or battery pickup
    public class FlashlightItem : MonoBehaviour, IInteractable
    {
        public enum ItemType
        {
            None,
            Battery,
            Flashlight
        }

        [Header("Flashlight / Battery Type")]
        [SerializeField] private ItemType _itemType = ItemType.None; // Defines whether this is a flashlight or battery

        [Header("Batteries Added On Pickup")]
        [SerializeField] private int batteryNumber = 1; // Amount of batteries granted on pickup

        // Property to get item type
        public ItemType FlashlightItemType => _itemType;

        // Property to get battery amount
        public int BatteryNumber => batteryNumber;

        public void StartLooking() { } // Optional: called when player looks at item

        public void StopInteraction() { } // Optional: called when player stops interaction

        public void HandleInputClick()
        {
            // Perform pickup action based on item type
            switch (_itemType)
            {
                case ItemType.Flashlight:
                    FlashlightController.instance.CollectFlashlight();
                    gameObject.SetActive(false); // Disable after pickup
                    break;
                case ItemType.Battery:
                    FlashlightController.instance.CollectBattery(batteryNumber);
                    gameObject.SetActive(false); // Disable after pickup
                    break;
            }
        }

        public void HandleInputHold() { } // Optional: holding interact key

        public void HandleInputStop() { } // Optional: released interact key
    }
}
