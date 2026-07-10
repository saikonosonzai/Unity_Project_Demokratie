using UnityEngine;
using UnityEngine.Events;

namespace AdventurePuzzleKit.ChessSystem
{
    public class ChessPowerManager : MonoBehaviour
    {
        [Header("Total Number of Fuse Boxes")]
        [SerializeField] private int maxFuseBoxCount = 6;          // Total number of fuse boxes needed to power the system
        [SerializeField] private int currentFuseBoxCount = 0;      // How many fuse boxes are currently powered

        [Header("Disabling Interaction")]
        [SerializeField] private bool disablePuzzleAfterUse = false; // Optionally disable the puzzle after it's completed

        [Header("Fuse Box List - To Disable Tags")]
        [SerializeField] private GameObject[] fuseBoxList = null;  // Fuse box objects to remove tag from when complete

        [Header("Power on - Chess pieces correct")]
        [SerializeField] private UnityEvent powerUp = null;        // Event triggered when all fuse boxes are powered

        // Called by fuse boxes when their state changes
        public void UpdateFuseCount(bool fuseBoxPowered)
        {
            // Add or subtract from the fuse count depending on whether the box is powered or not
            currentFuseBoxCount += fuseBoxPowered ? 1 : -1;

            // Clamp to max count and trigger the power-up if complete
            if (currentFuseBoxCount >= maxFuseBoxCount)
            {
                PowerFuseBox(); // Trigger success
                currentFuseBoxCount = maxFuseBoxCount;
            }
        }

        // Removes tags from all fuse boxes (so they can't be interacted with anymore)
        void RemoveTags()
        {
            for (int i = 0; i < fuseBoxList.Length; i++)
            {
                if (fuseBoxList[i].CompareTag("Untagged"))
                    continue;

                fuseBoxList[i].tag = "Untagged";
            }
        }

        // Final activation: disables UI/inventory if needed, removes interaction, and invokes success event
        public void PowerFuseBox()
        {
            if (disablePuzzleAfterUse)
            {
                // Disable fusebox-related UI and interaction if set
                AKUIManager.instance.DisableInventoryFusebox();
                RemoveTags(); // Make fuse boxes non-interactable
            }

            // Trigger whatever event is hooked up (e.g., power a room, light bulbs, etc.)
            powerUp.Invoke();
        }
    }
}
