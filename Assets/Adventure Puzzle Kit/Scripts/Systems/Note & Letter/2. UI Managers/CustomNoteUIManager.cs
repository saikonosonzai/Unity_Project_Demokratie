using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AdventurePuzzleKit.NoteSystem
{
    // Manages the UI for displaying and interacting with custom notes, including images and customizable text
    public class CustomNoteUIManager : MonoBehaviour
    {
        // UI element for audio prompt display
        [Header("Audio Prompt UI")]
        [SerializeField] private GameObject audioPromptUI = null;

        // UI elements for page navigation buttons
        [Header("Page Buttons UI")]
        [SerializeField] private GameObject pageButtons = null; // Container for page navigation buttons
        [SerializeField] private GameObject nextButton = null; // Button to navigate to the next page
        [SerializeField] private GameObject previousButton = null; // Button to navigate to the previous page

        // UI elements for the note page display
        [Header("Note Page UI's")]
        [SerializeField] private GameObject customNoteMainUI = null; // Main UI canvas for the custom note
        [SerializeField] private Image customNotePageUI = null; // Image component for displaying the note page

        // UI element for the note text
        [Header("Note Text UI's")]
        [SerializeField] private TMP_Text customNoteTextUI = null; // TextMeshPro component for displaying note text

        // Reference to the controller managing custom note logic
        public CustomNoteController noteController { get; set; } = null;

        // Singleton instance of the CustomNoteUIManager
        public static CustomNoteUIManager instance;

        // Called when the object is initialized
        private void Awake()
        {
            if (instance == null) { instance = this; } // Set this as the singleton instance
        }

        // Initializes the custom note UI with specified page image, text, and formatting
        public void CustomNoteInitialize(Sprite pageImage, Vector2 pageScale, string noteText, Vector2 noteTextAreaScale, int textSize,
            TMP_FontAsset fontType, Color fontColor)
        {
            DisplayPage(pageImage); // Set the initial page image
            DisableNoteDisplay(true); // Show the main note UI

            customNotePageUI.rectTransform.sizeDelta = pageScale; // Adjust the note page size

            customNoteTextUI.text = noteText; // Set the note text

            customNoteTextUI.rectTransform.sizeDelta = noteTextAreaScale; // Set the text area size

            customNoteTextUI.fontSize = textSize; // Set the font size
            customNoteTextUI.font = fontType; // Set the font type
            customNoteTextUI.color = fontColor; // Set the font color

            customNoteMainUI.SetActive(true); // Ensure the main UI is active
        }

        // Toggles the visibility of the previous page button
        public void ShowPreviousButton(bool show)
        {
            previousButton.SetActive(show);
        }

        // Toggles the visibility of the main note UI
        public void DisableNoteDisplay(bool active)
        {
            customNoteMainUI.SetActive(active);
        }

        // Updates the note page image
        public void DisplayPage(Sprite pageImage)
        {
            customNotePageUI.sprite = pageImage; // Set the image component to the provided sprite
        }

        // Updates the text content of the note
        public void FillNoteText(string noteText)
        {
            customNoteTextUI.text = noteText; // Set the text content
        }

        // Toggles the visibility of the next page button
        public void ShowNextButton(bool show)
        {
            nextButton.SetActive(show);
        }

        // Toggles the visibility of the page navigation buttons container
        public void ShowPageButtons(bool shouldShow)
        {
            pageButtons.SetActive(shouldShow); // Show or hide the page buttons
        }

        // Toggles the visibility of the audio prompt UI
        public void ShowAudioPrompt(bool show)
        {
            audioPromptUI.SetActive(show);
        }

        // Triggers play/pause of the note's audio
        public void PlayPauseAudio()
        {
            noteController.NoteReadingAudio(); // Call the controller to handle audio play/pause
        }

        // Triggers replay of the note's audio
        public void RepeatAudio()
        {
            noteController.RepeatReadingAudio(); // Call the controller to replay audio
        }

        // Closes the note UI
        public void CloseButton()
        {
            noteController.CloseNote(); // Call the controller to close the note
        }

        // Navigates to the next page of the note
        public void NextPage()
        {
            noteController.NextPage(); // Call the controller to show the next page
        }

        // Navigates to the previous page of the note
        public void BackPage()
        {
            noteController.BackPage(); // Call the controller to show the previous page
        }
    }
}