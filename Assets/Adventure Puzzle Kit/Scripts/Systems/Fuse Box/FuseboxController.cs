using UnityEngine;
using UnityEngine.Events;

namespace AdventurePuzzleKit.FuseSystem
{
    // Handles fuse insertion logic, fusebox visuals, sounds, and power activation
    public class FuseboxController : MonoBehaviour
    {
        [Header("Fuse Inserted?")]
        [SerializeField] private bool[] fuseInserted = new bool[4]; // Tracks which fuses are inserted

        [Header("Individual Fuses (Parented to the fusebox)")]
        [SerializeField] private GameObject[] fuseObjects = new GameObject[4]; // Fuse objects to show on insert

        [Header("Fusebox Lights (Parented to the fusebox)")]
        [SerializeField] private GameObject[] lights = new GameObject[4]; // Lights that change color when fuse is inserted

        [Header("Materials (Inside the project folder)")]
        [SerializeField] private Material greenButton = null; // Material used to show fuse is active

        [Header("Sound Effect Scriptables")]
        [SerializeField] private Sound zapSound = null; // Sound to play when inserting or failing to insert

        [Header("Power on - When all fuses are inserted")]
        [SerializeField] private UnityEvent powerUp = null; // Event to trigger when fusebox is complete

        private bool powerOn = false; // Tracks whether power is enabled

        void Start()
        {
            // Restore state of inserted fuses on start
            for (int i = 0; i < fuseInserted.Length; i++)
            {
                if (fuseInserted[i])
                {
                    lights[i].GetComponent<Renderer>().material = greenButton;
                    fuseObjects[i].SetActive(true);
                }
            }
        }

        // Called when inserting a fuse to check if all slots are filled
        void AllFusesInserted()
        {
            if (System.Array.TrueForAll(fuseInserted, inserted => inserted))
            {
                powerOn = true;
                gameObject.tag = "Untagged"; // Prevent re-tagging interaction
                powerUp.Invoke(); // Fire event for full power
            }
        }

        // Called when interacting with fusebox
        public void CheckFuseBox()
        {
            if (FuseInventory.instance.inventoryFuses <= 0 && !powerOn)
            {
                ZapAudio(); // Play sound if no fuses
            }

            if (FuseInventory.instance.inventoryFuses >= 1 && !powerOn)
            {
                // Insert fuse into first available slot
                for (int i = 0; i < fuseInserted.Length; i++)
                {
                    if (!fuseInserted[i])
                    {
                        fuseObjects[i].SetActive(true);
                        fuseInserted[i] = true;
                        lights[i].GetComponent<Renderer>().material = greenButton;
                        FuseInventory.instance.RemoveFuse();
                        ZapAudio();
                        AllFusesInserted();
                        break;
                    }
                }
            }
        }

        // Play zap sound effect
        void ZapAudio()
        {
            AKAudioManager.instance.Play(zapSound);
        }
    }
}
