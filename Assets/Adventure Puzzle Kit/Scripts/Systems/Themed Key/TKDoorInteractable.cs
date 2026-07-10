using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AdventurePuzzleKit.ThemedKey
{
    // Manages interaction with a themed key-locked door, handling key validation, animations, and audio
    public class TKDoorInteractable : MonoBehaviour
    {
        [Header("Key ScriptableObject")]
        [SerializeField] private bool removeKeyAfterUse = false; // Determines if the key is removed from inventory after use
        [SerializeField] private Key keyScriptable = null; // Reference to the specific key ScriptableObject required

        [Header("Animated Key Object")]
        [SerializeField] private GameObject animatedDoorKey = null; // GameObject for the animated key
        [SerializeField] private string keyAnimation = "HeartKey_Anim_Unlock"; // Animation name for key insertion

        [Header("Door - Audio Clips")]
        [SerializeField] private Sound lockedDoorSound = null; // Sound played when the door is locked
        [SerializeField] private Sound insertKeySound = null; // Sound played when the key is inserted

        [Header("Door Opening Sound Delays")]
        [SerializeField] private float keyAudioDelay = 0.5f; // Delay before playing the key insertion sound
        [SerializeField] private float doorOpenDelay = 1.5f; // Delay before triggering the unlock event

        [Header("Animation Event")]
        [SerializeField] private UnityEvent onUnlock = null;

        private Animator anim; // Reference to the Animator component for the key animation
        private Coroutine animationCoroutine; // Tracks the coroutine for playing the animation sequence

        private void Start()
        {
            anim = animatedDoorKey.GetComponent<Animator>(); // Cache the Animator component from the animated key
        }

        // Checks if the player has the required key and processes the door interaction
        public void CheckDoor()
        {
            // Verify if the required key is in the player's inventory
            if (TKInventory.instance._keyList.Contains(keyScriptable))
            {
                // Remove the key from inventory if configured
                if (removeKeyAfterUse)
                {
                    TKInventory.instance.RemoveKey(keyScriptable);
                }

                // Stop any existing animation coroutine to prevent overlap
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }

                gameObject.tag = "Untagged"; // Untag the door to prevent further interactions
                animationCoroutine = StartCoroutine(PlayAnimation()); // Start the animation sequence
            }
            else
            {
                LockedDoorSound(); // Play locked sound if the key is missing
            }
        }

        // Coroutine to play the key animation and audio sequence
        public IEnumerator PlayAnimation()
        {
            animatedDoorKey.SetActive(true); // Show the animated key
            anim.Play(keyAnimation, 0, 0.0f); // Play the key animation from the start

            yield return new WaitForSeconds(keyAudioDelay); // Wait for the key audio delay
            InsertKeySound(); // Play the key insertion sound
            yield return new WaitForSeconds(doorOpenDelay); // Wait for the door open delay

            animatedDoorKey.SetActive(false); // Hide the animated key
            onUnlock.Invoke(); // Trigger the unlock event (e.g., open the door)
        }

        void LockedDoorSound()
        {
            AKAudioManager.instance.Play(lockedDoorSound);
        }

        void InsertKeySound()
        {
            AKAudioManager.instance.Play(insertKeySound);
        }
    }
}