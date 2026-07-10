using AdventurePuzzleKit.FlashlightSystem;
using AdventurePuzzleKit.KeypadSystem;
using AdventurePuzzleKit.NoteSystem;
using AdventurePuzzleKit.PadlockSystem;
using AdventurePuzzleKit.PhoneSystem;
using AdventurePuzzleKit.SafeSystem;
using UnityEngine;

namespace AdventurePuzzleKit
{
    public class AKMasterTrigger : MonoBehaviour
    {
        // Different types of interactions this trigger can handle
        public enum TriggerType
        {
            None,
            Padlock,
            Note,
            Safe,
            Keypad,
            Flashlight,
            Phone
        }

        [Header("Trigger Settings")]
        [SerializeField] private TriggerType triggerType = TriggerType.None; // What this trigger should activate
        [SerializeField] private bool disableAfterUse = false; // Should the trigger disable after one use?

        [SerializeField] private MonoBehaviour linkedObject = null; // Optional reference to the target object or controller

        [SerializeField] private FlashlightItem.ItemType flashlightItemType = FlashlightItem.ItemType.None; // Type of flashlight item
        [SerializeField] private int batteryNumber = 1; // How many batteries to collect (if applicable)

        [Space(10)]
        [SerializeField] private string playerTag = "Player"; // The tag used to identify the player

        private bool canUse; // Tracks whether the player is within range
        private NoteTypeSelector noteComponent; // Cached reference for note display

        private void Start()
        {
            // If this is a note trigger, cache the NoteTypeSelector
            if (triggerType == TriggerType.Note && linkedObject != null)
            {
                noteComponent = linkedObject.GetComponent<NoteTypeSelector>();

                if (noteComponent == null)
                {
                    Debug.LogWarning($"No NoteTypeSelector found on {linkedObject.name}");
                }
            }
        }

        private void Update()
        {
            // Check for player interaction input each frame
            HandleInput();
        }

        private void HandleInput()
        {
            // Only proceed if the player is in range and presses the interact key
            if (canUse && Input.GetKeyDown(AKInputManager.instance.triggerInteractKey))
            {
                switch (triggerType)
                {
                    case TriggerType.Padlock:
                        if (linkedObject is PadlockController padlockController)
                        {
                            padlockController.ShowPadlock();
                        }
                        break;

                    case TriggerType.Note:
                        if (noteComponent != null)
                        {
                            noteComponent.DisplayNotes();
                        }
                        break;

                    case TriggerType.Safe:
                        if (linkedObject is SafeItem safeItem)
                        {
                            safeItem.ShowSafeLock();
                        }
                        break;

                    case TriggerType.Keypad:
                        if (linkedObject is KeypadItem keypadItem)
                        {
                            keypadItem.ShowKeypad();
                        }
                        break;

                    case TriggerType.Flashlight:
                        HandleFlashlightPickup();
                        break;

                    case TriggerType.Phone:
                        if (linkedObject is PhoneItem phoneItem)
                        {
                            phoneItem.ShowKeypad();
                        }
                        break;
                }

                // Optionally disable this trigger after one use
                if (disableAfterUse)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // When the player enters the trigger area
            if (other.CompareTag(playerTag))
            {
                canUse = true;
                AKUIManager.instance.EnableInteractPrompt(true); // Show interaction prompt
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // When the player leaves the trigger area
            if (other.CompareTag(playerTag))
            {
                canUse = false;
                AKUIManager.instance.EnableInteractPrompt(false); // Hide interaction prompt
            }
        }

        private void HandleFlashlightPickup()
        {
            // Special handling for flashlight or battery pickups (no linked object required)
            switch (flashlightItemType)
            {
                case FlashlightItem.ItemType.Flashlight:
                    FlashlightController.instance.CollectFlashlight();                // Give player the flashlight
                    FlashlightController.instance.CollectBattery(batteryNumber);      // Add batteries too
                    break;

                case FlashlightItem.ItemType.Battery:
                    FlashlightController.instance.CollectBattery(batteryNumber);      // Just give batteries
                    break;
            }

            AKUIManager.instance.EnableInteractPrompt(false); // Hide prompt after pickup
        }
    }
}
