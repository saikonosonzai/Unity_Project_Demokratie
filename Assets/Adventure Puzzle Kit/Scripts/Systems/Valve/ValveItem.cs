using UnityEngine;

namespace AdventurePuzzleKit.ValveSystem
{
    public class ValveItem : MonoBehaviour, IInteractable
    {
        public enum ItemType { None, Valve, Slot, Wheel }
        [SerializeField] private ItemType _itemType = ItemType.None;

        private ValveWheelInteractable _wheelInteractable;
        private ValveCollectable _valveCollectable;
        private ValveSlot _valveSlot;

        private void Awake()
        {
            switch (_itemType)
            {
                case ItemType.Valve:
                    if (!TryGetComponent(out _valveCollectable))
                        LogMissingComponent("ValveCollectable");
                    break;

                case ItemType.Slot:
                    if (!TryGetComponent(out _valveSlot))
                        LogMissingComponent("ValveSlot");
                    break;

                case ItemType.Wheel:
                    if (!TryGetComponent(out _wheelInteractable))
                        LogMissingComponent("ValveWheelInteractable");
                    break;
            }
        }

        private void LogMissingComponent(string componentName)
        {
            Debug.LogWarning($"ValveItem '{gameObject.name}' is set to {_itemType} but is missing a {componentName} component.");
        }

        public void StartLooking() { } //Started looking at valve object

        public void StopInteraction() //Stopped interacting with valve object
        {
            if (_itemType == ItemType.Wheel)
            {
                StopWheelInteraction();
            }
        }

        public void HandleInputClick() //Click interaction started on valve object
        {
            switch (_itemType)
            {
                case ItemType.Valve:
                    HandleValvePickup();
                    break;

                case ItemType.Slot:
                    HandleSlotCheck();
                    break;
            }
        }

        public void HandleInputHold() //Hold interaction started on valve object
        {
            if (_itemType == ItemType.Wheel)
            {
                StartWheelInteraction();
            }
        }

        public void HandleInputStop() //Stop interaction on valve object
        {
            if (_itemType == ItemType.Wheel)
            {
                StopWheelInteraction();
            }
        }

        // Separate Methods for Subsystem Logic
        private void HandleValvePickup()
        {
            if (_valveCollectable != null)
            {
                _valveCollectable.ValvePickup();
            }
            else
            {
                Debug.LogWarning($"ValveCollectable is missing on '{gameObject.name}' for Valve interaction.");
            }
        }

        private void HandleSlotCheck()
        {
            if (_valveSlot != null)
            {
                _valveSlot.CheckValveSlot();
            }
            else
            {
                Debug.LogWarning($"ValveSlot is missing on '{gameObject.name}' for Slot interaction.");
            }
        }

        private void StartWheelInteraction()
        {
            if (_wheelInteractable != null)
            {
                _wheelInteractable.StartTurning();
            }
            else
            {
                Debug.LogWarning($"ValveWheelInteractable is missing on '{gameObject.name}' for Wheel interaction.");
            }
        }

        private void StopWheelInteraction()
        {
            if (_wheelInteractable != null)
            {
                _wheelInteractable.StopTurning();
            }
            else
            {
                Debug.LogWarning($"ValveWheelInteractable is missing on '{gameObject.name}' for Wheel interaction.");
            }
        }
    }
}
