using UnityEngine;

namespace AdventurePuzzleKit.ChessSystem
{
    public class ChessFuseBoxInteractable : MonoBehaviour
    {
        [Header("Fuse Box Type")]
        [SerializeField] private ChessPiece chessPieceScriptable = null; // The correct fuse type that powers this box

        [Header("Started with a fuse?")]
        [SerializeField] private bool fusePlaced;                        // Whether this box starts with a fuse
        [SerializeField] private ChessPiece starterFuseScriptable = null; // The starting fuse (if any)

        [Header("Fuse Spawn Location")]
        [SerializeField] private Transform fuseLocation = null;          // Where the fuse should be spawned
        [SerializeField] private Vector3 spawnOffset = Vector3.zero;     // Position offset for the fuse object
        [SerializeField] private Quaternion fuseRotation = Quaternion.identity; // Rotation for the placed fuse

        [Header("Light Object")]
        [SerializeField] private Renderer fuseBoxLightRend = null;       // The light renderer used for color feedback

        [Header("Power Manager")]
        [SerializeField] private ChessPowerManager powerManager = null;  // Reference to the master manager that tracks fuse status

        [Header("Audio")]
        [SerializeField] private Sound insertFuseSound = null;           // Sound when inserting or removing a fuse

        // Private fields
        private GameObject spawnedFuse;          // The visual fuse object in the scene
        private bool isPowered;                 // Whether this box is currently powered
        private Material fuseBoxLightMaterial;  // Material instance for changing fuse light color

        public ChessPiece currentFuse { get; set; } // The currently placed fuse (if any)

        private void Awake()
        {
            // If this box starts with a fuse, spawn it at the beginning
            if (fusePlaced)
            {
                SpawnFuse(starterFuseScriptable);
            }

            // Cache the material so we can update its color
            fuseBoxLightMaterial = fuseBoxLightRend.material;
        }

        // Called when the player interacts with the fuse box
        public void InteractFuseBox()
        {
            // Opens the inventory UI specifically for the fuse box system
            AKUIManager.instance.OpenInventoryFusebox(this);
        }

        // Check if the currently placed fuse is the correct one
        public void CheckFuseBox(ChessPiece fuseType)
        {
            bool wasPowered = isPowered;

            // Set powered state depending on whether the correct fuse is inserted
            isPowered = fuseType == chessPieceScriptable;

            // Only update fuse count if power state changed
            if (wasPowered != isPowered)
            {
                powerManager.UpdateFuseCount(isPowered);
            }
        }

        // Called when the player places a fuse into the box
        public void PlaceFuse(ChessPiece fuseType)
        {
            if (!fusePlaced)
            {
                fusePlaced = true;
                SpawnFuse(fuseType);                           // Create the visual fuse
                ChessInventory.instance.RemoveChessPiece(fuseType); // Remove it from the inventory
                CheckFuseBox(fuseType);                        // Check if it's the correct one
            }

            // Play audio feedback
            AKAudioManager.instance.Play(insertFuseSound);
        }

        // Instantiates the fuse object in the world and sets it visually
        private void SpawnFuse(ChessPiece fuseType)
        {
            currentFuse = fuseType;

            // Update the light to green when a fuse is placed
            fuseBoxLightMaterial.color = Color.green;

            // Instantiate the fuse model at the specified location
            spawnedFuse = Instantiate(fuseType.ChessPrefab, fuseLocation.transform.position + spawnOffset, Quaternion.identity);
            spawnedFuse.transform.parent = fuseLocation.transform;
            spawnedFuse.transform.rotation = fuseRotation;
        }

        // Called when the player removes a fuse from the box
        public void RemoveFuse(ChessPiece fuseType)
        {
            if (fusePlaced)
            {
                fusePlaced = false;

                // Set light back to red to show it's inactive
                fuseBoxLightMaterial.color = Color.red;

                // Return the fuse to the inventory
                ChessInventory.instance.AddChessPiece(fuseType);

                // Destroy the placed fuse object
                Destroy(spawnedFuse);

                // Update power logic
                CheckFuseBox(null);

                // Play audio feedback
                AKAudioManager.instance.Play(insertFuseSound);
            }
        }

        // Cleanup if this object is destroyed
        private void OnDestroy()
        {
            Destroy(fuseBoxLightRend);
        }
    }
}
