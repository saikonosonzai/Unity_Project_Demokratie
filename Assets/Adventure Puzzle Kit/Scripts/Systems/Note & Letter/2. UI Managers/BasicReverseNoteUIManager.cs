using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AdventurePuzzleKit.NoteSystem
{
    // Manages the UI for displaying and interacting with reverse notes, which include both images and customizable text
    public class BasicReverseNoteUIManager : MonoBehaviour
    {
        // UI element for audio prompt display
        [Header("Audio Prompt UI")]
        [SerializeField] private GameObject audioPromptUI = null;

        // UI elements for page navigation buttons
        [Header("Page Buttons UI")]
        [SerializeField] private GameObject pageButtons = null; // Container for page navigation buttons
        [SerializeField] private GameObject nextButton = null; // Button to navigate to the next page
        [SerializeField] private GameObject previousButton = null; // Button to navigate to the previous page

        // UI elements for the main note display
        [Header("Reverse Note Main UI's")]
        [SerializeField] private GameObject reverseNoteMainUI = null; // Main UI canvas for the reverse note
        [SerializeField] private Image reverseNotePageUI = null; // Image component for displaying the note page

        // UI elements for the text panel
        [Header("Reverse Note Text UI's")]
        [SerializeField] private GameObject reverseNoteTextPanelUI = null; // Panel for displaying text content
        [SerializeField] private Image reverseNoteTextImage = null; // Background image for the text panel
        [SerializeField] private TMP_Text reverseNoteTextUI = null; // TextMeshPro component for displaying note text

        // Reference to the controller managing reverse note logic
        public BasicReverseNoteController noteController { get; set; }

        // Singleton instance of the BasicReverseNoteUIManager
        public static BasicReverseNoteUIManager instance;

        // Called when the object is initialized
        private void Awake()
        {
            if (instance == null) { instance = this; } // Set this as the singleton instance
        }

        // Initializes the reverse note UI with specified page image, text, and formatting
        public void BasicReverseInitialize(Sprite pageImage, Vector2 textAreaScale, string noteText, int textSize, TMP_FontAsset fontType,
            Color fontColor, Vector2 pageScale, Vector2 customTextBGScale, Color customTextBGColor)
        {
            DisplayPage(pageImage); // Set the initial page image
            DisableNoteDisplay(true); // Show the main note UI

            reverseNotePageUI.rectTransform.sizeDelta = pageScale; // Adjust the note page size

            reverseNoteTextImage.rectTransform.sizeDelta = customTextBGScale; // Set text panel background size
            reverseNoteTextImage.color = customTextBGColor; // Set text panel background color

            reverseNoteTextUI.rectTransform.sizeDelta = textAreaScale; // Set text area size
            reverseNoteTextUI.text = noteText; // Set the note text
            reverseNoteTextUI.fontSize = textSize; // Set the font size
            reverseNoteTextUI.font = fontType; // Set the font type
            reverseNoteTextUI.color = fontColor; // Set the font color
        }

        // Toggles the visibility of the main note UI
        public void DisableNoteDisplay(bool active)
        {
            reverseNoteMainUI.SetActive(active);
        }

        // Toggles the visibility of the text panel UI
        public void DisableReverseNoteDisplay(bool active)
        {
            reverseNoteTextPanelUI.SetActive(active);
        }

        // Updates the note page image
        public void DisplayPage(Sprite pageImage)
        {
            reverseNotePageUI.sprite = pageImage; // Set the image component to the provided sprite
        }

        // Updates the text content of the note
        public void FillReverseText(string textString)
        {
            reverseNoteTextUI.text = textString; // Set the text content
        }

        // Toggles the visibility of the text panel
        public void ShowReverseNotePanel(bool show)
        {
            reverseNoteTextPanelUI.SetActive(show);
        }

        // Toggles the visibility of the previous page button
        public void ShowPreviousButton(bool show)
        {
            previousButton.SetActive(show);
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

        // Triggers the reverse note action (e.g., toggling text panel)
        public void ReverseNoteButton()
        {
            noteController.ReverseNoteAction(); // Call the controller to handle reverse note action
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