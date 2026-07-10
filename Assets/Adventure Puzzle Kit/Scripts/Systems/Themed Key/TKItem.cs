using UnityEngine;

namespace AdventurePuzzleKit.ThemedKey
{
    public class TKItem : MonoBehaviour, IInteractable
    {
        [Header("Item Type")]
        [SerializeField] private ItemType _itemType = ItemType.None;
        private enum ItemType { None, Door, Key }

        private TKKeyCollectable keyController;
        private TKDoorInteractable doorController;

        private void Awake()
        {
            switch (_itemType)
            {
                case ItemType.Door:
                    if (!TryGetComponent(out doorController))
                    {
                        Debug.LogWarning($"Themed Key Item '{gameObject.name}' is set to Door but has no TKDoorInteractable attached.");
                    }
                    break;
                case ItemType.Key:
                    if (!TryGetComponent(out keyController))
                    {
                        Debug.LogWarning($"Themed Key Item '{gameObject.name}' is set to Key but has no TKKeyCollectable attached.");
                    }
                    break;
            }
        }

        public void StartLooking() { } //Started looking at the themed key object

        public void StopInteraction() { } //Stopped interacting with the themed key object

        public void HandleInputClick()
        {
            //Started interaction with the themed key object

            switch (_itemType)
            {
                case ItemType.Door: doorController.CheckDoor();
                    break;
                case ItemType.Key: keyController.KeyPickup();
                    break;
            }
        }

        public void HandleInputHold() { } //Holding interaction with the themed key object

        public void HandleInputStop() { } //Stopped interaction with the themed key object
    }
}
