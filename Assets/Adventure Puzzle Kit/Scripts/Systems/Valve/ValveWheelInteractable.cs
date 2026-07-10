using UnityEngine;
using UnityEngine.Events;

namespace AdventurePuzzleKit.ValveSystem
{
    // Manages interaction with a valve wheel, handling rotation, progress UI, audio, and completion events
    public class ValveWheelInteractable : MonoBehaviour
    {
        [Header("Valve Turn Parameters")]
        [Range(0, 500)][SerializeField] private float valveTurnSpeed = 200f; // Speed at which the valve wheel rotates (degrees per second)
        [Range(0, 5)][SerializeField] private float progressSpeedUI = 0.5f; // Speed at which the progress UI fills

        [Header("Audio Clips")]
        [SerializeField] private Sound valveCreakSound = null; // Sound played while turning the valve

        [Header("Valve Unity Events")]
        [SerializeField] private UnityEvent valveOpened = null;

        private float maxValveLimit = 0.99f; // Maximum fill amount for the progress UI (slightly less than 1 to ensure completion)
        private bool playOnce = true; // Ensures the creak sound plays only once per interaction
        private bool isComplete = false; // Tracks if the valve is fully opened

        // Initiates the valve turning process
        public void StartTurning()
        {
            // Only proceed if the valve is not already complete
            if (!isComplete)
            {
                Debug.Log("Turning valve wheel...", this);
                TurnValveWheel(); // Start turning the valve
            }
        }

        // Stops the valve turning process
        public void StopTurning()
        {
            // Only proceed if the valve is not already complete
            if (!isComplete)
            {
                Debug.Log("Stopped turning valve wheel.", this);
                ReleaseValveWheel(); // Reset progress and stop audio
            }
        }

        // Handles the logic for turning the valve wheel
        void TurnValveWheel()
        {
            // Check if the progress UI is below the maximum limit
            if (AKUIManager.instance.ValveProgressUI.fillAmount <= maxValveLimit)
            {
                Debug.Log($"ValveProgressUI fill amount: {AKUIManager.instance.ValveProgressUI.fillAmount}", this);
                AKUIManager.instance.SliderOpacity(true); // Show the progress UI
                // Rotate the valve wheel around the Z-axis
                gameObject.transform.Rotate(0, 0, valveTurnSpeed * Time.deltaTime, Space.Self);

                // Update the progress UI based on turning speed
                float valveProgress = progressSpeedUI * Time.deltaTime;
                AKUIManager.instance.UpdateValveProgress(valveProgress);

                // Play the creak sound once when turning starts
                if (playOnce)
                {
                    AKAudioManager.instance.Play(valveCreakSound);
                    playOnce = false;
                }

                // Check if the valve is fully opened
                if (AKUIManager.instance.ValveProgressUI.fillAmount >= maxValveLimit)
                {
                    Debug.Log("Valve is fully open", this);
                    AKAudioManager.instance.StopPlaying(valveCreakSound); // Stop the creak sound
                    playOnce = true; // Reset sound trigger
                    AKUIManager.instance.SliderOpacity(false); // Hide the progress UI
                    OnValveOpen(); // Trigger completion logic
                }
            }
            else
            {
                Debug.Log("Valve progress already at max limit", this);
            }
        }

        // Resets the valve progress and stops audio when turning is stopped
        void ReleaseValveWheel()
        {
            AKUIManager.instance.ResetProgress(); // Reset the progress UI
            AKAudioManager.instance.StopPlaying(valveCreakSound); // Stop the creak sound
            playOnce = true; // Reset sound trigger
        }

        // Handles the logic when the valve is fully opened
        void OnValveOpen()
        {
            isComplete = true; // Mark the valve as complete
            gameObject.tag = "Untagged"; // Untag to prevent further interactions
            valveOpened.Invoke(); // Trigger the valve opened event
            // Note: This script should be disabled in the UnityEvent for optimization
        }
    }
}