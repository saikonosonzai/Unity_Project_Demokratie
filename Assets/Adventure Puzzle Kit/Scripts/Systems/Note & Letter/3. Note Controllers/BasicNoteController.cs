/// REMEMBER: This script has a custom editor called "BasicNoteEditor", found in the "Editor" folder. You will need to add new properties to this
/// if you create new variables / fields in this script. Contact me if you have any troubles at all!

using System.Collections;
using UnityEngine;

namespace AdventurePuzzleKit.NoteSystem
{
    // Controls the logic for displaying and interacting with a basic note in the game
    public class BasicNoteController : MonoBehaviour
    {
        // Determines if the note can be read
        [SerializeField] private bool _isReadable = true;

        // The overall scale of the note in the UI (X, Y)
        [Tooltip("Overall X, Y scale of the note")]
        [SerializeField] private Vector2 noteScale = new Vector2(900, 900);

        // Indicates if the note has multiple pages
        [SerializeField] private bool hasMultPages = false;

        // Array of images used as backgrounds for note pages
        [Tooltip("Add the image from your project panel to this slot, as a note background")]
        [Space(5)][SerializeField] private Sprite[] pageImages = null;

        [SerializeField] private bool _allowAudioPlayback = false; // Determines if audio playback is enabled
        [SerializeField] private bool playOnOpen = false; // If true, audio plays automatically when note is opened
        [SerializeField] private Sound noteReadAudio = null; // Audio clip for note reading
        [SerializeField] private Sound noteFlipAudio = null; // Audio clip for page flipping

        [SerializeField] private GameObject triggerObject = null; // Object that triggers note interaction
        [SerializeField] private bool _isNoteTrigger = false; // Indicates if the note is activated by a trigger

        // References to components and state variables
        private BasicNoteUIManager noteUIController; // Reference to the UI manager for notes
        private AKInteractor notesRaycastScript; // Reference to the raycast interaction script
        private BoxCollider boxCollider; // Reference to the note's box collider
        private bool canClick; // Tracks if the note can be interacted with via input
        private bool audioPlaying; // Tracks if audio is currently playing
        private int pageNum = 0; // Current page number of the note

        // Public property for readability status
        public bool isReadable
        {
            get { return _isReadable; }
            set { _isReadable = value; }
        }

        // Public property for audio playback permission
        public bool allowAudioPlayback
        {
            get { return _allowAudioPlayback; }
            set { _allowAudioPlayback = value; }
        }

        // Public property for trigger status
        public bool isNoteTrigger
        {
            get { return _isNoteTrigger; }
            set { _isNoteTrigger = value; }
        }

        private void Awake()
        {
            canClick = false; // Disable input initially
            notesRaycastScript = Camera.main.GetComponent<AKInteractor>(); // Get raycast interaction component
            boxCollider = GetComponent<BoxCollider>(); // Get box collider component
            DebugReferenceCheck(); // Check for missing audio references
        }

        private void Update()
        {
            // Close the note if the close key is pressed while the note is open
            if (canClick && Input.GetKeyDown(AKInputManager.instance.closeNoteKey))
            {
                CloseNote();
            }
        }

        public void ShowNote()
        {
            BasicNoteUIManager.instance.noteController = gameObject.GetComponent<BasicNoteController>(); // Link this controller to the UI
            noteUIController = BasicNoteUIManager.instance; // Cache UI manager reference
            StartCoroutine(WaitTime()); // Start delay to enable input
            AKDisableManager.instance.DisablePlayerDefault(true, true, false); // Disable player movement and interaction
            notesRaycastScript.enabled = false; // Disable raycast interaction
            boxCollider.enabled = false; // Disable note's collider

            // Hide previous button if on the first page
            if (pageNum <= 1)
            {
                noteUIController.ShowPreviousButton(false);
            }

            // Show page navigation buttons if the note has multiple pages
            if (hasMultPages)
            {
                noteUIController.ShowPageButtons(true);
            }

            AKUIManager.instance.SetHighlightName(null, false, false, false, false, false); // Clear UI highlight
            noteUIController.BasicNoteInitialize(pageImages[pageNum], noteScale); // Initialize note UI with current page
            PlayFlipAudio(); // Play page flip sound

            // Handle audio playback settings
            if (allowAudioPlayback)
            {
                noteUIController.ShowAudioPrompt(true); // Show audio prompt UI
                if (playOnOpen)
                {
                    PlayAudio(); // Play audio automatically if configured
                }
            }

            // Handle trigger object visibility for trigger-based notes
            if (isNoteTrigger)
            {
                EnableTrigger(false); // Hide trigger object and prompt
            }

            AKPromptManager.Instance.RegisterPromptsForSubsystem("Note"); // Register note-specific prompts
        }

        public void CloseNote()
        {
            noteUIController.DisableNoteDisplay(false); // Hide note UI
            AKDisableManager.instance.DisablePlayerDefault(false, false, false); // Re-enable player movement and interaction
            notesRaycastScript.enabled = true; // Re-enable raycast interaction
            boxCollider.enabled = true; // Re-enable note's collider
            isReadable = true; // Mark note as readable again
            ResetNote(); // Reset note state
            enabled = false; // Disable this script

            // Hide page buttons and audio prompt for multi-page notes
            if (hasMultPages)
            {
                noteUIController.ShowPageButtons(false);
                noteUIController.ShowAudioPrompt(false);
            }

            // Stop audio if playing
            if (playOnOpen || allowAudioPlayback)
            {
                StopAudio();
            }

            // Restore trigger object visibility for trigger-based notes
            if (isNoteTrigger)
            {
                EnableTrigger(true);
            }

            AKPromptManager.Instance.ClearPrompts(); // Clear active prompts
        }

        // Navigates to the next page of the note
        public void NextPage()
        {
            if (pageNum < pageImages.Length - 1) // Check if there are more pages
            {
                pageNum++; // Increment page number
                noteUIController.DisplayPage(pageImages[pageNum]); // Update displayed page
                PlayFlipAudio(); // Play page flip sound
                EnabledButtons(); // Enable navigation buttons
                if (pageNum >= pageImages.Length - 1) // Hide next button if on last page
                {
                    noteUIController.ShowNextButton(false);
                }
            }
        }

        // Navigates to the previous page of the note
        public void BackPage()
        {
            if (pageNum >= 1) // Check if not on the first page
            {
                pageNum--; // Decrement page number
                noteUIController.DisplayPage(pageImages[pageNum]); // Update displayed page
                PlayFlipAudio(); // Play page flip sound
                EnabledButtons(); // Enable navigation buttons
                if (pageNum < 1) // Hide previous button if on first page
                {
                    noteUIController.ShowPreviousButton(false);
                }
            }
        }

        // Enables both navigation buttons
        void EnabledButtons()
        {
            noteUIController.ShowPreviousButton(true);
            noteUIController.ShowNextButton(true);
        }

        // Resets the note to its initial state
        void ResetNote()
        {
            noteUIController.ShowPreviousButton(false); // Hide previous button
            noteUIController.ShowNextButton(true); // Show next button
            pageNum = 0; // Reset to first page
        }

        // Toggles visibility of the trigger object and interaction prompt
        private void EnableTrigger(bool enable)
        {
            AKUIManager.instance.EnableInteractPrompt(enable); // Show/hide interaction prompt
            triggerObject.SetActive(enable); // Show/hide trigger object
        }

        // Coroutine to enable input after a short delay
        IEnumerator WaitTime()
        {
            const float WaitTimer = 0.1f; // Delay before enabling input
            yield return new WaitForSeconds(WaitTimer);
            canClick = true; // Allow input
        }

        // Toggles play/pause of the note's audio
        public void NoteReadingAudio()
        {
            if (!audioPlaying)
            {
                PlayAudio(); // Start audio if not playing
            }
            else
            {
                PauseAudio(); // Pause audio if playing
            }
        }

        // Plays the page flip audio
        void PlayFlipAudio()
        {
            AKAudioManager.instance.Play(noteFlipAudio);
        }

        // Replays the note's audio from the beginning
        public void RepeatReadingAudio()
        {
            StopAudio(); // Stop current audio
            PlayAudio(); // Start audio again
        }

        private void PlayAudio()
        {
            AKAudioManager.instance.Play(noteReadAudio);
            audioPlaying = true; // Mark audio as playing
        }

        private void StopAudio()
        {
            AKAudioManager.instance.StopPlaying(noteReadAudio);
            audioPlaying = false; // Mark audio as stopped
        }

        private void PauseAudio()
        {
            AKAudioManager.instance.PausePlaying(noteReadAudio);
            audioPlaying = false; // Mark audio as paused
        }

        // Checks for missing audio references and logs warnings
        void DebugReferenceCheck()
        {
            if (noteFlipAudio == null)
            {
                print("BasicNoteController on " + gameObject.name + ": Add a reference to the note flip sound Scriptable to the inspector");
            }

            if (allowAudioPlayback && noteReadAudio == null)
            {
                print("BasicNoteController on " + gameObject.name + ": Add a reference to the sound Scriptable to the inspector");
            }
        }
    }
}