using UnityEngine;
using System.Linq;
using TMPro;

namespace AdventurePuzzleKit.PhoneSystem
{
    // Serializable class to store a phone code and its associated audio clip
    [System.Serializable]
    public class PhoneCodes
    {
        public string phoneCode; // The code to dial on the phone
        public Sound phoneClip; // Audio clip played when the code is dialed correctly
    }

    // Manages the phone system, handling keypad input, code validation, UI, and audio feedback
    public class PhoneController : MonoBehaviour
    {
        // Defines the visual style of the phone
        [Header("Phone UI Type")]
        [SerializeField] private PhoneType _phoneType = PhoneType.None;
        private enum PhoneType { None, Pay, Office, Mobile }; // Enum for different phone UI styles

        [Header("Keypad Parameters")]
        [SerializeField] private int _inputLimit = 10;

        [Header("Phone Codes")]
        [SerializeField] private PhoneCodes[] phoneCodesList = null;

        [Header("Sound Effects")]
        [SerializeField] private Sound deadDialSound = null; // Sound played for invalid code
        [SerializeField] private Sound singleBeepSound = null; // Sound played for each key press

        [Header("Trigger Type - ONLY if using a trigger event")]
        [SerializeField] private bool isPhoneTrigger = false; // Indicates if the phone is activated by a trigger
        [SerializeField] private GameObject triggerObject = null; // Object that triggers phone interaction

        private AudioSource mainAudio; // Reference to the AudioSource component
        private bool isOpen = false; // Tracks if the phone UI is currently open

        // Public property for accessing/modifying the input character limit
        public int inputLimit
        {
            get { return _inputLimit; }
            set { _inputLimit = value; }
        }

        private void Awake()
        {
            mainAudio = GetComponent<AudioSource>(); // Cache the AudioSource component
        }

        private void Update()
        {
            // Close the phone UI if the close key is pressed while the UI is open
            if (isOpen && Input.GetKeyDown(AKInputManager.instance.closeKeypadKey))
            {
                CloseKeypad();
            }
        }

        public void ShowKeypad()
        {
            isOpen = true; // Mark phone UI as open
            AKDisableManager.instance.DisablePlayerDefault(true, true, false); // Disable player movement and interaction
            AKUIManager.instance.SetPhoneController(this); // Link this controller to the UI
            SetPhoneTypeActive(true); // Show the appropriate phone UI based on type

            // Handle trigger object visibility for trigger-based phones
            if (isPhoneTrigger)
            {
                AKUIManager.instance.SetPhoneInteractPrompt(false); // Hide interaction prompt
                triggerObject.SetActive(false); // Hide trigger object
            }

            AKPromptManager.Instance.RegisterPromptsForSubsystem("Keypad"); // Register keypad-specific prompts
        }

        public void CloseKeypad()
        {
            isOpen = false; // Mark phone UI as closed
            AKDisableManager.instance.DisablePlayerDefault(false, false, false); // Re-enable player movement and interaction
            AKUIManager.instance.PhoneKeyPressClear(); // Clear phone input
            SetPhoneTypeActive(false); // Hide the phone UI

            // Restore trigger object visibility for trigger-based phones
            if (isPhoneTrigger)
            {
                AKUIManager.instance.SetPhoneInteractPrompt(true); // Show interaction prompt
                triggerObject.SetActive(true); // Show trigger object
            }

            AKPromptManager.Instance.ClearPrompts(); // Clear active prompts
        }

        // Activates or deactivates the phone UI based on the phone type
        void SetPhoneTypeActive(bool on)
        {
            switch (_phoneType)
            {
                case PhoneType.Pay:
                    AKUIManager.instance.ShowPayPhoneCanvas(on); // Show/hide pay phone UI
                    break;
                case PhoneType.Office:
                    AKUIManager.instance.ShowOfficePhoneCanvas(on); // Show/hide office phone UI
                    break;
                case PhoneType.Mobile:
                    AKUIManager.instance.ShowMobilePhoneCanvas(on); // Show/hide mobile phone UI
                    break;
            }
        }

        // Validates the entered phone code against the list of valid codes
        public void CheckCode(TMP_InputField numberInputField)
        {
            StopAudio(); // Stop any currently playing audio
            // Check if the entered code matches any in the code list
            var code = phoneCodesList.FirstOrDefault(x => x.phoneCode == numberInputField.text);
            if (code != null)
            {
                AKAudioManager.instance.Play(code.phoneClip); // Play the associated audio clip for valid code
            }
            else
            {
                DeadDialSound(); // Play dead dial sound for invalid code
            }
        }

        public void SingleBeepSound()
        {
            AKAudioManager.instance.Play(singleBeepSound);
        }

        void DeadDialSound()
        {
            AKAudioManager.instance.Play(deadDialSound);
        }

        void StopDeadDialSound()
        {
            AKAudioManager.instance.StopPlaying(deadDialSound);
        }

        // Stops all audio, including the dead dial sound
        public void StopAudio()
        {
            AKAudioManager.instance.StopAll(); // Stop all audio clips
            StopDeadDialSound(); // Ensure dead dial sound is stopped
        }
    }
}