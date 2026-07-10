using UnityEngine;
using UnityEngine.UI;

namespace AdventurePuzzleKit.NoteSystem
{
    // Manages the UI for displaying and interacting with notes in the game
    public class BasicNoteUIManager : MonoBehaviour
    {
        // UI element for audio prompt display
        [Header("Audio Prompt UI")]
        [SerializeField] private GameObject audioPromptUI = null;

        // UI elements for page navigation buttons
        [Header("Page Buttons UI")]
        [SerializeField] private GameObject pageButtons = null; // Container for page navigation buttons
        [SerializeField] private GameObject nextButton = null; // Button to navigate to the next page
        [SerializeField] private GameObject previousButton = null; // Button to navigate to the previous page

        // UI elements for the note display
        [Header("Default Note UI")]
        [SerializeField] private GameObject basicNoteMainUI = null; // Main UI canvas for the note
        [SerializeField] private Image basicNotePageUI = null; // Image component for displaying the note page

        // Reference to the note controller managing note logic
        public BasicNoteController noteController { get; set; }

        // Singleton instance of the BasicNoteUIManager
        public static BasicNoteUIManager instance;

        // Called when the object is initialized
        private void Awake()
        {
            if (instance == null) { instance = this; } // Set this as the singleton instance
        }

        // Initializes the note UI with the specified page image and scale
        public void BasicNoteInitialize(Sprite pageImage, Vector2 noteScale)
        {
            DisplayPage(pageImage); // Set the initial page image
            basicNotePageUI.rectTransform.sizeDelta = noteScale; // Adjust the note's size
            DisableNoteDisplay(true); // Show the note UI
        }

        // Toggles the visibility of the main note UI
        public void DisableNoteDisplay(bool active)
        {
            basicNoteMainUI.SetActive(active);
        }

        // Updates the note page image
        public void DisplayPage(Sprite pageImage)
        {
            basicNotePageUI.sprite = pageImage; // Set the image component to the provided sprite
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
        public void ShowPageButtons(bool show)
        {
            pageButtons.SetActive(show);
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