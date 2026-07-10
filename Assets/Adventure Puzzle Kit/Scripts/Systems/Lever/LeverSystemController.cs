/// REMEMBER: This script has a custom editor called "LeverSystemControllerEditor", found in the "Editor" folder. You will need to add new properties to this
/// if you create new variables / fields in this script. Contact me if you have any troubles at all!

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AdventurePuzzleKit.LeverSystem
{
    // Manages the lever system, tracking lever pulls and validating the correct sequence
    public class LeverSystemController : MonoBehaviour
    {
        // The correct order in which levers should be pulled (e.g., "12345")
        [Tooltip("Order the levers should be pulled")]
        [SerializeField] private string leverOrder = "12345";

        // Maximum number of lever pulls allowed, should match the length of leverOrder
        [Tooltip("Match this with the number of values in the leverOrder")]
        [SerializeField] private int pullLimit = 5;

        // Time delay before a lever can be pulled again after interaction
        [Tooltip("Time before pulling lever after interacting")]
        [SerializeField] private float pullTimer = 1.0f;

        // Array of levers and buttons that can be interacted with
        [Tooltip("Add each of the levers and buttons that you will interact with")]
        [SerializeField] private GameObject[] interactiveObjects = null;

        // Animators for control box switches
        [Tooltip("Control Box Switches (Animated)")]
        [SerializeField] private Animator readySwitch = null; // Animator for the "ready" state switch
        [SerializeField] private Animator limitReachedSwitch = null; // Animator for the "limit reached" state switch
        [SerializeField] private Animator acceptedSwitch = null; // Animator for the "accepted" state switch
        [SerializeField] private Animator resettingSwitch = null; // Animator for the "resetting" state switch

        // Lights indicating control unit states
        [Tooltip("Control Unit Lights")]
        [SerializeField] private GameObject readyLight = null; // Light for the "ready" state
        [SerializeField] private GameObject limitReachedLight = null; // Light for the "limit reached" state
        [SerializeField] private GameObject acceptedLight = null; // Light for the "accepted" state
        [SerializeField] private GameObject resettingLight = null; // Light for the "resetting" state

        // Animators for accept and reset buttons
        [SerializeField] private Animator testButton = null; // Animator for the "test" (accept) button
        [SerializeField] private Animator resetButton = null; // Animator for the "reset" button

        // Animation names for switches and buttons
        [SerializeField] private string switchOnName = "Switch_On"; // Animation name for switch on state
        [SerializeField] private string switchOffName = "Switch_Off"; // Animation name for switch off state
        [SerializeField] private string redButtonName = "RedButton_Push"; // Animation name for button press

        // Audio clips for lever interactions
        [SerializeField] private Sound switchPullSound = null; // Sound played when a lever is pulled
        [SerializeField] private Sound switchFailSound = null; // Sound played when resetting or failing
        [SerializeField] private Sound switchDoorSound = null; // Sound played when the correct sequence is entered

        // Event triggered when the correct lever sequence is completed
        [SerializeField] private UnityEvent LeverPower = null;

        private string playerOrder = null; // Tracks the player's lever pull sequence
        private int pulls; // Tracks the number of lever pulls
        private bool canPull = true; // Controls whether a lever can be pulled (based on timer)


        // Materials for control unit lights
        private Material readyBtnMat;
        private Material resettingBtnMat;
        private Material acceptedBtnMat;
        private Material limitBtnMat;

        private void Start()
        {
            SetMaterials(); // Initialize light materials
            InitializeSwitches(); // Set initial switch states
        }

        // Initializes materials for control unit lights
        private void SetMaterials()
        {
            readyBtnMat = readyLight.GetComponent<Renderer>().material; // Get material for ready light
            resettingBtnMat = resettingLight.GetComponent<Renderer>().material; // Get material for resetting light
            acceptedBtnMat = acceptedLight.GetComponent<Renderer>().material; // Get material for accepted light
            limitBtnMat = limitReachedLight.GetComponent<Renderer>().material; // Get material for limit reached light
            readyBtnMat.color = Color.green; // Set initial ready light to green
        }

        // Initializes switch animations to their default states
        private void InitializeSwitches()
        {
            readySwitch.Play(switchOnName, 0, 0.0f); // Set ready switch to on
            resettingSwitch.Play(switchOffName, 0, 0.0f); // Set resetting switch to off
            acceptedSwitch.Play(switchOffName, 0, 0.0f); // Set accepted switch to off
            limitReachedSwitch.Play(switchOffName, 0, 0.0f); // Set limit reached switch to off
        }

        // Triggers the LeverPower event when the correct sequence is completed
        void LeverInteraction()
        {
            LeverPower.Invoke();
        }

        // Coroutine to enforce a delay between lever pulls
        IEnumerator Timer()
        {
            canPull = false; // Prevent pulling during delay
            yield return new WaitForSeconds(pullTimer); // Wait for specified time
            canPull = true; // Allow pulling again
        }

        // Initiates a lever pull if conditions are met
        public void InitLeverPull(LeverItem _leverItem, int leverNumber)
        {
            if (canPull && pulls <= pullLimit - 1) // Check if pulling is allowed and limit not reached
            {
                _leverItem.HandleAnimation(); // Trigger lever animation
                LeverPull(leverNumber); // Record the pull
            }
        }

        // Records a lever pull and updates the system state
        public void LeverPull(int leverNumber)
        {
            playerOrder = playerOrder + leverNumber; // Append lever number to player sequence
            pulls++; // Increment pull count
            if (canPull)
            {
                StartCoroutine(Timer()); // Start pull delay timer
                PlayAudio(switchPullSound); // Play pull sound
                if (pulls >= pullLimit) // If pull limit reached, update switches
                {
                    UpdateSwitches(Color.red, Color.green, true);
                }
            }
        }

        // Updates switch states and light colors
        private void UpdateSwitches(Color readyColor, Color limitColor, bool playAnimation)
        {
            readyBtnMat.color = readyColor; // Update ready light color
            limitBtnMat.color = limitColor; // Update limit reached light color

            readySwitch.Play(switchOffName, 0, 0.0f); // Turn off ready switch
            if (playAnimation)
            {
                limitReachedSwitch.Play(switchOnName, 0, 0.0f); // Turn on limit reached switch
            }
            else
            {
                limitReachedSwitch.Play(switchOffName, 0, 0.0f); // Turn off limit reached switch
            }
        }

        // Resets the lever system to its initial state
        public void LeverReset()
        {
            pulls = 0; // Reset pull count
            playerOrder = ""; // Clear player sequence
            PlayAudio(switchFailSound); // Play fail sound
            resetButton.Play(redButtonName, 0, 0.0f); // Animate reset button

            StartCoroutine(ResetTimer(1.0f)); // Start reset timer
            UpdateSwitches(Color.green, Color.red, false); // Update switches to reset state
        }

        // Checks if the player's lever sequence is correct
        public void LeverCheck()
        {
            testButton.Play(redButtonName, 0, 0.0f); // Animate test button
            if (playerOrder == leverOrder) // If player sequence matches correct order
            {
                CompleteLeverCheck(); // Process successful sequence
            }
            else
            {
                LeverReset(); // Reset if sequence is incorrect
            }
        }

        // Handles a successful lever sequence
        private void CompleteLeverCheck()
        {
            pulls = 0; // Reset pull count
            PlayAudio(switchDoorSound); // Play success sound

            LeverInteraction(); // Trigger the LeverPower event

            // Untag interactive objects to prevent further interaction
            foreach (GameObject obj in interactiveObjects)
            {
                obj.gameObject.tag = "Untagged";
            }

            // Update light colors to reflect success
            readyBtnMat.color = Color.red;
            resettingBtnMat.color = Color.red;
            acceptedBtnMat.color = Color.green;
            limitBtnMat.color = Color.red;

            // Update switch states
            readySwitch.Play(switchOffName, 0, 0.0f);
            resettingSwitch.Play(switchOffName, 0, 0.0f);
            acceptedSwitch.Play(switchOnName, 0, 0.0f);
            limitReachedSwitch.Play(switchOffName, 0, 0.0f);
        }

        // Coroutine to handle reset timing and light/switch updates
        IEnumerator ResetTimer(float waitTime)
        {
            yield return new WaitForSeconds(waitTime); // Wait for specified time
            readyBtnMat.color = Color.green; // Restore ready light to green
            resettingBtnMat.color = Color.red; // Ensure resetting light is red

            readySwitch.Play(switchOnName, 0, 0.0f); // Turn on ready switch
            resettingSwitch.Play(switchOffName, 0, 0.0f); // Ensure resetting switch is off
        }

        // Plays the specified audio clip
        void PlayAudio(Sound sound)
        {
            AKAudioManager.instance.Play(sound);
        }

        // Cleans up materials when the object is destroyed
        private void OnDestroy()
        {
            Destroy(readyBtnMat); // Free ready light material
            Destroy(resettingBtnMat); // Free resetting light material
            Destroy(acceptedBtnMat); // Free accepted light material
            Destroy(limitBtnMat); // Free limit reached light material
        }
    }
}