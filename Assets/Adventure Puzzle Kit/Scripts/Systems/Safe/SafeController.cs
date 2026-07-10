using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace AdventurePuzzleKit.SafeSystem
{
    // Manages the logic for a safe puzzle, handling combination input, UI, animations, audio, and unlocking events
    public class SafeController : MonoBehaviour
    {
        [Header("Universal")]
        [SerializeField] private GameObject safeModel = null; // The safe's 3D model
        [SerializeField] private Transform safeDial = null; // The safe's dial for rotation

        [Header("Animation References")]
        [SerializeField] private string safeAnimationName = "SafeDoorOpen";

        [Header("Animation Timers")]
        [SerializeField] private float beforeAnimationStart = 1.0f; // Delay before starting the unlock animation
        [SerializeField] private float beforeOpenDoor = 0.5f; // Delay before playing the door open sound

        [Header("Safe Solution: 0-15")]
        [Range(0, 15)][SerializeField] private int safeSolutionNum1 = 0; // First number of the combination
        [Range(0, 15)][SerializeField] private int safeSolutionNum2 = 0; // Second number of the combination
        [Range(0, 15)][SerializeField] private int safeSolutionNum3 = 0; // Third number of the combination

        [Header("Trigger Interaction?")]
        [SerializeField] private bool isTriggerInteraction = false; // Indicates if the safe is activated by a trigger
        [SerializeField] private GameObject triggerObject = null; // Object that triggers safe interaction

        [Header("Audio ScriptableObjects")]
        [SerializeField] private SafeAudioClips _safeAudioClips = null; // ScriptableObject containing safe audio clips

        [Header("Unity Event - What happens when you open the safe?")]
        [SerializeField] private UnityEvent safeOpened = null;

        // State variables
        private int lockState; // Current state of the combination input (1-3)
        private bool canClose = false; // Tracks if the safe UI can be closed
        private bool isInteracting = false; // Tracks if the safe UI is active
        private Animator safeAnim; // Animator for the safe model
        private int[] currentLockNumbers = new int[3]; // Current numbers for each lock state
        private int currentLockNumber; // Current number being adjusted

        private void Start()
        {
            safeAnim = safeModel.gameObject.GetComponent<Animator>(); // Cache the safe's Animator component
            for (int i = 0; i < currentLockNumbers.Length; i++)
                currentLockNumbers[i] = 0; // Initialize lock numbers to 0
        }

        public void ShowSafeUI()
        {
            // Handle trigger object visibility for trigger-based safes
            if (isTriggerInteraction)
            {
                canClose = false;
                triggerObject.SetActive(false); // Hide trigger object
                AKUIManager.instance.SetInteractPrompt(false); // Hide interaction prompt
            }

            isInteracting = true; // Mark safe as being interacted with
            lockState = 1; // Start at the first lock state
            AKUIManager.instance.ShowMainSafeUI(true); // Show the safe UI
            AKDisableManager.instance.DisablePlayerDefault(true, true, false); // Disable player movement and interaction
            AKUIManager.instance.SetUIButtons(this); // Link UI buttons to this controller
            PlayInteractSound(); // Play interaction sound

            AKPromptManager.Instance.RegisterPromptsForSubsystem("Safe"); // Register safe-specific prompts
        }

        private void Update()
        {
            // Close the safe UI if the close key is pressed while interacting and closing is allowed
            if (!canClose && isInteracting && Input.GetKeyDown(AKInputManager.instance.closeSafeKey))
            {
                CloseSafeUI();
            }
        }

        private void CloseSafeUI()
        {
            // Restore trigger object visibility for trigger-based safes
            if (isTriggerInteraction)
            {
                canClose = true;
                isInteracting = false;
                triggerObject.SetActive(true); // Show trigger object
                AKUIManager.instance.SetInteractPrompt(true); // Show interaction prompt
            }

            AKDisableManager.instance.DisablePlayerDefault(false, false, false); // Re-enable player movement and interaction
            ResetSafeDial(false); // Reset the safe dial and state
            AKUIManager.instance.ShowMainSafeUI(false); // Hide the safe UI
            isInteracting = false; // Mark safe as not being interacted with

            AKPromptManager.Instance.ClearPrompts(); // Clear active prompts
        }

        // Resets the safe dial and lock state
        void ResetSafeDial(bool hasComplete)
        {
            if (!hasComplete)
            {
                PlayRattleSound(); // Play rattle sound for failed attempt
            }

            lockState = 1; // Reset to first lock state
            AKUIManager.instance.ResetSafeUI(); // Reset the UI
            safeDial.transform.localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f); // Reset dial rotation

            // Reset current lock number and all lock numbers
            currentLockNumber = 0;
            for (int i = 0; i < currentLockNumbers.Length; i++)
            {
                currentLockNumbers[i] = 0;
            }
        }

        // Coroutine to check the entered combination and handle unlocking
        private IEnumerator CheckCode()
        {
            AKUIManager.instance.PlayerInputCode(); // Get the player's input
            string safeSolution = $"{safeSolutionNum1}{safeSolutionNum2}{safeSolutionNum3}"; // Construct the correct combination

            if (AKUIManager.instance.playerInputNumber == safeSolution) // If the combination is correct
            {
                AKDisableManager.instance.DisablePlayerDefault(false, false, false); // Re-enable player movement
                AKUIManager.instance.ShowMainSafeUI(false); // Hide the safe UI
                isInteracting = false; // Mark safe as not being interacted with

                // Untag the interactable object for non-trigger safes
                if (!isTriggerInteraction)
                {
                    safeModel.tag = "Untagged";
                }

                PlayBoltUnlockSound(); // Play bolt unlock sound
                yield return new WaitForSeconds(beforeAnimationStart); // Wait before animation
                safeAnim.Play(safeAnimationName, 0, 0.0f); // Play the safe open animation
                PlayHandleSpinSound(); // Play handle spin sound
                yield return new WaitForSeconds(beforeOpenDoor); // Wait before door open sound
                PlayDoorOpenSound(); // Play door open sound

                // Handle trigger object for trigger-based safes
                if (isTriggerInteraction)
                {
                    canClose = true;
                    triggerObject.SetActive(false); // Hide trigger object
                }

                ResetSafeDial(true); // Reset dial after successful unlock
                safeOpened.Invoke(); // Trigger the safe opened event
            }
            else
            {
                ResetSafeDial(false); // Reset dial for incorrect combination
            }
        }

        // Advances the lock state or checks the combination
        public void CheckDialNumber()
        {
            AKUIManager.instance.ResetEventSystem(); // Reset the UI event system
            PlayInteractSound(); // Play interaction sound

            // Save the current lock number for the current state
            currentLockNumbers[lockState - 1] = currentLockNumber;

            if (lockState < 3) // If not on the last lock state
            {
                AKUIManager.instance.UpdateUIState(lockState); // Update UI to reflect current state
                currentLockNumbers[lockState] = currentLockNumber; // Save current number
                lockState++; // Advance to next lock state
            }
            else
            {
                AKUIManager.instance.UpdateUIState(3); // Update UI for final state
                StartCoroutine(CheckCode()); // Check the combination
                lockState = 1; // Reset to first lock state
            }

            // Set the current lock number to the saved value for the new state
            currentLockNumber = currentLockNumbers[lockState - 1];
            AKUIManager.instance.UpdateNumber(lockState - 1, currentLockNumber); // Update UI number
        }

        // Handles dial movement logic based on user input
        public void MoveDialLogic(int lockNumberSelection)
        {
            AKUIManager.instance.ResetEventSystem(); // Reset the UI event system
            PlaySafeClickSound(); // Play click sound for dial movement

            if (lockNumberSelection == 1 || lockNumberSelection == 3) // Clockwise rotation
            {
                currentLockNumber = (currentLockNumber + 1) % 16; // Increment number (0-15)
                currentLockNumbers[lockState - 1] = currentLockNumber; // Update current state
                RotateDial(false); // Rotate dial counterclockwise
            }
            else if (lockNumberSelection == 2) // Counterclockwise rotation
            {
                currentLockNumber = (currentLockNumber + 15) % 16; // Decrement number (0-15)
                currentLockNumbers[lockState - 1] = currentLockNumber; // Update current state
                RotateDial(true); // Rotate dial clockwise
            }

            AKUIManager.instance.UpdateNumber(lockNumberSelection - 1, currentLockNumber); // Update UI number
        }

        // Rotates the safe dial visually
        void RotateDial(bool positive)
        {
            // Rotate 22.5 degrees (360/16) for each number
            float rotationAngle = positive ? 22.5f : -22.5f;
            safeDial.transform.Rotate(0.0f, 0.0f, rotationAngle, Space.Self);
        }

        void PlayInteractSound()
        {
            _safeAudioClips.PlayInteractSound();
        }

        void PlayBoltUnlockSound()
        {
            _safeAudioClips.PlayBoltUnlockSound();
        }

        void PlayHandleSpinSound()
        {
            _safeAudioClips.PlayHandleSpinSound();
        }

        void PlayDoorOpenSound()
        {
            _safeAudioClips.PlayDoorOpenSound();
        }

        void PlayRattleSound()
        {
            _safeAudioClips.PlayRattleSound();
        }

        void PlaySafeClickSound()
        {
            _safeAudioClips.PlaySafeClickSound();
        }
    }
}