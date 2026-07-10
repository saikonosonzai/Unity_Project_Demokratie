using UnityEngine;

namespace AdventurePuzzleKit.ChessSystem
{
    // Allows interaction with chess-related objects (fuse piece or fuse box)
    public class ChessItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemType _itemType = ItemType.None; // Define item role: fuse or fuse box
        private enum ItemType { None, ChessFuse, Fusebox }

        // References to specific interactable components
        private ChessFuseCollectable _fuseCollectable;
        private ChessFuseBoxInteractable _fuseboxInteractable;

        private void Awake()
        {
            // Cache appropriate component based on item type
            switch (_itemType)
            {
                case ItemType.ChessFuse:
                    if (!TryGetComponent(out _fuseCollectable))
                    {
                        Debug.LogWarning($"Chess Item '{gameObject.name}' is set to Door but has no CPFuseCollectable attached.");
                    }
                    break;
                case ItemType.Fusebox:
                    if (!TryGetComponent(out _fuseboxInteractable))
                    {
                        Debug.LogWarning($"Chess Item Item '{gameObject.name}' is set to Key but has no CPFuseBoxInteractable attached.");
                    }
                    break;
            }
        }

        public void StartLooking() { } // Optional: called when player looks at the object

        public void StopInteraction() { } // Optional: called when interaction stops

        public void HandleInputClick()
        {
            // Trigger interaction based on item type
            switch (_itemType)
            {
                case ItemType.ChessFuse:
                    _fuseCollectable.PickupChessPiece();
                    break;
                case ItemType.Fusebox:
                    _fuseboxInteractable.InteractFuseBox();
                    break;
            }
        }

        public void HandleInputHold() { } // Optional: holding interaction

        public void HandleInputStop() { } // Optional: released interaction
    }
}
