using System.Collections;
using UnityEngine;

namespace AdventurePuzzleKit.ValveSystem
{
    // Manages a valve slot, handling valve attachment, audio, and UI feedback
    public class ValveSlot : MonoBehaviour
    {
        [Header("Valve ScriptableObject")]
        [SerializeField] private Valve valveScriptable = null;

        [Header("Valve Slot Objects")]
        [SerializeField] private GameObject valveWheel = null; // The valve wheel object to activate when attached
        [SerializeField] private GameObject questionMarkPopout = null; // Optional popout UI (e.g., question mark) for feedback

        [Header("Valve Attach - Audio Clip")]
        [SerializeField] private Sound attachSound = null;

        public bool _AttachSlot { get; set; } = false; // Property indicating if the slot has a valve attached


        // Checks if the required valve is in the inventory and processes attachment
        public void CheckValveSlot()
        {
            // Check if the required valve exists in the player's inventory
            bool valveExistsInInventory = ValveInventory.instance._valvesList.Contains(valveScriptable);

            if (valveExistsInInventory)
            {
                ValveInventory.instance.RemoveValve(valveScriptable); // Remove the valve from inventory
                AKAudioManager.instance.Play(attachSound); // Play the attachment sound
                valveWheel.SetActive(true); // Show the valve wheel
                gameObject.SetActive(false); // Deactivate the slot object
            }
            // If the valve exists and a question mark popout is assigned, show the popout UI
            else if (questionMarkPopout != null && valveExistsInInventory)
            {
                StartCoroutine(ShowValveTextUI()); // Start coroutine to show popout UI temporarily
            }
        }

        // Coroutine to display the question mark popout UI for a short duration
        IEnumerator ShowValveTextUI()
        {
            questionMarkPopout.SetActive(true); // Show the popout UI
            yield return new WaitForSeconds(1); // Wait for 1 second
            questionMarkPopout.SetActive(false); // Hide the popout UI
        }
    }
}