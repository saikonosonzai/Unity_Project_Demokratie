using UnityEngine;

namespace AdventurePuzzleKit
{
    public class AKInputManager : MonoBehaviour
    {
        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true;

        [Header("Raycast / First Person Pickup Inputs")]
        public KeyCode mainInteractionKey;
        public KeyCode examineKey;

        [Header("Trigger Input")]
        public KeyCode triggerInteractKey;

        [Header("Open Inventory Input")]
        public KeyCode toggleInventoryKey;
        public KeyCode closeInventoryKey;

        [Header("Keypad Input")]
        public KeyCode closeKeypadKey;

        [Header("Safe Input")]
        public KeyCode closeSafeKey;

        [Header("Padlock Input")]
        public KeyCode closePadlockKey;

        [Header("GasMask System Inputs")]
        public KeyCode equipMaskKey;
        public KeyCode replaceFilterKey;

        [Header("Note System Inputs")]
        public KeyCode closeNoteKey;
        public KeyCode reverseNoteKey;
        public KeyCode playNoteAudioKey;

        [Header("Examine System Inputs")]
        public KeyCode interactKey;
        public KeyCode rotateKey;
        public KeyCode dropKey;
        public KeyCode pickupItemKey;

        [Header("Flashlight System Inputs")]
        public KeyCode flashlightSwitch;
        public KeyCode reloadBattery;

        public static AKInputManager instance;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
        }
    }
}
