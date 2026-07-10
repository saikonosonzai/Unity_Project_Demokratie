using UnityEngine;

namespace AdventurePuzzleKit.FuseSystem
{
    // Manages the player's fuse inventory and updates the UI
    public class FuseInventory : MonoBehaviour
    {
        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true; // Keep this object across scene loads

        [Header("Fuses in Inventory")]
        [SerializeField] private int _inventoryFuses; // Current number of fuses held

        // Public property for accessing and modifying fuse count
        public int inventoryFuses
        {
            get { return _inventoryFuses; }
            set { _inventoryFuses = value; }
        }

        public static FuseInventory instance; // Singleton reference

        private void Awake()
        {
            // Singleton setup with optional persistence
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

        // Add one fuse and update UI
        public void AddFuse()
        {
            inventoryFuses++;
            AKUIManager.instance.FuseCollected();
            AKUIManager.instance.UpdateFuseCountUI(_inventoryFuses);
        }

        // Remove one fuse and update UI
        public void RemoveFuse()
        {
            inventoryFuses--;
            AKUIManager.instance.UpdateFuseCountUI(_inventoryFuses);
        }
    }
}
