/// REMEMBER: This script has a custom editor called "CustomReverseNoteCustomEditor", found in the "Editor" folder. You will need to add new properties to this
/// if you create new variables / fields in this script. Contact me if you have any troubles at all!

using System.Collections;
using TMPro;
using UnityEngine;

namespace AdventurePuzzleKit.NoteSystem
{
    // Controls the logic for displaying and interacting with a custom reverse note, featuring a single image and a flippable text panel with customizable text
    public class CustomReverseNoteController : MonoBehaviour
    {
        // Determines if the note can be read
        [SerializeField] private bool _isReadable = true;

        // Background image for the note
        [SerializeField] private Sprite pageImage = null;

        // The overall scale of the note page in the UI (X, Y)
        [SerializeField] private Vector2 pageScale = new Vector2(900, 900);

        // Indicates if the note has multiple pages
        [SerializeField] private bool hasMultPages = false;

        // Array of text content for the note pages
        [TextArea(4, 8)][SerializeField] private string[] noteReverseText = null;

        // Scale of the main text area for the note
        [SerializeField] private Vector2 mainTextAreaScale = new Vector2(495, 795);

        // Font size for the main text
        [SerializeField] private int mainTextSize = 25;

        // Font asset for the main text
        [SerializeField] private TMP_FontAsset mainFontType = null;

        // Font color for the main text
        [SerializeField] private Color mainFontColor = Color.black;

        // Background color for the flip-side text panel
        [SerializeField] private Color flipTextBGColor = Color.white;

        // Scale of the flip-side text area
        [SerializeField] private Vector2 flipTextAreaScale = new Vector2(1045, 300);

        // Scale of the flip-side text background
        [SerializeField] private Vector2 flipTextBGScale = new Vector2(1160, 300);

        // Font size for the flip-side text
        [SerializeField] private int flipTextSize = 25;

        // Font asset for the flip-side text
        [SerializeField] private TMP_FontAsset flipFontType = null;

        // Font color for the flip-side text
        [SerializeField] private Color flipFontColor = Color.black;

        // Audio playback settings for the note
        [SerializeField] private bool _allowAudioPlayback = false; // Determines if audio playback is enabled
        [SerializeField] private bool playOnOpen = false; // If true, audio plays automatically when note is opened
        [SerializeField] private Sound noteReadAudio = null; // Audio clip for note reading
        [SerializeField] private Sound noteFlipAudio = null; // Audio clip for page flipping

        // Trigger settings for note interaction
        [SerializeField] private bool _isNoteTrigger = false; // Indicates if the note is activated by a trigger
        [SerializeField] private GameObject triggerObject = null; // Object that triggers note interaction

        // State variables
        private bool canReverse; // Tracks if the reverse text panel can be shown
        private bool canClick; // Tracks if the note can be interacted with via input
        private bool isNoteActivate; // Tracks if the note UI is currently open
        private BoxCollider boxCollider; // Reference to the note's box collider
        private AKInteractor notesRaycastScript; // Reference to the raycast interaction script
        private CustomReverseNoteUIManager noteUIController; // Reference to the UI manager for custom reverse notes
        private int pageNum = 0; // Current page number of the note
        private bool audioPlaying; // Tracks if audio is currently playing

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

        private void Start()
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
            CustomReverseNoteUIManager.instance.noteController = gameObject.GetComponent<CustomReverseNoteController>(); // Link this controller to the UI
            noteUIController = CustomReverseNoteUIManager.instance; // Cache UI manager reference
            StartCoroutine(WaitTime()); // Start delay to enable input
            AKDisableManager.instance.DisablePlayerDefault(true, true, false); // Disable player movement and interaction
            boxCollider.enabled = false; // Disable note's collider
            notesRaycastScript.enabled = false; // Disable raycast interaction

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
            // Initialize main note UI with page image, text, and formatting
            noteUIController.ReverseCustomInitialiseMainNote(pageImage, pageScale, noteReverseText[pageNum], mainTextAreaScale, mainTextSize, mainFontType, mainFontColor);
            // Initialize flip-side text panel with formatting
            noteUIController.ReverseCustomInitialiseFlipSide(flipTextBGColor, flipTextAreaScale, noteReverseText[pageNum], flipTextSize, flipFontType, flipFontColor, flipTextBGScale);

            PlayFlipAudio(); // Play page flip sound
            isNoteActivate = true; // Mark note as active

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
            noteUIController.DisableNoteDisplay(false); // Hide main note UI
            noteUIController.SetReverseNoteAction(false); // Hide reverse text panel
            AKDisableManager.instance.DisablePlayerDefault(false, false, false); // Re-enable player movement and interaction
            notesRaycastScript.enabled = true; // Re-enable raycast interaction
            boxCollider.enabled = true; // Re-enable note's collider
            canReverse = false; // Disable reverse panel
            isNoteActivate = false; // Mark note as inactive
            ResetNote(); // Reset note state
            enabled = false; // Disable this script

            // Hide page buttons for multi-page notes
            if (hasMultPages)
            {
                noteUIController.ShowPageButtons(false);
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

        // Toggles the visibility of the reverse text panel
        public void ReverseNoteAction()
        {
            if (isNoteActivate)
            {
                canReverse = !canReverse; // Toggle reverse panel state

                noteUIController.SetReverseNoteAction(canReverse); // Show or hide reverse panel
            }
        }

        // Navigates to the next page of the note
        public void NextPage()
        {
            if (pageNum < noteReverseText.Length - 1) // Check if there are more pages
            {
                pageNum++; // Increment page number
                noteUIController.FillNoteText(noteReverseText[pageNum]); // Update text content for both main and flip-side
                EnabledButtons(); // Enable navigation buttons
                PlayFlipAudio(); // Play page flip sound

                // Hide next button if on the last page
                if (pageNum >= noteReverseText.Length - 1)
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
                noteUIController.FillNoteText(noteReverseText[pageNum]); // Update text content for both main and flip-side
                EnabledButtons(); // Enable navigation buttons
                PlayFlipAudio(); // Play page flip sound

                // Hide previous button if on the first page
                if (pageNum < 1)
                {
                    noteUIController.ShowPreviousButton(false);
                }
            }
        }

        // Resets the note to its initial state
        void ResetNote()
        {
            noteUIController.ShowPreviousButton(false); // Hide previous button
            noteUIController.ShowNextButton(true); // Show next button
            pageNum = 0; // Reset to first page
        }

        // Enables both navigation buttons
        void EnabledButtons()
        {
            noteUIController.ShowPreviousButton(true);
            noteUIController.ShowNextButton(true);
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

        void PlayFlipAudio()
        {
            AKAudioManager.instance.Play(noteFlipAudio);
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

        public void RepeatReadingAudio()
        {
            StopAudio(); // Stop current audio
            PlayAudio(); // Start audio again
        }

        public void PlayAudio()
        {
            AKAudioManager.instance.Play(noteReadAudio);
            audioPlaying = true; // Mark audio as playing
        }

        public void StopAudio()
        {
            AKAudioManager.instance.StopPlaying(noteReadAudio);
            audioPlaying = false; // Mark audio as stopped
        }

        public void PauseAudio()
        {
            AKAudioManager.instance.PausePlaying(noteReadAudio);
            audioPlaying = false; // Mark audio as paused
        }

        // Checks for missing audio references and logs warnings
        void DebugReferenceCheck()
        {
            if (noteFlipAudio == null)
            {
                print("ReverseCustomNoteController on " + gameObject.name + ": Add a reference to the note flip sound Scriptable to the inspector");
            }

            if (allowAudioPlayback && noteReadAudio == null)
            {
                print("ReverseCustomNoteController on " + gameObject.name + ": Add a reference to the sound Scriptable to the inspector");
            }
        }
    }
}