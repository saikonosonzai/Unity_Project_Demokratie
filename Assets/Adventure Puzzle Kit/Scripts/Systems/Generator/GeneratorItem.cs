/// REMEMBER: This script has a custom editor called "GeneratorItemEditor", found in the "Editor" folder. You will need to add new properties to this
/// if you create new variables / fields in this script. Contact me if you have any troubles at all!

using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace AdventurePuzzleKit.GeneratorSystem
{
    // Represents an interactable generator item (e.g., jerrycan, fuel barrel, or generator) in the game
    public class GeneratorItem : MonoBehaviour, IInteractable
    {
        // Enum to define the type of generator item
        public enum GeneratorItemType { None, Jerrycan, Generator, FuelBarrel }
        // The specific type of this item
        public GeneratorItemType itemType;

        // Fuel Parameters
        [SerializeField] private float fillRate = 1; // Rate at which fuel is added to the generator (units per second)
        [SerializeField] private float burnRate = 1; // Rate at which fuel is consumed when generator is active (units per second)
        [Range(0, 1000)][SerializeField] private float _itemFuelAmount = 100.0f; // Current fuel amount in the item
        [Range(0, 1000)][SerializeField] private float _itemMaxFuelAmount = 500.0f; // Maximum fuel capacity of the item

        // Rumble and Sound
        [SerializeField] private bool _canBurnFuel = false; // Determines if the item can burn fuel (e.g., generator)
        [SerializeField] private bool _canRumble = false; // Determines if the item can rumble (e.g., generator vibration)
        [SerializeField] private float rumbleSpeed = 5.0f; // Speed of the rumble effect
        [SerializeField] private float rumbleIntensity = 0.01f; // Intensity of the rumble effect

        [SerializeField] private Sound interactSound = null; // Sound played on interaction (e.g., picking up jerrycan)
        [SerializeField] private Sound fuelPourSound = null; // Sound played when pouring fuel

        // Unlimited Fuel Barrel
        [SerializeField] private bool infiniteFuelBarrel = false; // If true, fuel barrel provides unlimited fuel

        // UI
        [SerializeField] private bool _showUI = true; // Determines if UI for item stats is shown
        [SerializeField] private PopoutUI _popoutUI = null; // Reference to the UI component for displaying item stats

        // Unity Events triggered when the generator is activated or deactivated
        [SerializeField] private UnityEvent activateGenerator = null;
        [SerializeField] private UnityEvent deactivateGenerator = null;

        // State variables
        private bool isGeneratorOn = false; // Tracks if the generator is currently running
        private bool isFilling = false; // Tracks if the generator is being filled with fuel
        private bool isPouringFuel = false; // Tracks if fuel pouring sound is active
        private bool rumbling = false; // Tracks if the rumble effect is active
        private Vector3 generatorOrigin; // Original position of the generator for rumble effect

        private float keyHoldDelay = 0.5f; // Delay (in seconds) to detect a long press
        private float fuelRequired; // Amount of fuel needed to fill the generator
        private bool isCheckingLongPress = false; // Tracks if long press detection is active
        private float interactionStartTime; // Time when interaction started for long press detection
        private bool updatePopoutUILevels;

        private GeneratorInventory _generatorInventory; // Reference to the player's generator inventory

        // Public properties for accessing private fields
        public bool canBurnFuel
        {
            get { return _canBurnFuel; }
            set { _canBurnFuel = value; }
        }

        public bool canRumble
        {
            get { return _canRumble; }
            set { _canRumble = value; }
        }

        public float itemFuelAmount
        {
            get { return _itemFuelAmount; }
            set { _itemFuelAmount = value; }
        }

        public float itemMaxFuelAmount
        {
            get { return _itemMaxFuelAmount; }
            set { _itemMaxFuelAmount = value; }
        }

        public bool showUI
        {
            get { return _showUI; }
            set { _showUI = value; }
        }

        private void Awake()
        {
            UpdateItemStats(true); // Initialize UI with current item stats
            if (itemType == GeneratorItemType.Generator)
            {
                generatorOrigin = transform.localPosition; // Store initial position for rumble effect
            }
        }

        private void Start()
        {
            _generatorInventory = GeneratorInventory.instance; // Get reference to the singleton inventory
        }

        private void Update()
        {
            if (_canRumble && rumbling)
            {
                RumbleGenerator(); // Apply rumble effect if enabled and active
            }

            if (isGeneratorOn && _canBurnFuel)
            {
                GeneratorFuelBurnLogic(); // Burn fuel if generator is on and can burn fuel
            }

            if (updatePopoutUILevels)
            {
                UpdateItemStats(true);
            }
        }

        // Subsystem Interaction Methods
        public void StartLooking() // Called when the player starts looking at the item
        {
            ShowObjectStats(true);
            updatePopoutUILevels = true;
        }

        // Called when the player stops interacting with the item
        public void StopInteraction()
        {
            // Stop all coroutines and reset interaction states
            StopAllCoroutines();
            isFilling = false;
            if (isPouringFuel)
            {
                StopWaterPour(); // Stop fuel pouring sound if active
            }
            ShowObjectStats(false); // Hide item stats UI
            updatePopoutUILevels = false;
        }

        // Called when the player clicks on the item
        public void HandleInputClick()
        {
            // Handle interaction based on item type
            if (itemType == GeneratorItemType.Jerrycan)
            {
                JerrycanLogic(); // Handle jerrycan pickup
            }
            else if (itemType == GeneratorItemType.FuelBarrel)
            {
                FuelBarrelLogic(); // Handle fuel barrel interaction
            }
            else if (itemType == GeneratorItemType.Generator)
            {
                interactionStartTime = Time.time; // Record start time for long press detection
                isCheckingLongPress = true;
                StartCoroutine(CheckForLongPress()); // Start long-press detection
            }
        }

        // Called when the player holds input on the item
        public void HandleInputHold()
        {
            if (itemType == GeneratorItemType.Generator && isFilling)
            {
                GeneratorFillingLogic(); // Fill generator with fuel if conditions are met
            }
            //UpdateItemStats(true);
        }

        // Called when the player stops holding input
        public void HandleInputStop()
        {
            // Handle end of interaction
            if (isCheckingLongPress)
            {
                // If long press wasn't detected, treat as single-click
                isCheckingLongPress = false;
                if (!isFilling)
                {
                    GeneratorToggleLogic(); // Toggle generator on/off
                }
            }

            StopAllCoroutines();
            isFilling = false;

            if (isPouringFuel)
            {
                StopWaterPour(); // Stop fuel pouring sound
            }
        }

        // Coroutine to detect long press for generator filling
        private IEnumerator CheckForLongPress()
        {
            yield return new WaitForSeconds(keyHoldDelay);

            if (isCheckingLongPress)
            {
                // If key is still held, start filling
                isFilling = true;

                if (DebugSettings.EnableDebugLogs)
                    Debug.Log("Long press detected. Starting generator filling.");
            }
        }

        // Toggle generator on or off
        private void GeneratorToggleLogic()
        {
            if (isGeneratorOn)
            {
                isGeneratorOn = false;
                DeactivateGenerator(); // Turn off generator
            }
            else if (_itemFuelAmount > 0)
            {
                isGeneratorOn = true;
                ActivateGenerator(); // Turn on generator if it has fuel
            }
        }

        // Logic for picking up a jerrycan
        private void JerrycanLogic()
        {
            if (DebugSettings.EnableDebugLogs)
                Debug.Log("Collecting jerrycan.");

            // Add jerrycan to inventory with its fuel stats
            _generatorInventory.CollectedJerrycan(true, itemFuelAmount, itemMaxFuelAmount);
            AudioInteractSound(); // Play interaction sound
            gameObject.SetActive(false); // Deactivate jerrycan after pickup
        }

        // Logic for interacting with a fuel barrel
        private void FuelBarrelLogic()
        {
            // Check if inventory is already full
            if (_generatorInventory.currentInvFuel >= _generatorInventory.maximumInvFuel)
            {
                if (DebugSettings.EnableDebugLogs)
                    Debug.Log("Fuel barrel: inventory is already full.");
                return;
            }

            // If player has jerrycan and barrel has fuel
            if (_generatorInventory.hasJerrycan && itemFuelAmount > 0)
            {
                if (infiniteFuelBarrel)
                {
                    // Fill inventory to max for infinite fuel barrel
                    _generatorInventory.SetFuelAmounts(true, _generatorInventory.maximumInvFuel, 0);
                    AudioInteractSound();
                }
                else
                {
                    // Add as much fuel as possible from barrel to inventory
                    float fuelToAdd = Mathf.Min(itemFuelAmount, _generatorInventory.maximumInvFuel - _generatorInventory.currentInvFuel);

                    _generatorInventory.SetFuelAmounts(true, fuelToAdd, 0);
                    itemFuelAmount -= fuelToAdd;

                    AudioInteractSound();

                    if (DebugSettings.EnableDebugLogs)
                        Debug.Log($"Fuel barrel: added {fuelToAdd} fuel to inventory.");
                }
            }
        }

        // Logic for filling the generator with fuel
        private void GeneratorFillingLogic()
        {
            // Check if player has jerrycan, fuel in inventory, and generator isn't full
            if (_generatorInventory.hasJerrycan && _generatorInventory.currentInvFuel > 0 && itemFuelAmount < itemMaxFuelAmount)
            {
                fuelRequired = itemMaxFuelAmount - itemFuelAmount;
                // Calculate fuel to add based on fill rate and available fuel
                float fuelToAdd = Mathf.Min(fuelRequired, _generatorInventory.currentInvFuel, fillRate * Time.deltaTime);

                itemFuelAmount += fuelToAdd;
                _generatorInventory.SetFuelAmounts(false, _generatorInventory.currentInvFuel - fuelToAdd, 0);

                if (!isPouringFuel)
                {
                    AudioWaterPour(); // Play fuel pouring sound
                    isPouringFuel = true;
                }

                if (itemFuelAmount >= itemMaxFuelAmount)
                {
                    ActivateGenerator(); // Activate generator if full
                }
            }

            // Stop pouring sound if inventory is out of fuel
            if (_generatorInventory.currentInvFuel <= 0 && isPouringFuel)
            {
                StopWaterPour();
                isPouringFuel = false;
            }
        }

        // Logic for burning fuel when generator is active
        private void GeneratorFuelBurnLogic()
        {
            if (itemFuelAmount > 0)
            {
                // Decrease fuel based on burn rate
                itemFuelAmount -= Time.deltaTime * burnRate;

                if (itemFuelAmount <= 0)
                {
                    DeactivateGenerator(); // Turn off generator if out of fuel
                    itemFuelAmount = 0;
                }
            }
        }

        // Apply rumble effect to generator using Perlin noise
        private void RumbleGenerator()
        {
            transform.localPosition = generatorOrigin + rumbleIntensity * new Vector3(
                Mathf.PerlinNoise(rumbleSpeed * Time.time, 1),
                Mathf.PerlinNoise(rumbleSpeed * Time.time, 2),
                Mathf.PerlinNoise(rumbleSpeed * Time.time, 3));
        }

        // Activate the generator and trigger associated events
        private void ActivateGenerator()
        {
            if (DebugSettings.EnableDebugLogs)
                Debug.Log("Activating generator.");

            rumbling = true; // Start rumble effect
            activateGenerator.Invoke(); // Trigger activation event
        }

        // Deactivate the generator and trigger associated events
        private void DeactivateGenerator()
        {
            if (DebugSettings.EnableDebugLogs)
                Debug.Log("Deactivating generator.");

            rumbling = false; // Stop rumble effect
            deactivateGenerator.Invoke(); // Trigger deactivation event
        }

        // Show or hide item stats UI
        private void ShowObjectStats(bool show)
        {
            if (showUI)
            {
                _popoutUI.itemCanvas.SetActive(show); // Toggle UI canvas
                UpdateItemStats(show); // Update UI with current stats
            }
        }

        // Update UI with current item stats
        private void UpdateItemStats(bool on)
        {
            if (_showUI)
            {
                _popoutUI.itemNameUI.text = _popoutUI.itemName; // Set item name
                _popoutUI.iconImageUI.sprite = _popoutUI.iconImage; // Set item icon
                _popoutUI.fuelAmountUI.text = itemFuelAmount.ToString("0"); // Set current fuel
                _popoutUI.maxFuelAmountUI.text = itemMaxFuelAmount.ToString("0"); // Set max fuel
                _popoutUI.fuelGaugeUI.fillAmount = (itemFuelAmount / itemMaxFuelAmount); // Update fuel gauge
            }
        }

        // Play interaction sound
        private void AudioInteractSound()
        {
            AKAudioManager.instance.Play(interactSound);
        }

        // Play fuel pouring sound
        private void AudioWaterPour()
        {
            AKAudioManager.instance.Play(fuelPourSound);
        }

        // Stop fuel pouring sound with fade-out
        private void StopWaterPour()
        {
            if (AKAudioManager.instance.IsPlaying(fuelPourSound))
            {
                StartCoroutine(AKAudioManager.instance.FadeOut(fuelPourSound, 0.25f));
            }
            isPouringFuel = false;
        }
    }
}