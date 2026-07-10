/// REMEMBER: This script has a custom editor called "PadlockControllerEditor", found in the "Editor" folder. You will need to add new properties to this
/// if you create new variables / fields in this script. Contact me if you have any troubles at all!

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AdventurePuzzleKit.PadlockSystem
{
    // Manages the logic for a padlock puzzle, handling combination input, UI, audio, and unlocking events
    public class PadlockController : MonoBehaviour
    {
        // The correct combination for the padlock (e.g., "1234")
        [Header("Padlock Code")]
        [SerializeField] private string yourCombination = null;

        // The interactable lock object in the scene
        [Header("Interactive Padlock")]
        [SerializeField] private GameObject interactableLock = null;

        // The padlock UI prefab to spawn when interacting
        [Header("Prefab To Spawn")]
        [SerializeField] private GameObject padlockPrefab = null;

        // Distance from the camera to spawn the padlock UI
        [Header("Spawn Distance")]
        [SerializeField] private float distanceFromCamera = 0.3f;

        // Animation name for the padlock opening
        [Header("Animation Name")]
        [SerializeField] private string lockOpen = "LockOpen";

        // Audio clips for padlock interactions
        [Header("Sounds")]
        [SerializeField] private Sound padlockInteract = null; // Sound played when interacting with the padlock
        [SerializeField] private Sound padlockSpin = null; // Sound played when spinning a number
        [SerializeField] private Sound padlockUnlock = null; // Sound played when unlocking the padlock

        // Trigger settings for padlock interaction
        [Header("Trigger Type - ONLY if using a trigger event")]
        [SerializeField] private bool isPadlockTrigger = false; // Indicates if the padlock is activated by a trigger
        [SerializeField] private GameObject triggerObject = null; // Object that triggers padlock interaction

        // Event triggered when the padlock is unlocked
        [Header("Unlock Events")]
        [SerializeField] private UnityEvent unlock = null;

        // Current values for each combination row (1-9)
        public int combinationRow1 { get; set; }
        public int combinationRow2 { get; set; }
        public int combinationRow3 { get; set; }
        public int combinationRow4 { get; set; }

        private string playerCombi; // Player's current combination input
        private bool hasUnlocked; // Tracks if the padlock has been unlocked
        private bool isShowing; // Tracks if the padlock UI is currently displayed
        private Camera mainCamera; // Reference to the main camera
        private Animator lockAnim; // Animator for the padlock UI
        private GameObject instantiatedPadlock; // Reference to the spawned padlock prefab instance

        void Awake()
        {
            mainCamera = Camera.main; // Cache reference to the main camera
            // Initialize combination rows to 1
            combinationRow1 = 1;
            combinationRow2 = 1;
            combinationRow3 = 1;
            combinationRow4 = 1;
        }

        void Update()
        {
            // Close the padlock UI if the close key is pressed while the UI is showing
            if (isShowing && Input.GetKeyDown(AKInputManager.instance.closePadlockKey))
            {
                DisablePadlock();
            }
        }

        // Triggers the unlock event when the correct combination is entered
        void UnlockPadlock()
        {
            unlock.Invoke();
        }

        // Displays the padlock UI and sets up interaction
        public void ShowPadlock()
        {
            isShowing = true; // Mark padlock UI as active
            AKDisableManager.instance.DisablePlayerDefault(true, true, false); // Disable player movement and interaction
            SpawnPadlock(distanceFromCamera); // Spawn the padlock UI
            mainCamera.transform.localEulerAngles = new Vector3(0, 0, 0); // Reset camera rotation
            InteractSound(); // Play interaction sound

            // Handle trigger object visibility for trigger-based padlocks
            if (isPadlockTrigger)
            {
                AKUIManager.instance.EnableInteractPrompt(false); // Hide interaction prompt
                triggerObject.SetActive(false); // Hide trigger object
            }

            AKPromptManager.Instance.RegisterPromptsForSubsystem("Padlock"); // Register padlock-specific prompts
        }

        // Spawns the padlock UI prefab at the specified distance from the camera
        void SpawnPadlock(float distance)
        {
            // Instantiate the padlock prefab as a child of the camera
            GameObject padlockInstance = Instantiate(padlockPrefab, mainCamera.transform);

            // Set the local position and rotation of the instantiated padlock
            padlockInstance.transform.localPosition = new Vector3(0, 0, distance);
            padlockInstance.transform.localRotation = Quaternion.Euler(0, 90, 0);

            // Store the instantiated padlock reference
            instantiatedPadlock = padlockInstance;

            // Get the Animator component from the padlock's children
            lockAnim = padlockInstance.GetComponentInChildren<Animator>();

            // Get all PadlockNumberSelector components on the padlock's children
            PadlockNumberSelector[] numberSelectors = padlockInstance.GetComponentsInChildren<PadlockNumberSelector>();

            // Update each selector to reference this controller
            foreach (PadlockNumberSelector selector in numberSelectors)
            {
                selector.UpdatePadlockController(this);
            }
        }

        // Closes the padlock UI and resets interaction
        void DisablePadlock()
        {
            isShowing = false; // Mark padlock UI as inactive
            AKDisableManager.instance.DisablePlayerDefault(false, false, false); // Re-enable player movement and interaction
            Destroy(instantiatedPadlock); // Destroy the padlock UI instance

            // Restore trigger object visibility for trigger-based padlocks
            if (isPadlockTrigger)
            {
                AKUIManager.instance.EnableInteractPrompt(true); // Show interaction prompt
                triggerObject.SetActive(true); // Show trigger object
            }

            AKPromptManager.Instance.ClearPrompts(); // Clear active prompts
        }

        // Checks if the player's combination matches the correct combination
        public void CheckCombination()
        {
            // Construct the player's combination string
            playerCombi = combinationRow1.ToString("0") + combinationRow2.ToString("0") + combinationRow3.ToString("0") + combinationRow4.ToString("0");

            // If the combination is correct and the padlock hasn't been unlocked yet
            if (playerCombi == yourCombination && !hasUnlocked)
            {
                StartCoroutine(CorrectCombination()); // Handle successful unlock
                hasUnlocked = true; // Mark as unlocked
            }
        }

        // Coroutine to handle the successful combination sequence
        IEnumerator CorrectCombination()
        {
            lockAnim.Play(lockOpen); // Play the unlock animation
            UnlockSound(); // Play the unlock sound

            const float waitDuration = 1.2f; // Wait time for animation
            yield return new WaitForSeconds(waitDuration);

            Destroy(instantiatedPadlock); // Destroy the padlock UI
            interactableLock.SetActive(false); // Hide the interactable lock
            UnlockPadlock(); // Trigger the unlock event

            AKDisableManager.instance.DisablePlayerDefault(false, false, false); // Re-enable player movement
            gameObject.SetActive(false); // Deactivate this game object
        }

        void InteractSound()
        {
            AKAudioManager.instance.Play(padlockInteract);
        }

        public void SpinSound()
        {
            AKAudioManager.instance.Play(padlockSpin);
        }

        public void UnlockSound()
        {
            AKAudioManager.instance.Play(padlockUnlock);
        }
    }
}