using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace AdventurePuzzleKit.KeypadSystem
{
    // Serializable class to store a keypad code and its associated event
    [System.Serializable]
    public class KeypadCodes
    {
        public string keypadCode; // The code required to trigger the event
        [Space(10)]
        public UnityEvent keypadEvent; // Event triggered when the code is entered correctly
    }

    // Manages the keypad system, handling user input, code validation, and UI/audio feedback
    public class KeypadController : MonoBehaviour
    {
        // Defines the visual style of the keypad
        [Header("Keypad Type")]
        [SerializeField] private KeypadType _keypadType = KeypadType.None;
        private enum KeypadType { None, Modern, Scifi, Keyboard }; // Enum for different keypad styles

        // Maximum number of characters allowed in the input field
        [Header("Character Limit")]
        [SerializeField] private int _inputLimit = 10;

        // List of valid codes and their associated events
        [Header("Code List")]
        [SerializeField] private KeypadCodes[] keypadCodesList = null;

        // Audio clips for keypad interactions
        [Header("Keypad Sounds")]
        [SerializeField] private Sound keypadBeep = null; // Sound played for each key press
        [SerializeField] private Sound keypadDenied = null; // Sound played when an invalid code is entered

        // Configuration for trigger-based keypads
        [Header("Trigger Event")]
        [SerializeField] private bool isTriggerEvent = false; // If true, keypad is tied to a trigger object
        [SerializeField] private GameObject triggerObject = null; // Object that triggers the keypad interaction
        private bool isOpen = false; // Tracks if the keypad UI is currently open

        // Public property for accessing/modifying the input character limit
        public int inputLimit
        {
            get { return _inputLimit; }
            set { _inputLimit = value; }
        }

        private void Update()
        {
            // Close the keypad if it's open and the close key is pressed
            if (isOpen && Input.GetKeyDown(AKInputManager.instance.closeKeypadKey))
            {
                CloseKeypad();
            }
        }

        public void ShowKeypad()
        {
            isOpen = true; // Mark keypad as open
            // Disable player movement and interaction while keypad is active
            AKDisableManager.instance.DisablePlayerDefault(true, true, false);
            AKUIManager.instance.SetKeypadController(this); // Link this controller to the UI
            SetKeypadTypeActive(true); // Show the appropriate keypad UI based on type

            // Handle trigger object visibility for trigger-based keypads
            if (isTriggerEvent)
            {
                AKUIManager.instance.SetKeypadInteractPrompt(false); // Hide interaction prompt
                triggerObject.SetActive(false); // Deactivate trigger object
            }

            // Register prompts for the keypad subsystem
            AKPromptManager.Instance.RegisterPromptsForSubsystem("Keypad");
        }

        public void CloseKeypad()
        {
            isOpen = false; // Mark keypad as closed
            // Re-enable player movement and interaction
            AKDisableManager.instance.DisablePlayerDefault(false, false, false);
            AKUIManager.instance.KeypadKeyPressClear(); // Clear keypad input
            SetKeypadTypeActive(false); // Hide the keypad UI

            // Restore trigger object visibility for trigger-based keypads
            if (isTriggerEvent)
            {
                AKUIManager.instance.SetKeypadInteractPrompt(true); // Show interaction prompt
                triggerObject.SetActive(true); // Reactivate trigger object
            }

            // Clear any active prompts
            AKPromptManager.Instance.ClearPrompts();
        }

        // Activates or deactivates the keypad UI based on the keypad type
        void SetKeypadTypeActive(bool on)
        {
            switch (_keypadType)
            {
                case KeypadType.Modern:
                    AKUIManager.instance.ShowModernCanvas(on); // Show/hide modern keypad UI
                    break;
                case KeypadType.Scifi:
                    AKUIManager.instance.ShowScifiCanvas(on); // Show/hide sci-fi keypad UI
                    break;
                case KeypadType.Keyboard:
                    AKUIManager.instance.ShowKeyboardCanvas(on); // Show/hide keyboard keypad UI
                    break;
            }
        }

        // Validates the entered code against the list of valid codes
        public void CheckCode(TMP_InputField numberInputField)
        {
            // Check if the entered code matches any in the code list
            var code = keypadCodesList.FirstOrDefault(x => x.keypadCode == numberInputField.text);
            if (code != null)
            {
                code.keypadEvent.Invoke(); // Trigger the associated event for the valid code
            }
            else
            {
                KeyPadDeniedSound(); // Play denied sound for invalid code
            }
        }

        // Plays the beep sound for a single key press
        public void SingleBeepSound()
        {
            AKAudioManager.instance.Play(keypadBeep);
        }

        // Plays the denied sound for an invalid code
        public void KeyPadDeniedSound()
        {
            AKAudioManager.instance.Play(keypadDenied);
        }
    }
}