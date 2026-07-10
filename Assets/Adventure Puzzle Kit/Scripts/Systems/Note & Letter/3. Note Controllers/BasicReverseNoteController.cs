/// REMEMBER: This script has a custom editor called "BasicReverseNoteEditor", found in the "Editor" folder. You will need to add new properties to this
/// if you create new variables / fields in this script. Contact me if you have any troubles at all!

using System.Collections;
using TMPro;
using UnityEngine;

namespace AdventurePuzzleKit.NoteSystem
{
    // Controls the logic for displaying and interacting with a reverse note, which includes images and a flippable text panel
    public class BasicReverseNoteController : MonoBehaviour
    {
        // Determines if the note can be read
        [SerializeField] private bool _isReadable = true;

        // The overall scale of the note page in the UI (X, Y)
        [Tooltip("Overall X, Y scale of the note")]
        [SerializeField] private Vector2 pageScale = new Vector2(900, 900);

        // Indicates if the note has multiple pages
        [SerializeField] private bool hasMultPages = false;

        // Array of images used as backgrounds for note pages
        [Tooltip("Add the image from your project panel to this slot, as a note background")]
        [SerializeField] private Sprite[] pageImages = null;

        // Array of text content for the reverse side of the note
        [TextArea(4, 8)][SerializeField] private string[] noteReverseText = null;

        // Scale of the text area for the reverse note
        [Tooltip("This is the scale of where the text is applied, usually slightly smaller than the object below")]
        [SerializeField] private Vector2 noteTextAreaScale = new Vector2(1045, 300);

        // Scale of the background image for the reverse text
        [Tooltip("This is the scale of background image for the reverse text")]
        [SerializeField] private Vector2 customTextBGScale = new Vector2(1160, 300);

        // Background color for the reverse text (alpha should be 1)
        [Tooltip("This is the background colour of the reverse text - Make sure the alpha value is set to 1")]
        [SerializeField] private Color customTextBGColor = Color.white;

        // Font size for the reverse text
        [SerializeField] private int textSize = 25;

        // Font asset for the reverse text
        [SerializeField] private TMP_FontAsset fontType = null;

        // Font color for the reverse text (alpha should be 1)
        [Tooltip("Make sure the alpha value is set to 1")]
        [SerializeField] private Color fontColor = Color.black;

        [SerializeField] private bool _allowAudioPlayback = false; // Determines if audio playback is enabled
        [SerializeField] private bool playOnOpen = false; // If true, audio plays automatically when note is opened
        [SerializeField] private Sound noteReadAudio = null; // Audio clip for note reading
        [SerializeField] private Sound noteFlipAudio = null; // Audio clip for page flipping

        [SerializeField] private bool _isNoteTrigger = false; // Indicates if the note is activated by a trigger
        [SerializeField] private GameObject triggerObject = null; // Object that triggers note interaction

        private BoxCollider boxCollider; // Reference to the note's box collider
        private bool canReverse; // Tracks if the reverse text panel can be shown
        private bool canClick; // Tracks if the note can be interacted with via input
        private bool isNoteActive; // Tracks if the note UI is currently open
        private AKInteractor notesRaycastScript; // Reference to the raycast interaction script
        private BasicReverseNoteUIManager noteUIController; // Reference to the UI manager for reverse notes
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
            BasicReverseNoteUIManager.instance.noteController = gameObject.GetComponent<BasicReverseNoteController>(); // Link this controller to the UI
            noteUIController = BasicReverseNoteUIManager.instance; // Cache UI manager reference
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
            // Initialize note UI with current page, text, and formatting
            noteUIController.BasicReverseInitialize(pageImages[pageNum], noteTextAreaScale, noteReverseText[pageNum], textSize, fontType,
                fontColor, pageScale, customTextBGScale, customTextBGColor);

            PlayFlipAudio(); // Play page flip sound
            isNoteActive = true; // Mark note as active

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
            noteUIController.DisableReverseNoteDisplay(false); // Hide reverse text panel
            AKDisableManager.instance.DisablePlayerDefault(false, false, false); // Re-enable player movement and interaction
            notesRaycastScript.enabled = true; // Re-enable raycast interaction
            boxCollider.enabled = true; // Re-enable note's collider
            isNoteActive = false; // Mark note as inactive
            canReverse = false; // Disable reverse panel
            isReadable = true; // Mark note as readable again
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

        // Navigates to the next page of the note
        public void NextPage()
        {
            if (pageNum < pageImages.Length - 1) // Check if there are more pages
            {
                pageNum++; // Increment page number
                noteUIController.DisplayPage(pageImages[pageNum]); // Update displayed page
                noteUIController.FillReverseText(noteReverseText[pageNum]); // Update reverse text
                PlayFlipAudio(); // Play page flip sound
                EnabledButtons(); // Enable navigation buttons

                // Hide next button if on the last page
                if (pageNum >= pageImages.Length - 1)
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
                noteUIController.FillReverseText(noteReverseText[pageNum]); // Update reverse text
                PlayFlipAudio(); // Play page flip sound
                EnabledButtons(); // Enable navigation buttons

                // Hide previous button if on the first page
                if (pageNum < 1)
                {
                    noteUIController.ShowPreviousButton(false);
                }
            }
        }

        // Toggles the visibility of the reverse text panel
        public void ReverseNoteAction()
        {
            if (isNoteActive)
            {
                canReverse = !canReverse; // Toggle reverse panel state

                noteUIController.ShowReverseNotePanel(canReverse); // Show or hide reverse panel
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
                print("BasicReverseNoteController on " + gameObject.name + ": Add a reference to the note flip sound Scriptable to the inspector");
            }

            if (allowAudioPlayback && noteReadAudio == null)
            {
                print("BasicReverseNoteController on " + gameObject.name + ": Add a reference to the sound Scriptable to the inspector");
            }
        }
    }
}