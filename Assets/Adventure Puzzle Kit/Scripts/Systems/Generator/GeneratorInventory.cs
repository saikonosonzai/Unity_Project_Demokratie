using UnityEngine;

namespace AdventurePuzzleKit.GeneratorSystem
{
    // Manages the player's fuel inventory and jerrycan status for generators
    public class GeneratorInventory : MonoBehaviour
    {
        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true; // If true, object will survive between scene loads

        [Header("Jerry can OnStart?")]
        [SerializeField] private bool _hasJerrycan; // Determines if the player starts with a jerrycan

        // Internal fuel values
        private float _currentInvFuel = 0; // Current fuel stored
        private float _maximumInvFuel = 100; // Max fuel capacity

        // Public property for current fuel (clamped to max)
        public float currentInvFuel
        {
            get { return _currentInvFuel; }
            set { _currentInvFuel = Mathf.Min(value, _maximumInvFuel); }
        }

        // Public property for max fuel capacity
        public float maximumInvFuel
        {
            get { return _maximumInvFuel; }
            set { _maximumInvFuel = value; }
        }

        // Public property for jerrycan possession
        public bool hasJerrycan
        {
            get { return _hasJerrycan; }
            set { _hasJerrycan = value; }
        }

        public static GeneratorInventory instance; // Singleton instance

        void Awake()
        {
            // Standard singleton pattern
            if (instance != null)
            {
                Destroy(gameObject); // Destroy if duplicate
            }
            else
            {
                instance = this;
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject); // Persist between scenes
                }
            }
        }

        // Called when a jerrycan is collected
        public void CollectedJerrycan(bool shouldAdd, float fuelAmount, float maxFuelAmount)
        {
            hasJerrycan = true; // Flag the jerrycan as collected
            AKUIManager.instance.JerrycanCollected(); // Update UI feedback
            SetFuelAmounts(shouldAdd, fuelAmount, maxFuelAmount); // Apply fuel logic
        }

        // Sets or adds to fuel amount based on flag
        public void SetFuelAmounts(bool shouldAdd, float fuelAmount, float maxFuelAmount)
        {
            if (shouldAdd) currentInvFuel += fuelAmount; // Add fuel
            else currentInvFuel = fuelAmount; // Set fuel directly

            AKUIManager.instance.UpdateInventoryUI(currentInvFuel, maximumInvFuel); // Refresh fuel bar/UI
        }
    }
}

