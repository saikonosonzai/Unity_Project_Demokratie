using UnityEngine;

namespace AdventurePuzzleKit
{
    public class AKFPSController : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [SerializeField] private float walkSpeed = 3.0f;              // Normal walking speed
        [SerializeField] private float sprintMultiplier = 2.0f;       // Multiplier when sprinting
        [SerializeField] private float crouchSpeed = 1.5f;            // Slower movement when crouched

        [Header("Jump Parameters")]
        [SerializeField] private float jumpForce = 5.0f;              // Force applied when jumping
        [SerializeField] private float gravity = 9.81f;               // Gravity effect when falling

        [Header("Look Sensitivity")]
        [SerializeField] private float mouseSensitivity = 2.0f;       // Mouse sensitivity
        [SerializeField] private float upDownRange = 80.0f;           // Vertical look limit (degrees)

        [Header("Crouch Settings")]
        [SerializeField] private float crouchHeight = 1.0f;           // Height of character when crouching
        [SerializeField] private float standHeight = 1.9f;            // Height when standing
        [SerializeField] private float crouchTransitionSpeed = 5.0f;  // Speed of crouch animation
        [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl; // Key to toggle crouch

        [Header("Footstep Audio Settings")]
        [SerializeField] private AudioSource playerAudioSource;       // AudioSource used to play player sounds
        [Space(5)]
        [SerializeField] private AudioClip[] footstepSounds;          // Sounds for walking/running
        [SerializeField] private AudioClip jumpSound;                 // Sound played when jumping

        [Header("Footstep Intervals")]
        [SerializeField] private float walkStepInterval = 0.5f;       // Time between steps when walking
        [SerializeField] private float sprintStepInterval = 0.3f;     // Time between steps when sprinting
        [SerializeField] private float crouchStepInterval = 0.7f;     // Time between steps when crouched
        [SerializeField] private float velocityThreshold = 2.0f;      // Min velocity before footstep sound plays

        [Header("Control Toggles")]
        public bool canMove = true;                                   // Toggle for player movement
        public bool canRotate = true;                                 // Toggle for player rotation

        private bool canCrouch = true;        // Used to disable crouching when walk speed is too low
        private bool isCrouching = false;     // Tracks crouch state
        private int lastPlayedIndex = -1;     // Used to avoid repeating the same footstep sound
        private bool isMoving;                // True when player is moving
        private float nextStepTime;           // Timer for footstep sounds
        private Camera mainCamera;            // Reference to main camera
        private float verticalRotation;       // Tracks up/down camera rotation
        private Vector3 currentMovement = Vector3.zero; // Full movement vector (incl. gravity)
        private CharacterController characterController; // CharacterController component

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            mainCamera = Camera.main;
        }

        private void Update()
        {
            // Handle player controls
            if (canMove) HandleMovement();
            if (canRotate) HandleRotation();
            HandleCrouching();
            HandleFootsteps();
        }

        // Allow speeds to be set dynamically (e.g., for slow states)
        public void SetMovementSpeeds(float newWalkSpeed, float newSprintMultiplier)
        {
            walkSpeed = Mathf.Clamp(newWalkSpeed, 0, 10);
            sprintMultiplier = Mathf.Clamp(newSprintMultiplier, 1, 5);

            // If walking speed is very low, disable crouch (e.g., during cutscenes or slow states)
            canCrouch = newWalkSpeed > 1.0f;
        }

        // Handles player input and movement
        void HandleMovement()
        {
            // Determine speed modifiers based on crouch/sprint state
            float speedMultiplier = Input.GetKey(KeyCode.LeftShift) && !isCrouching ? sprintMultiplier : 1f;
            float speed = isCrouching ? crouchSpeed : walkSpeed;

            // Input axes for movement
            float verticalSpeed = 0;
            float horizontalSpeed = 0;

            if (Input.GetKey(KeyCode.W))
            {
                verticalSpeed = 1;
            }

            if (Input.GetKey(KeyCode.S))
            {
                verticalSpeed = -1;
            }

            if (Input.GetKey(KeyCode.D))
            {
                horizontalSpeed = 1;
            }

            if (Input.GetKey(KeyCode.A))
            {
                horizontalSpeed = -1;
            }

            Vector3 horizontalMovement = new Vector3(horizontalSpeed, 0, verticalSpeed);

            // Normalize to prevent faster diagonal movement
            if (horizontalMovement.magnitude > 1)
            {
                horizontalMovement.Normalize();
            }

            // Apply speed and rotation
            horizontalMovement *= speed * speedMultiplier;
            horizontalMovement = transform.rotation * horizontalMovement;

            HandleGravityAndJumping(); // Apply gravity or jumping if needed

            // Combine horizontal and vertical movement
            currentMovement.x = horizontalMovement.x;
            currentMovement.z = horizontalMovement.z;

            // Apply final movement to character controller
            characterController.Move(currentMovement * Time.deltaTime);

            // Determine if player is actively moving
            isMoving = verticalSpeed != 0 || horizontalSpeed != 0;
        }

        // Apply jumping and gravity
        void HandleGravityAndJumping()
        {
            if (characterController.isGrounded)
            {
                currentMovement.y = -0.5f; // Small push to stay grounded

                if (Input.GetButtonDown("Jump"))
                {
                    currentMovement.y = jumpForce;

                    // Play jump sound
                    if (playerAudioSource != null && jumpSound != null)
                    {
                        playerAudioSource.PlayOneShot(jumpSound);
                    }
                }
            }
            else
            {
                // Apply gravity over time
                currentMovement.y -= gravity * Time.deltaTime;
            }
        }

        // Rotate player and camera with the mouse
        void HandleRotation()
        {
            // Horizontal rotation
            float mouseXRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(0, mouseXRotation, 0);

            // Vertical rotation (clamped)
            verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
            mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }

        // Handles crouch toggle and transition
        void HandleCrouching()
        {
            // If crouch is disabled, force standing height
            if (!canCrouch)
            {
                if (isCrouching)
                {
                    isCrouching = false;
                    characterController.height = standHeight;
                }
                return;
            }

            // Toggle crouch state on key press
            if (Input.GetKeyDown(crouchKey))
            {
                isCrouching = !isCrouching;
            }

            // Smoothly transition to target height
            float targetHeight = isCrouching ? crouchHeight : standHeight;
            characterController.height = Mathf.MoveTowards(characterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        }

        // Triggers footstep sounds based on movement and state
        void HandleFootsteps()
        {
            if (GameState.IsInventoryOpen) return; // Don't play footsteps in menus

            // Determine step interval based on crouch/sprint
            float currentStepInterval = isCrouching ? crouchStepInterval : (Input.GetKey(KeyCode.LeftShift) ? sprintStepInterval : walkStepInterval);

            // Adjust velocity sensitivity for crouching
            float adjustedVelocityThreshold = isCrouching ? 0.1f : velocityThreshold;

            // Play footstep sound if grounded, moving, and enough time has passed
            if (characterController.isGrounded && isMoving && Time.time > nextStepTime && characterController.velocity.magnitude > adjustedVelocityThreshold)
            {
                PlayFoostepSounds();
                nextStepTime = Time.time + currentStepInterval;
            }
        }

        // Picks and plays a random footstep sound
        void PlayFoostepSounds()
        {
            int randomIndex;

            if (footstepSounds.Length == 1)
            {
                randomIndex = 0;
            }
            else
            {
                // Avoid repeating the last sound
                randomIndex = Random.Range(0, footstepSounds.Length - 1);
                if (randomIndex >= lastPlayedIndex)
                {
                    randomIndex++;
                }
            }

            lastPlayedIndex = randomIndex;
            playerAudioSource.clip = footstepSounds[randomIndex];
            playerAudioSource.Play();
        }

        // Disables player control when needed (e.g., in cutscenes)
        public void SetPlayerDisableMode(bool active)
        {
            canMove = !active;
            canRotate = !active;
        }
    }
}
