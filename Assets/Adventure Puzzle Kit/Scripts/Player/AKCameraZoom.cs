using UnityEngine;

namespace AdventurePuzzleKit
{
    public class AKCameraZoom : MonoBehaviour
    {
        [Header("Zoom Settings")]
        [SerializeField] private float zoomedFOV = 30f; // FOV (Field of View) when zoomed in
        [SerializeField] private float zoomSpeed = 10f; // Speed at which the camera zooms
        [SerializeField] private float defaultFOV = 60f; // Normal FOV when not zoomed

        [Header("Input Settings")]
        [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1; // Zoom input key (default is right mouse button)

        private Camera playerCamera; // Reference to the camera component
        private bool isZooming = false; // Whether we're currently zooming

        private void Awake()
        {
            // Get the Camera component on the same GameObject
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
            {
                Debug.LogError("CameraZoomController requires a Camera component on the same GameObject.");
            }
        }

        private void Update()
        {
            // Skip zooming if the player is busy (e.g., interacting, paused, etc.)
            if (GameState.IsPlayerBusy)
            {
                isZooming = false;
                return;
            }

            HandleZoom(); // Check input and apply zoom
        }

        private void HandleZoom()
        {
            // Check if zoom key was pressed
            if (Input.GetKeyDown(zoomKey))
            {
                isZooming = true;
            }
            // Check if zoom key was released
            else if (Input.GetKeyUp(zoomKey))
            {
                isZooming = false;
            }

            // Smoothly interpolate camera FOV between default and zoomed values
            float targetFOV = isZooming ? zoomedFOV : defaultFOV;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        }

        // Public method to manually control zoom from other scripts
        public void SetZoomState(bool zoom)
        {
            isZooming = zoom;
        }
    }
}
