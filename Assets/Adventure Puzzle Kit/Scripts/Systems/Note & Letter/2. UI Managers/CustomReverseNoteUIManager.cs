using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AdventurePuzzleKit.NoteSystem
{
    // Manages the UI for custom reverse notes, which include a main note with image and text, and a flip-side text panel
    public class CustomReverseNoteUIManager : MonoBehaviour
    {
        [Header("Audio Prompt UI")]
        [SerializeField] private GameObject audioPromptUI = null;

        [Header("Page Buttons UI")]
        [SerializeField] private GameObject pageButtons = null; // Container for page navigation buttons
        [SerializeField] private GameObject nextButton = null; // Button to navigate to the next page
        [SerializeField] private GameObject previousButton = null; // Button to navigate to the previous page

        [Header("Main Note Settings")]
        [SerializeField] private GameObject customReverseMainNoteUI = null; // Main UI canvas for the reverse note
        [SerializeField] private Image customReverseNotePageUI = null; // Image component for displaying the note page
        [SerializeField] private TMP_Text customReverseNoteTextUI = null; // TextMeshPro component for main note text

        [Header("Custom Reverse Pop-out Settings")]
        [SerializeField] private GameObject customReverseNoteTextPanelBG = null; // Background panel for flip-side text
        [SerializeField] private Image customReverseNoteTextImage = null; // Background image for flip-side text
        [SerializeField] private TMP_Text customReverseFlipNoteTextUI = null; // TextMeshPro component for flip-side text

        // Reference to the controller managing custom reverse note logic
        public CustomReverseNoteController noteController { get; set; }

        // Singleton instance of the CustomReverseNoteUIManager
        public static CustomReverseNoteUIManager instance;

        private void Awake()
        {
            if (instance == null) { instance = this; } // Set this as the singleton instance
        }

        // Initializes the main note UI with specified image, text, and formatting
        public void ReverseCustomInitialiseMainNote(Sprite pageImage, Vector2 pageScale, string noteReverseText, Vector2 mainTextAreaScale, int mainTextSize,
            TMP_FontAsset mainFontType, Color mainFontColor)
        {
            DisplayPage(pageImage); // Set the initial page image
            DisableNoteDisplay(true); // Show the main note UI

            customReverseNotePageUI.rectTransform.sizeDelta = pageScale; // Adjust the note page size
            customReverseNoteTextUI.text = noteReverseText; // Set the main note text

            customReverseNoteTextUI.rectTransform.sizeDelta = mainTextAreaScale; // Set the main text area size
            customReverseNoteTextUI.fontSize = mainTextSize; // Set the main text font size
            customReverseNoteTextUI.font = mainFontType; // Set the main text font type
            customReverseNoteTextUI.color = mainFontColor; // Set the main text font color
        }

        // Initializes the flip-side text panel with specified background, text, and formatting
        public void ReverseCustomInitialiseFlipSide(Color flipTextBGColor, Vector2 flipTextAreaScale, string noteReverseText, int flipTextSize, TMP_FontAsset flipFontType,
            Color flipFontColor, Vector2 flipTextBGScale)
        {
            customReverseNoteTextImage.color = flipTextBGColor; // Set the flip-side background color

            customReverseFlipNoteTextUI.rectTransform.sizeDelta = flipTextAreaScale; // Set the flip-side text area size
            customReverseFlipNoteTextUI.text = noteReverseText; // Set the flip-side text

            customReverseFlipNoteTextUI.fontSize = flipTextSize; // Set the flip-side text font size
            customReverseFlipNoteTextUI.font = flipFontType; // Set the flip-side text font type
            customReverseFlipNoteTextUI.color = flipFontColor; // Set the flip-side text font color

            customReverseNoteTextImage.rectTransform.sizeDelta = flipTextBGScale; // Set the flip-side background size
        }

        // Updates the note page image
        public void DisplayPage(Sprite pageImage)
        {
            customReverseNotePageUI.sprite = pageImage; // Set the image component to the provided sprite
        }

        // Updates the text content for both main and flip-side text
        public void FillNoteText(string noteText)
        {
            customReverseNoteTextUI.text = noteText; // Set the main note text
            customReverseFlipNoteTextUI.text = noteText; // Set the flip-side text
        }

        // Toggles the visibility of the main note UI
        public void DisableNoteDisplay(bool active)
        {
            customReverseMainNoteUI.SetActive(active);
        }

        // Toggles the visibility of the flip-side text panel
        public void SetReverseNoteAction(bool active)
        {
            customReverseNoteTextPanelBG.SetActive(active);
        }

        // Toggles the visibility of the page navigation buttons container
        public void ShowPageButtons(bool shouldShow)
        {
            pageButtons.SetActive(shouldShow); // Show or hide the page buttons
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

        // Triggers the reverse note action (e.g., toggling the flip-side panel)
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