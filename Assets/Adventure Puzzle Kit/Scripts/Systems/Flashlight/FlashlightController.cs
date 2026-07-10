using UnityEngine;

namespace AdventurePuzzleKit.FlashlightSystem
{
    // Controls flashlight behavior, including UI, input, battery use, and scene persistence
    public class FlashlightController : MonoBehaviour
    {
        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true; // Keep flashlight across scenes

        [Header("Flashlight On Start")]
        [SerializeField] private bool hasFlashlight = false; // If player starts with flashlight

        [Header("Inventory Toggle")]
        [SerializeField] private bool showFlashlightInventory = false; // Show flashlight in inventory?

        [Header("Infinite Flashlight")]
        [SerializeField] private bool infiniteFlashlight = false; // If true, battery is ignored

        [Header("Battery Parameters")]
        [SerializeField] private float batteryDrainAmount = 0.01f; // Rate of battery drain
        [SerializeField] private int batteryCount = 0; // Number of extra batteries held

        [Header("Battery Reload Timers")]
        [SerializeField] private float replaceBatteryTimer = 1.0f; // Time to reload battery
        [SerializeField] private float maxReplaceBatteryTimer = 1.0f; // Max timer reset value

        [Header("Flashlight Parameters")]
        [Range(0, 10)][SerializeField] private float maxFlashlightIntensity = 1.0f; // Max light intensity
        [Range(1, 10)][SerializeField] private int flashlightRotationSpeed = 2; // Speed of flashlight movement

        [Header("Main Flashlight References")]
        [SerializeField] private Light flashlightSpot = null; // Light component for flashlight
        [SerializeField] private FlashlightMovement flashlightMovement = null; // Handles flashlight movement

        [Header("Sounds")]
        [SerializeField] private Sound flashlightPickup = null;
        [SerializeField] private Sound flashlightClick = null;
        [SerializeField] private Sound flashlightReload = null;

        private bool shouldUpdate = false;
        private bool isFlashlightOn;

        public static FlashlightController instance;

        private void Awake()
        {
            // Singleton setup and persistence
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

        void Start()
        {
            // Set defaults for flashlight and movement
            flashlightSpot.intensity = maxFlashlightIntensity;
            flashlightMovement.speed = flashlightRotationSpeed;
            maxReplaceBatteryTimer = replaceBatteryTimer;

            // Warn user if flashlight is hidden but not infinite
            if (!showFlashlightInventory && !infiniteFlashlight)
            {
                print("You may want to make the flashlight infinite if you're not showing the flashlight UI!");
            }

            // Handle initial UI visibility
            if (!showFlashlightInventory)
            {
                AKUIManager.instance.ToggleFlashlightUI(false);
                AKUIManager.instance.disableFlashlightUI = true;
            }
            else
            {
                AKUIManager.instance.UpdateBatteryCountUI(batteryCount);
            }

            // If starting with flashlight, update UI
            if (hasFlashlight)
            {
                UpdateUIElements(true, true);
            }
        }

        // Updates flashlight and battery UI states
        private void UpdateUIElements(bool UpdateBatteryCount = false, bool isFlashlight = false)
        {
            if (showFlashlightInventory)
            {
                if (isFlashlight)
                {
                    AKUIManager.instance.FlashlightCollected();
                }

                if (UpdateBatteryCount)
                {
                    AKUIManager.instance.BatteryCollected();
                    AKUIManager.instance.UpdateBatteryCountUI(batteryCount);
                }

                AKUIManager.instance.FlashlightIndicatorColor(isFlashlightOn);
            }
        }

        void Update()
        {
            // Skip if player is doing something else
            if (GameState.IsPlayerBusy) return;

            // Handle input and battery logic
            if (hasFlashlight)
            {
                PlayerInput();
                DrainBattery();
            }
        }

        // Input handling for flashlight and battery use
        void PlayerInput()
        {
            if (Input.GetKeyDown(AKInputManager.instance.flashlightSwitch))
            {
                FlashlightSwitch();
            }

            if (!infiniteFlashlight)
            {
                if (Input.GetKey(AKInputManager.instance.reloadBattery) && batteryCount >= 1)
                {
                    ReplaceBattery();
                }
                else
                {
                    CoolDownTimer();
                }

                if (Input.GetKeyUp(AKInputManager.instance.reloadBattery))
                {
                    shouldUpdate = true;
                }
            }
        }

        // Called when flashlight is picked up
        public void CollectFlashlight()
        {
            hasFlashlight = true;
            FlashlightPickupSound();
            UpdateUIElements(true, true);
        }

        // Called when battery is collected
        public void CollectBattery(int batteries)
        {
            batteryCount += batteries;
            FlashlightPickupSound();
            UpdateUIElements(true);
        }

        // Toggle flashlight on/off
        void FlashlightSwitch()
        {
            isFlashlightOn = !isFlashlightOn;
            flashlightSpot.enabled = isFlashlightOn;
            UpdateUIElements();
            FlashlightClickSound();
        }

        // Drain one battery over time
        void ReplaceBattery()
        {
            shouldUpdate = false;
            replaceBatteryTimer -= Time.deltaTime;

            if (showFlashlightInventory)
            {
                AKUIManager.instance.EnableRadialIndicatorUI(replaceBatteryTimer);
            }

            if (replaceBatteryTimer <= 0)
            {
                batteryCount--;
                UpdateUIElements(true);
                flashlightSpot.intensity = maxFlashlightIntensity;

                if (showFlashlightInventory)
                {
                    AKUIManager.instance.MaximumBatteryLevel(maxFlashlightIntensity);
                }

                FlashlightReloadSound();
                replaceBatteryTimer = maxReplaceBatteryTimer;

                if (showFlashlightInventory)
                {
                    AKUIManager.instance.DisableRadialIndicatorUI(maxReplaceBatteryTimer);
                }
            }
        }

        // Reset battery timer if player cancels recharge
        void CoolDownTimer()
        {
            if (shouldUpdate)
            {
                replaceBatteryTimer += Time.deltaTime;

                if (showFlashlightInventory)
                {
                    AKUIManager.instance.EnableRadialIndicatorUI(replaceBatteryTimer);
                }

                if (replaceBatteryTimer >= maxReplaceBatteryTimer)
                {
                    replaceBatteryTimer = maxReplaceBatteryTimer;

                    if (showFlashlightInventory)
                    {
                        AKUIManager.instance.DisableRadialIndicatorUI(maxReplaceBatteryTimer);
                    }

                    shouldUpdate = false;
                }
            }
        }

        // Reduces intensity while flashlight is on
        void DrainBattery()
        {
            if (!infiniteFlashlight && isFlashlightOn)
            {
                flashlightSpot.intensity = Mathf.Clamp(
                    flashlightSpot.intensity - batteryDrainAmount * Time.deltaTime * maxFlashlightIntensity,
                    0, maxFlashlightIntensity);

                if (showFlashlightInventory)
                {
                    AKUIManager.instance.UpdateBatteryLevelUI(batteryDrainAmount * Time.deltaTime);
                }
            }
        }

        // Plays sound when flashlight or battery is picked up
        void FlashlightPickupSound()
        {
            AKAudioManager.instance.Play(flashlightPickup);
        }

        // Plays sound when toggling flashlight
        void FlashlightClickSound()
        {
            AKAudioManager.instance.Play(flashlightClick);
        }

        // Plays sound when battery is replaced
        void FlashlightReloadSound()
        {
            AKAudioManager.instance.Play(flashlightReload);
        }
    }
}
