using UnityEngine;
using UnityEngine.Events;

namespace AdventurePuzzleKit.GasMaskSystem
{
    // Manages the player's health while exposed to gas and handles regeneration and death
    public class GasMaskHealthManager : MonoBehaviour
    {
        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true; // Option to persist this object between scenes

        [Header("Health Variables")]
        [Range(0, 100)][SerializeField] private float currentHealth = 100.0f; // Player's current health
        [Range(0, 100)][SerializeField] private float maxHealth = 100.0f; // Maximum possible health
        [SerializeField] private float healthFall = 2; // Health lost per second while in gas

        [Header("Health Regeneration Variables")]
        [SerializeField] private float regenerationDelay = 1.0f; // Delay before regeneration starts
        [SerializeField] private float regenerationSpeed = 50.0f; // Health regained per second

        [Header("Death Event")]
        [SerializeField] private UnityEvent onDeath = null; // Event triggered when health reaches zero

        private float currentHealthTimer = 1.0f; // Countdown timer before health starts regenerating
        private bool regenHealth = false; // Flag that enables or disables regeneration

        public static GasMaskHealthManager instance; // Singleton instance

        void Awake()
        {
            // Prevent duplicates and persist if enabled
            if (instance != null)
            {
                Destroy(gameObject); // Destroy if already exists
            }
            else
            {
                instance = this; // Set singleton instance
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject); // Keep across scenes
                }
            }

            // Initialize regeneration timer
            currentHealthTimer = regenerationDelay;
        }

        public void Update()
        {
            HandleHealthRegeneration(); // Run regeneration logic each frame
        }

        private void HandleHealthRegeneration()
        {
            // Skip if not regenerating or already at full health
            if (!regenHealth || currentHealth >= maxHealth)
            {
                regenHealth = false; // Stop regeneration
                currentHealthTimer = regenerationDelay; // Reset timer
                return;
            }

            // Countdown before regeneration begins
            currentHealthTimer -= Time.deltaTime;

            // If timer finished, start regenerating health
            if (currentHealthTimer <= 0)
            {
                // Increase health but clamp to max
                currentHealth = Mathf.Min(currentHealth + Time.deltaTime * regenerationSpeed, maxHealth);
                UpdateHealthUI(); // Update UI display
                currentHealthTimer = regenerationDelay; // Reset regen delay
            }
        }

        public void ToggleHealthRegeneration(bool on)
        {
            regenHealth = on; // Enable or disable health regeneration
        }

        public void DamageHealth()
        {
            // Subtract health based on fall rate and clamp to 0
            currentHealth = Mathf.Max(currentHealth - healthFall * Time.deltaTime, 0);
            UpdateHealthUI(); // Reflect new health on UI
        }

        public void UpdateHealthUI()
        {
            // Update the health bar or HUD
            AKUIManager.instance.UpdateHealthUI(currentHealth, maxHealth);

            // Trigger death if health reaches 0
            if (currentHealth <= 0)
            {
                Death();
            }
        }

        public void Death()
        {
            onDeath.Invoke(); // Run all methods bound to the death event
        }
    }
}
