using UnityEngine;

namespace AdventurePuzzleKit.KeycardSystem
{
    public class KeycardItem : MonoBehaviour, IInteractable
    {
        [Space(5)][SerializeField] private ItemType _itemType = ItemType.None;
        private enum ItemType { None, Scanner, Keycard }

        private KeycardCollectable keyController;
        private KeycardScannerInteractable scannerController;

        private void Awake()
        {
            switch (_itemType)
            {
                case ItemType.Keycard: keyController = GetComponent<KeycardCollectable>();
                    break;
                case ItemType.Scanner: scannerController = GetComponent<KeycardScannerInteractable>();
                    break;
            }
        }

        public void StartLooking() { } //Started looking at the Keycard object

        public void StopInteraction() { } //Stopped interacting with the Keycard object

        public void HandleInputClick()
        {
            //Started interaction with the Keycard object

            switch (_itemType)
            {
                case ItemType.Keycard:
                    keyController.KeyPickup();
                    break;
                case ItemType.Scanner:
                    scannerController.CheckScanner();
                    break;
            }
        }

        public void HandleInputHold() { } //Holding interaction with the Keycard object

        public void HandleInputStop() { } //Stopped interaction with the Keycard object
    }
}
