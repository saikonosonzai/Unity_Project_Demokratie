using System.Collections;
using UnityEngine;

namespace AdventurePuzzleKit.GasMaskSystem
{
    // Controls all gas mask behavior: equipping, filter use, breathing states, audio, and UI
    public class GasMaskController : MonoBehaviour
    {
        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true; // Keep this object when loading new scenes

        // Defines the possible breathing states
        public enum GasMaskState { GasMaskOn, GasMaskOffInSmoke, GasMaskOffOutOfSmoke }
        private GasMaskState _gasMaskState; // Current breathing state

        [Header("Gas Mask Features")]
        [SerializeField] private float maxEquipTimer = 1f; // Time needed to equip/unequip the mask
        private float maskBeforeTimer = 0.99f; // Used to cancel equip just before it completes
        private bool hasGasMask = false; // Whether the player has collected a gas mask
        private float equipTimer = 1f; // Timer for current equip/unequip action
        private bool puttingOn = false; // True while mask is being put on
        private bool pullingOff = false; // True while mask is being removed

        [Header("Player")]
        [SerializeField] private AKFPSController player = null; // Reference to player movement controller

        [Header("Movement Speeds")]
        [SerializeField] private float walkNorm = 3; // Normal walking speed
        [SerializeField] private float runNorm = 5; // Normal running speed
        [SerializeField] private float walkGas = 1; // Speed while choking (reduced)

        [Header("Filter Options")]
        [Range(0, 20)][SerializeField] private float filterFallRate = 2f; // Rate at which the filter depletes
        [Range(0, 100)][SerializeField] private int warningPercentage = 20; // When to warn the player about low filter

        [Range(0, 100)][SerializeField] private float _filterTimer = 100f; // Current remaining filter value
        private bool hasFilter = true; // Whether filter is currently active
        private bool filterChanged = false; // Prevents warning sound/UI from repeating
        [SerializeField] private int _maskFilters = 0; // Number of backup filters in inventory

        [Header("Human Audio")]
        [SerializeField] private Sound deepBreathAudio = null; // Played when regaining breath
        [SerializeField] private Sound breathInAudio = null; // Played when equipping mask
        [SerializeField] private Sound breathOutAudio = null; // Played when removing mask
        [SerializeField] private Sound breathingFullAudio = null; // Idle breathing with mask on
        [SerializeField] private Sound chokingAudio = null; // Played when choking in gas

        [Header("Gas Mask Audio")]
        [SerializeField] private Sound pickupAudio = null; // Played when picking up mask or filter
        [SerializeField] private Sound replaceFilterAudio = null; // Played when filter is replaced
        [SerializeField] private Sound warningAudio = null; // Played when filter is low

        private bool canBreath = true; // Tracks whether player can currently breathe
        private bool playOnce = false; // Used to prevent audio spam
        private bool shouldUpdateEquip = false; // True if equip input was released early
        private bool shouldUpdateFilter = false; // True if filter input was released early
        private bool isGasMaskEquipped = false; // True when mask is currently being worn
        private float warningFilterTimerValue; // Filter level that triggers low filter warning

        public float maxFilterTimer { get; set; } = 100f; // Max possible filter time

        // Public property to get/set filter timer
        public float filterTimer
        {
            get { return _filterTimer; }
            set { _filterTimer = value; }
        }

        // Public property to get/set number of backup filters
        public int maskFilters
        {
            get { return _maskFilters; }
            set { _maskFilters = value; }
        }

        public static GasMaskController instance; // Static reference for easy access

        void Awake()
        {
            // Handle singleton pattern and optional persistence
            if (instance != null)
            {
                Destroy(gameObject); // Prevent duplicates
            }
            else
            {
                instance = this; // Set singleton reference
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject); // Persist across scenes
                }
            }
        }

        void Start()
        {
            // Initialize filter and equip timers
            filterTimer = maxFilterTimer;
            equipTimer = maxEquipTimer;
            maskBeforeTimer = maxEquipTimer - 0.01f;

            // Update UI with current number of filters
            string filterAmount = maskFilters.ToString("0");
            AKUIManager.instance.UpdateFilterUI(AKUIManager.FilterState.FilterNumber, filterAmount, 0);

            // Calculate when to trigger filter warning
            warningFilterTimerValue = (maxFilterTimer / 100f) * warningPercentage;
        }

        void Update()
        {
            // Run all core systems each frame
            EquippingGasMask(); // Check mask toggle input
            EquippingFilter();  // Check filter swap input
            FilterUsage();      // Drain filter if mask is on
            SetGasMaskState();  // Update state + audio
        }

        void PlayerMovement(bool slowPlayer)
        {
            ///UPDATE THIS METHOD IF YOU WANT TO USE YOUR OWN CHARACTER CONTROLLER - AS IT WILL NEED LOGIC TO SLOW THE PLAYER WHEN INSIDE THE GAS

            // Slows or restores player movement
            if (slowPlayer)
            {
                player.SetMovementSpeeds(walkGas, 1); // No sprinting
            }
            else
            {
                player.SetMovementSpeeds(walkNorm, runNorm); // Normal speed
            }
        }

        void EquippingGasMask()
        {
            // Check input state
            bool equipHeld = Input.GetKey(AKInputManager.instance.equipMaskKey);
            bool equipReleased = Input.GetKeyUp(AKInputManager.instance.equipMaskKey);
            bool canEquip = equipHeld && hasFilter && hasGasMask && !puttingOn && !pullingOff;

            // Start equipping the mask
            if (canEquip && !isGasMaskEquipped)
            {
                shouldUpdateEquip = false;
                equipTimer -= Time.deltaTime;
                AKUIManager.instance.EnableRadialIndicatorUI(equipTimer);

                // Finish equip
                if (equipTimer <= 0)
                {
                    equipTimer = maxEquipTimer;
                    AKUIManager.instance.DisableRadialIndicatorUI(maxEquipTimer);
                    StartCoroutine(MaskOn()); // Play effects
                    StartCoroutine(Wait());   // Input delay
                }
            }
            // Start removing the mask
            else if (canEquip && isGasMaskEquipped)
            {
                shouldUpdateEquip = false;
                equipTimer -= Time.deltaTime;
                AKUIManager.instance.EnableRadialIndicatorUI(equipTimer);

                // Finish unequip
                if (equipTimer <= 0)
                {
                    equipTimer = maxEquipTimer;
                    AKUIManager.instance.DisableRadialIndicatorUI(maxEquipTimer);
                    pullingOff = true;
                    MaskOff(); // Play remove effects
                    StartCoroutine(Wait());
                }
            }
            // Cancel equip/unequip if input is released
            else if (shouldUpdateEquip)
            {
                equipTimer += Time.deltaTime;
                AKUIManager.instance.EnableRadialIndicatorUI(equipTimer);

                if (equipTimer >= maskBeforeTimer)
                {
                    equipTimer = maxEquipTimer;
                    AKUIManager.instance.DisableRadialIndicatorUI(maxEquipTimer);
                    shouldUpdateEquip = false;
                    StartCoroutine(Wait());
                }
            }

            // Track early release
            if (equipReleased) shouldUpdateEquip = true;
        }

        void EquippingFilter()
        {
            // Do nothing if no gas mask
            if (!hasGasMask) return;

            // Holding filter key and filters available
            if (Input.GetKey(AKInputManager.instance.replaceFilterKey) && _maskFilters >= 1)
            {
                shouldUpdateFilter = false;
                equipTimer -= Time.deltaTime;
                AKUIManager.instance.EnableRadialIndicatorUI(equipTimer);

                if (equipTimer <= 0)
                {
                    equipTimer = maxEquipTimer;
                    AKUIManager.instance.DisableRadialIndicatorUI(maxEquipTimer);
                    ReplaceFilter(); // Apply filter
                }
            }
            // Cancel or refill progress if released
            else if (shouldUpdateFilter)
            {
                equipTimer += Time.deltaTime;
                AKUIManager.instance.EnableRadialIndicatorUI(equipTimer);

                if (equipTimer >= maxEquipTimer)
                {
                    equipTimer = maxEquipTimer;
                    AKUIManager.instance.DisableRadialIndicatorUI(maxEquipTimer);
                    shouldUpdateFilter = false;
                }
            }

            // Input released
            if (Input.GetKeyUp(AKInputManager.instance.replaceFilterKey)) shouldUpdateFilter = true;
        }

        void FilterUsage()
        {
            // Do not drain filter if inventory is open
            if (GameState.IsInventoryOpen) return;

            // Active gas mask
            if (hasGasMask && isGasMaskEquipped)
            {
                filterTimer -= Time.deltaTime * filterFallRate; // Reduce filter over time
                float fill = filterTimer / maxFilterTimer;

                AKUIManager.instance.UpdateFilterUI(AKUIManager.FilterState.FilterValue, null, fill);

                if (filterTimer <= 1)
                {
                    if (_maskFilters >= 1) ReplaceFilter(); // Auto swap
                    else
                    {
                        filterTimer = 0;
                        hasFilter = false;
                        MaskOff(); // No filter = remove mask
                        AKUIManager.instance.UpdateFilterUI(AKUIManager.FilterState.FilterValue, null, fill);
                    }
                }

                // Trigger low filter warning
                if (filterTimer <= warningFilterTimerValue && !filterChanged)
                {
                    AKUIManager.instance.UpdateFilterUI(AKUIManager.FilterState.FilterAlarm, null, 0);
                    AKAudioManager.instance.Play(warningAudio);
                    filterChanged = true;
                }
            }
        }

        void SetGasMaskState()
        {
            // Set state based on current condition
            if (isGasMaskEquipped) _gasMaskState = GasMaskState.GasMaskOn;
            else _gasMaskState = canBreath ? GasMaskState.GasMaskOffOutOfSmoke : GasMaskState.GasMaskOffInSmoke;

            // Apply state effects
            switch (_gasMaskState)
            {
                case GasMaskState.GasMaskOffOutOfSmoke:
                    if (playOnce)
                    {
                        AKAudioManager.instance.StopPlaying(chokingAudio);
                        AKAudioManager.instance.Play(deepBreathAudio);
                        playOnce = false;
                    }
                    break;

                case GasMaskState.GasMaskOffInSmoke:
                    if (!playOnce)
                    {
                        AKAudioManager.instance.StopPlaying(deepBreathAudio);
                        AKAudioManager.instance.Play(chokingAudio);
                        playOnce = true;
                    }
                    GasMaskHealthManager.instance.DamageHealth(); // Take damage over time
                    break;

                case GasMaskState.GasMaskOn:
                    AKAudioManager.instance.StopPlaying(chokingAudio);
                    AKAudioManager.instance.StopPlaying(deepBreathAudio);
                    break;
            }
        }

        public void PickupGasMask()
        {
            if (hasGasMask) return;

            hasGasMask = true;
            AKUIManager.instance.GasMaskCollected();
            AKUIManager.instance.FilterCollected();
            AKAudioManager.instance.Play(pickupAudio);
            AKUIManager.instance.UpdateMaskUI(AKUIManager.MaskUIState.MaskNormal);
        }

        public void PickupFilter()
        {
            _maskFilters++;
            AKUIManager.instance.FilterCollected();
            AKAudioManager.instance.Play(pickupAudio);

            string filterAmount = maskFilters.ToString("0");
            AKUIManager.instance.UpdateFilterUI(AKUIManager.FilterState.FilterNumber, filterAmount, 0);
        }

        void ReplaceFilter()
        {
            _maskFilters--;
            filterTimer = maxFilterTimer;
            hasFilter = true;
            filterChanged = false;

            AKAudioManager.instance.Play(replaceFilterAudio);

            string filterAmount = maskFilters.ToString("0");
            float fill = filterTimer / maxFilterTimer;

            AKUIManager.instance.UpdateFilterUI(AKUIManager.FilterState.FilterNormal, null, 0);
            AKUIManager.instance.UpdateFilterUI(AKUIManager.FilterState.FilterNumber, filterAmount, 0);
            AKUIManager.instance.UpdateFilterUI(AKUIManager.FilterState.FilterValue, null, fill);
        }

        public void DamageGas()
        {

        }

        public void EnableBreathing()
        {

        }

        IEnumerator MaskOn()
        {
            return null;
        }

        void MaskOff()
        {

        }

        IEnumerator Wait()
        {
            if (!isGasMaskEquipped) pullingOff = true;
            else puttingOn = true;

            yield return new WaitForSeconds(2.5f); // Delay before next input allowed

            puttingOn = pullingOff = false;

        }
        
    }
    
}
