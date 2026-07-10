using UnityEngine;
using System.Collections;

namespace AdventurePuzzleKit
{
    // Allows door to be interacted with using interface-based interaction system
    public class DoorController : MonoBehaviour, IInteractable
    {
        // Two types of doors: one uses animation, the other rotates via code
        public enum DoorType { Animation, CodeDriven }

        [Header("Door Type")]
        [SerializeField] private DoorType doorType = DoorType.Animation; // Choose how the door should open

        [Header("Use Self Rotation")]
        [SerializeField] private bool rotateSelf = true; // If true, rotate this object; otherwise rotate doorObject

        [Header("Door Object (Optional)")]
        [SerializeField] private GameObject doorObject; // Optional: assign a different door GameObject to rotate

        [Header("Door Animation Name")]
        [SerializeField] private string animationName = "OpenDoor"; // Name of the animation to play when opening

        [Header("Door Audio")]
        [SerializeField] private Sound soundClip = null; // Optional sound to play when door opens

        [Header("Code-Driven Rotation")]
        [SerializeField] private Vector3 rotationAxis = Vector3.up; // Axis to rotate around (usually Y)
        [SerializeField] private float rotationAngle = -90f; // Amount to rotate
        [SerializeField] private float rotationSpeed = 1.5f; // Speed of rotation

        // Private references
        private Animator doorAnim;
        private Quaternion initialRotation;
        private Quaternion targetRotation;
        private bool isOpen = false;
        private Transform targetTransform; // The actual transform that will rotate

        private void Awake()
        {
            // Decide what object we're rotating: this one or a linked door object
            targetTransform = (rotateSelf || doorObject == null) ? transform : doorObject.transform;

            if (doorType == DoorType.Animation)
            {
                // Try to get Animator if we're using an animated door
                if (!targetTransform.TryGetComponent(out doorAnim))
                {
                    Debug.LogWarning($"Animator component missing on {targetTransform.gameObject.name}, but DoorType is set to Animation.");
                }
            }
            else
            {
                // For code-driven doors, save initial rotation for toggling
                initialRotation = targetTransform.rotation;
            }
        }

        public void StartLooking() { } // Called when player starts looking at the door (optional use)

        public void StopInteraction() { } // Called when player stops interacting (optional use)

        public void HandleInputClick() { OpenDoor(); } // Called when player presses the interact key

        public void HandleInputHold() { } // Called if player is holding the interact button

        public void HandleInputStop() { } // Called when player releases the interact button

        // Open the door (play animation or rotate via code)
        public void OpenDoor()
        {
            if (doorType == DoorType.Animation)
            {
                PlayAnimation();
            }
            else
            {
                StopAllCoroutines(); // Stop any previous rotation
                StartCoroutine(RotateDoor());
            }
        }

        // Plays door open animation and sound
        private void PlayAnimation()
        {
            AKAudioManager.instance.Play(soundClip);
            doorAnim.Play(animationName, 0, 0.0f); // Play from the start
        }

        // Smoothly rotates the door over time
        private IEnumerator RotateDoor()
        {
            // Determine target rotation based on open/close state
            targetRotation = isOpen ? initialRotation : initialRotation * Quaternion.Euler(rotationAxis * rotationAngle);

            Quaternion startRotation = targetTransform.rotation;

            AKAudioManager.instance.Play(soundClip);

            float duration = 1f / rotationSpeed; // Convert speed into duration
            float elapsedTime = 0f;

            // Interpolate rotation over time
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                targetTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                yield return null;
            }

            // Ensure exact final rotation and toggle state
            targetTransform.rotation = targetRotation;
            isOpen = !isOpen;
        }
    }
}
