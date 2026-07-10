using UnityEngine;

namespace AdventurePuzzleKit
{
    // Manages disabling/enabling of player controls and camera effects for interactions
    public class AKDisableManager : MonoBehaviour
    {
        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true; // Determines if this GameObject persists between scene loads

        [Header("First Person Fields")]
        [SerializeField] private bool isFirstPerson = false; // Indicates if using first-person perspective
        [SerializeField] private AKFPSController fpsController = null; // Reference to first-person controller

        [Header("Main Camera Fields")]
        [SerializeField] private AKInteractor akInteractor = null; // Handles player interaction with objects
        [SerializeField] private AKCameraZoom cameraZoom = null; // Controls camera zoom functionality
        [SerializeField] private BlurOptimized blur = null; // Manages blur effect during interactions

        [Header("Cursor Settings")]
        [SerializeField] private bool keepCursorVisible = false; // Determines if cursor remains visible

        public static AKDisableManager instance; // Singleton instance for global access

        private void Awake()
        {
            // Ensure only one instance exists
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            // Make object persistent if specified
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            // Set initial cursor state and validate component references
            SetCursorState(keepCursorVisible);
            ValidateFields();
        }

        // Controls player input and UI state during interactions
        public void DisablePlayerDefault(bool disable, bool isInteracting, bool isExamine)
        {
            // Update interaction and UI components
            akInteractor.enabled = !disable;
            SetCursorState(disable);
            GameState.IsUsingSystem = isInteracting;
            AKUIManager.instance.ShowCrosshair(!disable);
            AKUIManager.instance.SetHighlightName(null, !disable, null, null, null, null);
            AKPromptManager.Instance.ClearPrompts(); // Remove any active UI prompts

            // Apply appropriate camera and player settings based on state
            if (disable)
            {
                SetCameraAndPlayerState(true, isExamine);
            }
            else
            {
                SetCameraAndPlayerState(false, isExamine);
            }
        }

        // Configures camera effects and player controls based on interaction state
        private void SetCameraAndPlayerState(bool disable, bool isExamine)
        {
            // Toggle camera zoom functionality
            if (cameraZoom != null)
            {
                cameraZoom.enabled = !disable;
            }

            // Enable blur effect during examination when disabled
            if (blur != null)
            {
                blur.enabled = disable && isExamine;
            }

            // Update first-person controller state if applicable
            if (isFirstPerson && fpsController != null)
            {
                fpsController.SetPlayerDisableMode(disable);
            }
        }

        // Manages cursor visibility and lock state
        private void SetCursorState(bool isVisible)
        {
            Cursor.visible = isVisible;
            Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        // Validates that all required fields are assigned and logs warnings for missing ones
        private void ValidateFields()
        {
            CheckFieldAssigned(akInteractor, nameof(akInteractor));
            CheckFieldAssigned(cameraZoom, nameof(cameraZoom));
            CheckFieldAssigned(blur, nameof(blur));
            CheckFieldAssigned(fpsController, nameof(fpsController));
        }

        // Checks if a field is assigned and logs a warning if not
        private void CheckFieldAssigned(Object field, string fieldName)
        {
            if (field == null)
            {
                Debug.LogWarning($"{fieldName} is not assigned in {nameof(AKDisableManager)} on GameObject '{gameObject.name}'.");
            }
        }
    }
}