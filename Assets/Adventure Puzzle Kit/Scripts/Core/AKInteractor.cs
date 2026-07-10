using UnityEngine;

namespace AdventurePuzzleKit
{
    [RequireComponent(typeof(Camera))]
    public class AKInteractor : MonoBehaviour
    {
        [Header("Raycast Features")]
        [SerializeField] private float interactDistance = 2.5f;
        [SerializeField] private string interactableTag = "InteractiveObject"; // Tag for interactable objects

        private AKItem currentTarget;
        private Camera _camera;


        void Start()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            // Skip raycast and input handling when examining
            if (GameState.IsPlayerBusy)
                return;

            HandleRaycast();
            HandleInput();          
        }

        private void HandleRaycast()
        {
            // Perform the raycast
            if (Physics.Raycast(_camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)), transform.forward, out RaycastHit hit, interactDistance))
            {
                // Check for the interactable tag and AKItem
                if (hit.collider.CompareTag(interactableTag) && hit.collider.TryGetComponent(out AKItem akItem))
                {
                    if (akItem != currentTarget)
                    {
                        // If transitioning to a new target, stop interaction with the previous one
                        ClearCurrentTarget();

                        currentTarget = akItem;

                        if (currentTarget != null)
                        {
                            currentTarget.StartLooking();
                            HighlightCrosshair(true);
                        }
                    }
                }
                else
                {
                    // No valid interactable object was hit
                    ClearCurrentTarget();
                }
            }
            else
            {
                // No object was hit at all
                ClearCurrentTarget();
            }
        }

        private void HandleInput()
        {
            if (currentTarget == null) return;

            if (Input.GetKeyDown(AKInputManager.instance.mainInteractionKey)) // 'E' for pickup
                currentTarget.HandleInputClick();

            if (Input.GetKeyDown(AKInputManager.instance.examineKey)) // 'Q' for examine
            {
                currentTarget.HandleExamine();
            }

            if (Input.GetKey(AKInputManager.instance.mainInteractionKey))
                currentTarget.HandleInputHold();

            if (Input.GetKeyUp(AKInputManager.instance.mainInteractionKey))
                currentTarget.HandleInputStop();
        }

        private void ClearCurrentTarget()
        {
            if (currentTarget != null)
            {
                currentTarget.StopInteraction(); // Signal the current target to stop any ongoing interactions
                HighlightCrosshair(false); // De-highlight crosshair
                currentTarget = null; // Clear the reference
            }
        }

        void HighlightCrosshair(bool on)
        {
            AKUIManager.instance.HighlightCrosshair(on);
        }
    }
}
