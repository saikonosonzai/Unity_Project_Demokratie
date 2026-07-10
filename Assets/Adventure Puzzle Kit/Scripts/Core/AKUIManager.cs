/// This script has a custom editor called "AKUIManagerEditor", found in the "Editor" folder. You will need to add new properties to this if you create new variables / fields in this script.
/// Contact me if you have any troubles at all!

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using AdventurePuzzleKit.ChessSystem;
using AdventurePuzzleKit.ExamineSystem;
using AdventurePuzzleKit.ThemedKey;
using AdventurePuzzleKit.ValveSystem;
using AdventurePuzzleKit.SafeSystem;
using AdventurePuzzleKit.PhoneSystem;
using AdventurePuzzleKit.KeypadSystem;
using AdventurePuzzleKit.KeycardSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace AdventurePuzzleKit
{
    public class AKUIManager : MonoBehaviour
    {
        #region Inventory UI System Container Fields / Core UI 
        [SerializeField] private GameObject inventoryContainer = null;

        [SerializeField] private GameObject flashlightUI = null;
        [SerializeField] private GameObject flashlightBatteryUI = null;
        [SerializeField] private GameObject gasmaskUI = null;
        [SerializeField] private GameObject gasMaskFilterUI = null;
        [SerializeField] private GameObject generatorUI = null;
        [SerializeField] private GameObject themedKeyUI = null;
        [SerializeField] private GameObject valveUI = null;
        [SerializeField] private GameObject chessPuzzleUI = null;
        [SerializeField] private GameObject keycardUI = null;

        [SerializeField] private GameObject themedKeyContainerUI = null;
        [SerializeField] private GameObject valveContainerUI = null;
        [SerializeField] private GameObject chessPuzzleContainerUI = null;
        [SerializeField] private GameObject keycardContainerUI = null;

        [SerializeField] private Image _radialIndicator = null;
        [SerializeField] private GameObject triggerInteractPrompt = null;

        [SerializeField] private Image crosshair = null;
        #endregion

        #region Highlight, pickup, examine prompts fields
        public static bool ShouldHighlightName { get; set; } = false; //Added this public state so we know which AKItem should make highlight appear or not

        [SerializeField] private GameObject highlightNameCanvas = null;
        [SerializeField] private TMP_Text highlightItemNameUI = null;
        [SerializeField] private RectTransform highlightBackground; // The actual black box

        [SerializeField] private GameObject interactPromptContainer = null;
        [SerializeField] private string interactPromptText = "Interact";
        [SerializeField] private TMP_Text interactPromptTextUI = null;

        [SerializeField] private GameObject pickupPromptContainer = null;
        [SerializeField] private string pickupPromptText = "Pickup";
        [SerializeField] private TMP_Text pickupPromptTextUI = null;

        [SerializeField] private string interactPromptButtonText = "E";
        [SerializeField] private TMP_Text interactPromptButtonUI = null;

        [SerializeField] private string pickupPromptButtonText = "E";
        [SerializeField] private TMP_Text pickupPromptButtonUI = null;

        [SerializeField] private GameObject examinePromptContainer = null;
        [SerializeField] private string examinePromptButtonText = "Q";
        [SerializeField] private TMP_Text examinePromptButtonUI = null;

        [SerializeField] private RectTransform promptContainer; // Container for prompts
        [SerializeField] private GameObject promptPrefab; // Prefab for a single prompt UI

        private int initialPromptPoolSize = 5; //Change this to change how many prompts should be available to use from start - This can be optimized by reducing this further
        private readonly Queue<GameObject> promptPool = new Queue<GameObject>();

        //To remember previous interaction prompts
        private bool? previousShowInteractPrompt = null;
        private bool? previousShowPickupPrompt = null;
        private bool? previousShowExaminePrompt = null;
        private bool? previousShowHighlightNamePrompt = null;
        private string? previousItemName = null;

        #endregion

        #region Flashlight Fields
        [SerializeField] private Image flashlightIndicatorUI = null;
        [SerializeField] private Image batteryLevelUI = null;

        [SerializeField] private Image batteryIconUI = null;
        [SerializeField] private TMP_Text batteryCountUI = null;
        #endregion

        #region Fuse Fields
        [SerializeField] private GameObject fuseboxUI = null;
        [SerializeField] private Image fuseIcon = null;
        [SerializeField] private TMP_Text fuseCountUI = null;
        #endregion

        #region GasMask Fields
        public enum MaskUIState { MaskNormal, MaskEquipped }
        private MaskUIState _maskUIState;

        public enum FilterState { FilterNumber, FilterAlarm, FilterNormal, FilterValue }
        private FilterState _filterState;

        public enum PostProcessState { OriginalPostProcess, GasPostProcess }
        private PostProcessState _postProcessState;

        [Header("Gas Mask UI Colours")]
        [SerializeField] private Color maskEquippedColor = Color.green;
        [SerializeField] private Color maskNormalColor = Color.white;

        [Header("Filter UI Colours")]
        [SerializeField] private Color filterAlarmColor = Color.red;
        [SerializeField] private Color filterNormalColor = Color.white;

        [Header("Health UI")]
        [SerializeField] private TMP_Text healthPercTextUI = null;
        [SerializeField] private Image healthSliderUI = null;

        [Header("Gas Mask UI")]
        [SerializeField] private Image _maskIconUI = null;
        [SerializeField] private Image _filterIconUI = null;
        [SerializeField] private TMP_Text _filterCountUI = null;
        [SerializeField] private Image _filterSliderUI = null;

        [Header("Visor Overlay UI")]
        [SerializeField] private GameObject visorImageOverlay = null;
        #endregion

        #region Generator Fields
        [Header("UI Image Elements")]
        [SerializeField] private Image fuelFillUI = null;

        [Header("UI Text Elements")]
        [SerializeField] private TMP_Text currentFuelText = null;
        [SerializeField] private TMP_Text maximumFuelText = null;
        #endregion

        #region Examine Fields
        [Header("No UI Close Button")]
        [SerializeField] private GameObject noUICloseButton = null;

        [Header("Basic Example UI References")]
        [SerializeField] private TMP_Text basicItemNameUI = null;
        [SerializeField] private TMP_Text basicItemDescUI = null;
        [SerializeField] private GameObject basicExamineUI = null;

        [Header("Right Side Example UI References")]
        [SerializeField] private TMP_Text rightItemNameUI = null;
        [SerializeField] private TMP_Text rightItemDescUI = null;
        [SerializeField] private GameObject rightExamineUI = null;

        [Header("Interest Point UI's")]
        [SerializeField] private TMP_Text interestPointText = null;
        [SerializeField] private GameObject interestPointParentUI = null;

        [HideInInspector] public ExaminableItem _examinableItem;
        #endregion

        #region Themed Key Fields
        [SerializeField] private Image[] tkInventorySlots = null;
        [SerializeField] private Image[] tkInventoryBGSlots = null;
        #endregion

        #region Valve Fields
        [SerializeField] private Image[] valveInventorySlots = null;
        [SerializeField] private Image[] valveInventoryBGSlots = null;

        [SerializeField] private Image valveProgressUI = null;
        [SerializeField] private GameObject valveSliderContainerUI = null;

        public Image ValveProgressUI
        {
            get { return valveProgressUI; }
            set { valveProgressUI = value; }
        }

        public GameObject ValveSliderContainerUI
        {
            get { return valveSliderContainerUI; }
            set { valveSliderContainerUI = value; }
        }
        #endregion

        #region Keycard Fields
        [SerializeField] private Image[] keycardInventorySlots = null;
        [SerializeField] private Image[] keycardInventoryBGSlots = null;
        #endregion

        #region Chess Fields
        [SerializeField] private ChessSlotWidget[] _slotWidgets = null;

        //private bool disableInventoryOpen = false;
        private bool disableRemoveButton;
        private ChessFuseBoxInteractable _fuseBox;

        public ChessFuseBoxInteractable fuseBoxInteractable
        {
            get { return _fuseBox; }
            set
            {
                _fuseBox = value;
                foreach (ChessSlotWidget slotWidget in _slotWidgets)
                {
                    slotWidget.FuseBox = _fuseBox;
                }
            }
        }
        #endregion

        #region Safe Fields
        [Tooltip("Add the main safe canvas here")]
        [SerializeField] private GameObject safeCanvasContainerUI = null;

        [Tooltip("Add the UI numbers text UI elements here")]
        [Space(5)] [SerializeField] private Button acceptBtn = null;

        [Tooltip("Add the UI numbers text UI elements here")]
        [Space(5)] [SerializeField] private TMP_Text[] numberUI = new TMP_Text[3];

        [Tooltip("Add the UI selection buttons, there should be 3")]
        [Space(5)] [SerializeField] private Button[] selectionBtn = new Button[3];

        public string playerInputNumber { get; private set; }
        #endregion

        #region Phone Fields
        [Header("Phone Type Input Fields")]
        [SerializeField] private TMP_InputField payPhoneCodeText = null;
        [SerializeField] private TMP_InputField officePhoneCodeText = null;
        [SerializeField] private TMP_InputField mobilePhoneCodeText = null;

        [Header("Phone Type Canvas Fields")]
        [SerializeField] private GameObject payPhoneCanvas = null;
        [SerializeField] private GameObject officePhoneCanvas = null;
        [SerializeField] private GameObject mobilePhoneCanvas = null;

        private bool firstPhoneClick;
        private PhoneController _phoneController;
        private PhoneType _phoneType;
        private enum PhoneType { None, Pay, Office, Mobile };
        #endregion

        #region Keypad Fields
        [Header("Keypad Type Input Fields")]
        [SerializeField] private TMP_InputField modernCodeText = null;
        [SerializeField] private TMP_InputField scifiCodeText = null;
        [SerializeField] private TMP_InputField keyboardCodeText = null;

        [Header("Phone Type Canvas Fields")]
        [SerializeField] private GameObject modernCanvas = null;
        [SerializeField] private GameObject scifiCanvas = null;
        [SerializeField] private GameObject keyboardCanvas = null;

        private bool firstKeypadClick;
        private KeypadController _keypadController;
        private KeypadType _keypadType;
        private enum KeypadType { None, Modern, Scifi, Keyboard };
        #endregion

        #region Collectable Item Fields
        public Image radialIndicator
        {
            get { return _radialIndicator; }
            set { _radialIndicator = value; }
        }

        public bool hasFlashlight { get; set; }
        public bool disableFlashlightUI { get; set; }
        public bool hasJerrycan { get; set; }
        public bool disableJerrycan { get; set; }
        public bool hasGasMask { get; set; }
        public bool disableGasmaskUI { get; set; }
        public bool hasThemedKey { get; set; }
        public bool hasValve { get; set; }
        public bool hasKeycard { get; set; }
        public bool hasChessPiece { get; set; }
        public bool hasFuse { get; set; }
        public bool isInteracting { get; set; }
        public bool isInventoryOpen { get; set; }
        #endregion

        [SerializeField] private bool persistAcrossScenes = true;

        private bool showUI;

        public static AKUIManager instance;

        private void Awake()
        {
            #region Instantiation logic - Instance creations
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
            #endregion

            FieldNullCheck();
            PrewarmPromptPool();

            #region Post Processing Debug
            if (DebugSettings.EnablePostProcessingMessage)
                Debug.Log("Please make sure Post Processing is installed, navigate to Package Manager > Unity Registery > Type 'Post Processing' - Then install");
            #endregion
            

            #region Disable Prompt Container UI 
            ShowPromptContainer(false);
            #endregion
        }

        #region Update Method for Enabling / Disabling Inventory + Input Checks
        private void Update()
        {
            // If isInteracting is true, exit the method early
            if (GameState.IsUsingSystem) return;

            if (Input.GetKeyDown(AKInputManager.instance.toggleInventoryKey))
            {
                ToggleInventory();
            }
            else if (showUI && Input.GetKeyDown(AKInputManager.instance.closeInventoryKey))
            {
                CloseInventoryUI();
            }
        }

        private void ToggleInventory()
        {
            showUI = !showUI;
            if (showUI)
                OpenInventoryUI();
            else
                CloseInventoryUI();
        }
        #endregion

        #region What happens when inventory opens / closes
        void OpenInventoryUI()
        {
            GameState.IsInventoryOpen = true;
            inventoryContainer.SetActive(true);
            isInventoryOpen = true;
            AKDisableManager.instance.DisablePlayerDefault(true, false, false);
            isInteracting = false;
            showUI = true;
            AKPromptManager.Instance.RegisterPromptsForSubsystem("Inventory");
        }

        void CloseInventoryUI()
        {
            GameState.IsInventoryOpen = false;
            inventoryContainer.SetActive(false);
            isInventoryOpen = false;
            AKDisableManager.instance.DisablePlayerDefault(false, false, false);
            isInteracting = false;
            fuseBoxInteractable = null;
            showUI = false;
            AKPromptManager.Instance.ClearPrompts();
        }
        #endregion

        #region Prompt UI updating (Using AKPromptManager
        public void ShowPromptContainer(bool show)
        {
            if (promptContainer != null)
            {
                promptContainer.gameObject.SetActive(show);
            }
        }

        private void PrewarmPromptPool()
        {
            for (int i = 0; i < initialPromptPoolSize; i++)
            {
                GameObject prompt = Instantiate(promptPrefab);
                prompt.SetActive(false);
                promptPool.Enqueue(prompt);
            }
        }

        public void UpdatePromptsUI(List<AKPromptManager.Prompt> prompts)
        {
            // Show or hide the container based on the presence of prompts
            ShowPromptContainer(prompts.Count > 0);

            // Step 1: Return all current prompt objects to the pool
            var childrenToReturn = new List<GameObject>();

            foreach (Transform child in promptContainer)
            {
                var prompt = child.gameObject;
                childrenToReturn.Add(prompt);
            }

            foreach (var prompt in childrenToReturn)
            {
                prompt.transform.SetParent(null); // Detach
                prompt.SetActive(false);
                promptPool.Enqueue(prompt); // Return to pool
            }

            // Step 2: Display the required number of prompts
            for (int i = 0; i < prompts.Count; i++)
            {
                var promptData = prompts[i];
                GameObject promptUI = GetPooledPrompt(); // Will expand pool if needed

                promptUI.transform.SetParent(promptContainer); // Reparent
                promptUI.transform.SetSiblingIndex(i);

                var textElements = promptUI.GetComponentsInChildren<TextMeshProUGUI>();
                if (textElements.Length >= 2)
                {
                    textElements[0].text = promptData.Key;
                    textElements[1].text = promptData.Label;
                }
                else
                {
                    Debug.LogWarning("Prompt prefab missing TextMeshProUGUI elements!");
                }

                promptUI.SetActive(true);
            }
        }

        private GameObject GetPooledPrompt()
        {
            // Retrieve a prompt from the pool, or instantiate a new one if the pool is empty
            if (promptPool.Count > 0)
            {
                return promptPool.Dequeue();
            }

            //var newPrompt = Instantiate(promptPrefab, promptContainer);
            var newPrompt = Instantiate(promptPrefab);
            return newPrompt;
        }
        #endregion

        #region Highlighting name / pickup or examining
        public void SetHighlightName(string? itemName, bool? isVisible, bool? showHighlightNamePrompt, bool? showInteractPrompt, bool? showPickupPrompt, bool? showExaminePrompt)
        {
            // Store previous states before making changes
            if (showInteractPrompt.HasValue) previousShowInteractPrompt = showInteractPrompt;
            if (showPickupPrompt.HasValue) previousShowPickupPrompt = showPickupPrompt;
            if (showExaminePrompt.HasValue) previousShowExaminePrompt = showExaminePrompt;
            if (showHighlightNamePrompt.HasValue) previousShowHighlightNamePrompt = showHighlightNamePrompt;

            // Update the item name
            highlightItemNameUI.text = itemName ?? (isVisible.HasValue && isVisible.Value ? previousItemName ?? string.Empty : string.Empty);
            if (itemName != null) previousItemName = itemName;

            // **Adjust background size dynamically**
            ResizeHighlightBackground();

            // Update highlight name visibility
            UpdateContainerState(highlightNameCanvas, isVisible, showHighlightNamePrompt, previousShowHighlightNamePrompt);

            // Update interact prompt visibility
            UpdateContainerState(interactPromptContainer, isVisible, showInteractPrompt, previousShowInteractPrompt);
            interactPromptTextUI.text = interactPromptText;
            interactPromptButtonUI.text = interactPromptButtonText;

            // Update pickup prompt visibility
            UpdateContainerState(pickupPromptContainer, isVisible, showPickupPrompt, previousShowPickupPrompt);
            pickupPromptTextUI.text = pickupPromptText;
            pickupPromptButtonUI.text = pickupPromptButtonText;

            // Update examine prompt visibility
            UpdateContainerState(examinePromptContainer, isVisible, showExaminePrompt, previousShowExaminePrompt);
            examinePromptButtonUI.text = examinePromptButtonText;
        }

        // Helper method to update container visibility
        private void UpdateContainerState(GameObject container, bool? isVisible, bool? explicitState, bool? previousState)
        {
            if (isVisible.HasValue)
            {
                container.SetActive(explicitState.HasValue ? explicitState.Value : isVisible.Value && (previousState ?? false));
            }
            else if (explicitState.HasValue)
            {
                container.SetActive(explicitState.Value);
            }
        }

        private void ResizeHighlightBackground()
        {
            float textWidth = highlightItemNameUI.preferredWidth;
            float padding = 35f; // Add extra padding if needed

            // Set the background's width dynamically
            highlightBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth + padding);
        }
        #endregion

        #region Trigger Interact Prompt
        public void EnableInteractPrompt(bool on)
        {
            triggerInteractPrompt.SetActive(on);
        }
        #endregion

        #region Disabled Themed Key / Valve / Chess UI Containers
        public void DisableUIContainers()
        {
            themedKeyContainerUI.SetActive(false);
            valveContainerUI.SetActive(false);
            chessPuzzleContainerUI.SetActive(false);
            keycardContainerUI.SetActive(false);
        }
        #endregion

        #region Examine UI Logic
        public void SetInspectPointParent(bool on, Vector3 position)
        {
            interestPointParentUI.SetActive(on);
            interestPointParentUI.transform.position = position;
        }

        public void SetInspectPointText(string inspectText)
        {
            interestPointText.text = inspectText;
        }

        public void SetBasicUIText(string itemName, string itemDescription, bool on)
        {
            basicItemNameUI.text = itemName;
            basicItemDescUI.text = itemDescription;
            basicExamineUI.SetActive(on);
        }

        public void SetBasicUITextSettings(int textSize, TMP_FontAsset fontType, Color fontColor, int textSizeDesc, TMP_FontAsset fontTypeDesc, Color fontColorDesc)
        {
            basicItemNameUI.fontSize = textSize;
            basicItemNameUI.font = fontType;
            basicItemNameUI.color = fontColor;

            basicItemDescUI.fontSize = textSizeDesc;
            basicItemDescUI.font = fontTypeDesc;
            basicItemDescUI.color = fontColorDesc;
        }

        public void SetRightSideUIText(string itemName, string itemDescription, bool on)
        {
            rightItemNameUI.text = itemName;
            rightItemDescUI.text = itemDescription;
            rightExamineUI.SetActive(on);
        }

        public void SetRightUITextSettings(int textSize, TMP_FontAsset fontType, Color fontColor, int textSizeDesc, TMP_FontAsset fontTypeDesc, Color fontColorDesc)
        {
            rightItemNameUI.fontSize = textSize;
            rightItemNameUI.font = fontType;
            rightItemNameUI.color = fontColor;

            rightItemDescUI.fontSize = textSizeDesc;
            rightItemDescUI.font = fontTypeDesc;
            rightItemDescUI.color = fontColorDesc;
        }

        public void ShowCloseButton(bool on)
        {
            noUICloseButton.SetActive(on);
        }

        public void ShowInteractionName(bool on)
        {
            highlightNameCanvas.SetActive(on);
        }

        public void CloseButton()
        {
            _examinableItem.DropObject(true);
        }
        #endregion

        #region Chess Puzzle UI Logic
        public void ChessPieceCollected()
        {
            hasChessPiece = true;
            if (chessPuzzleUI)
            {
                chessPuzzleUI.SetActive(true);
            }
            else
            {
                Debug.Log("Add the Chess piece canvas to avoid errors!");
            }
        }

        public void EnabledChessPuzzleUIContainer()
        {
            if (chessPuzzleUI)
            {
                DisableUIContainers();
                chessPuzzleContainerUI.SetActive(true);
            }
        }

        public void FillChessInventorySlot()
        {
            int chessPieceCount = ChessInventory.instance.chessPieceList.Count;

            for (int i = 0; i < _slotWidgets.Length; i++)
            {
                ChessPiece chessPiece = i < chessPieceCount ? ChessInventory.instance.chessPieceList[i] : null;
                _slotWidgets[i].SetPiece(chessPiece);
            }
        }

        public void ResetChessInventorySlot()
        {
            FillChessInventorySlot();
        }

        public void RemoveFuseButton()
        {
            if (!disableRemoveButton)
            {
                fuseBoxInteractable.RemoveFuse(fuseBoxInteractable.currentFuse);
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void OpenInventoryFusebox(ChessFuseBoxInteractable fuseBoxController)
        {
            fuseBoxInteractable = fuseBoxController;
            OpenInventoryUI();
            disableRemoveButton = false;
        }

        public void DisableInventoryFusebox()
        {
            CloseInventoryUI();
            disableRemoveButton = true;
        }
        #endregion

        #region Flashlight UI Logic
        public void FlashlightCollected()
        {
            hasFlashlight = true;
            if (!disableFlashlightUI)
            {
                if (flashlightUI)
                {
                    flashlightUI.SetActive(true);
                }
                else
                {
                    Debug.Log("Add the Flashlight UI Parent to avoid errors!");
                }
            }
        }

        public void BatteryCollected()
        {
            if (!disableFlashlightUI)
            {
                if (flashlightBatteryUI)
                {
                    flashlightBatteryUI.SetActive(true);
                }
                else
                {
                    Debug.Log("Add the Flashlight Battery Parent to avoid errors!");
                }
            }
        }

        public void ToggleFlashlightUI(bool showUI)
        {
            flashlightUI.SetActive(showUI);
        }
        public void FlashlightIndicatorColor(bool on)
        {
            flashlightIndicatorUI.color = on ? Color.white : Color.black;
        }
        public void MaximumBatteryLevel(float maxIntensity)
        {
            batteryLevelUI.fillAmount = maxIntensity;
        }
        public void UpdateBatteryLevelUI(float drainAmount)
        {
            batteryLevelUI.fillAmount -= drainAmount;
        }
        public void UpdateBatteryCountUI(int batteryCount)
        {
            if (batteryCount >= 1)
            {
                batteryIconUI.color = Color.white;
            }
            else
            {
                batteryIconUI.color = Color.black;
            }
            batteryCountUI.text = batteryCount.ToString("0");
        }
        #endregion

        #region Gas Mask UI Logic
        public void GasMaskCollected()
        {
            hasGasMask = true;
            if (!disableGasmaskUI)
            {
                if (gasmaskUI)
                {
                    gasmaskUI.SetActive(true);
                }
            }
            else
            {
                Debug.Log("Add the Gas Mask Parent to avoid errors!");
            }
        }

        public void FilterCollected()
        {
            if (gasMaskFilterUI)
            {
                gasMaskFilterUI.SetActive(true);
            }
            else
            {
                Debug.Log("Add the Gas Mask Filter Parent to avoid errors!");
            }
        }

        public void UpdateFilterUI(FilterState _filterState, string filterAmount, float filterFillAmount)
        {
            switch (_filterState)
            {
                case FilterState.FilterNumber:
                    _filterCountUI.text = filterAmount;
                    break;
                case FilterState.FilterAlarm:
                    _filterIconUI.color = filterAlarmColor;
                    break;
                case FilterState.FilterNormal:
                    _filterIconUI.color = filterNormalColor;
                    break;
                case FilterState.FilterValue:
                    _filterSliderUI.fillAmount = filterFillAmount;
                    break;
            }
        }

        public void UpdateMaskUI(MaskUIState _maskUIState)
        {
            switch (_maskUIState)
            {
                case MaskUIState.MaskNormal: _maskIconUI.color = maskNormalColor; break;
                case MaskUIState.MaskEquipped: _maskIconUI.color = maskEquippedColor; break;
            }
        }

        public void UpdateHealthUI(float currentHealth, float maxHealth)
        {
            healthPercTextUI.text = currentHealth.ToString("0");
            healthSliderUI.fillAmount = (currentHealth / maxHealth);
        }
        #endregion

        #region Generator UI Logic
        public void JerrycanCollected()
        {
            hasJerrycan = true;
            if (!disableJerrycan)
            {
                if (generatorUI)
                {
                    generatorUI.SetActive(true);
                }
                else
                {
                    Debug.Log("Add the Generator Canvas to avoid errors!");
                }
            }
        }
        public void UpdateInventoryUI(float currentFuel, float maximumFuel)
        {
            fuelFillUI.fillAmount = (currentFuel / maximumFuel);
            currentFuelText.text = currentFuel.ToString("0");
            maximumFuelText.text = maximumFuel.ToString("0");
        }
        #endregion

        #region Themed Key UI Logic
        public void ThemedKeyCollected()
        {
            hasThemedKey = true;
            if (themedKeyUI)
            {
                themedKeyUI.SetActive(true);
            }
            else
            {
                Debug.Log("Add the Themed Key Canvas to avoid errors!");
            }
        }

        public void EnabledThemedKeyUIContainer()
        {
            if (hasThemedKey)
            {
                DisableUIContainers();
                themedKeyContainerUI.SetActive(true);
            }
        }

        public void FillTKInventorySlot()
        {
            for (int i = 0; i < tkInventorySlots.Length; i++)
            {
                if (tkInventorySlots[i].enabled == false)
                {
                    tkInventorySlots[i].sprite = TKInventory.instance._keyList[i]._KeySprite;
                    tkInventorySlots[i].enabled = true;
                    tkInventoryBGSlots[i].enabled = true;
                    break;
                }
            }
        }

        public void ResetTKInventorySlot(int keysCollected)
        {
            tkInventorySlots[keysCollected - 1].sprite = null;
            tkInventorySlots[keysCollected - 1].enabled = false;
            tkInventoryBGSlots[keysCollected - 1].enabled = false;

            for (int i = 0; i < tkInventorySlots.Length; i++)
            {
                if (tkInventorySlots[i].enabled == true)
                {
                    tkInventorySlots[i].sprite = TKInventory.instance._keyList[i]._KeySprite;
                }
            }
        }
        #endregion

        #region Valve UI logic
        public void ValveCollected()
        {
            hasValve = true;
            if (valveUI)
            {
                valveUI.SetActive(true);
            }
            else
            {
                Debug.Log("Add the Themed Key Canvas to avoid errors!");
            }
        }

        public void EnabledValveUIContainer()
        {
            if (valveUI)
            {
                DisableUIContainers();
                valveContainerUI.SetActive(true);
            }
        }

        public void ResetProgress()
        {
            ValveProgressUI.fillAmount = 0;
            SliderOpacity(false);
        }

        public void UpdateValveProgress(float valveProgress)
        {
            ValveProgressUI.fillAmount += valveProgress;
        }

        public void SliderOpacity(bool visible)
        {
            valveSliderContainerUI.SetActive(visible);
        }

        public void FillValveInventorySlot()
        {
            for (int i = 0; i < valveInventorySlots.Length; i++)
            {
                if (valveInventorySlots[i].enabled == false)
                {
                    valveInventorySlots[i].sprite = ValveInventory.instance._valvesList[i].ValveSprite;
                    valveInventorySlots[i].enabled = true;
                    valveInventoryBGSlots[i].enabled = true;
                    break;
                }
            }
        }

        public void ResetValveInventorySlot(int valvesCollected)
        {
            valveInventorySlots[valvesCollected - 1].sprite = null;
            valveInventorySlots[valvesCollected - 1].enabled = false;
            valveInventoryBGSlots[valvesCollected - 1].enabled = false;

            for (int i = 0; i < valveInventorySlots.Length; i++)
            {
                if (valveInventorySlots[i].enabled == true)
                {
                    valveInventorySlots[i].sprite = ValveInventory.instance._valvesList[i].ValveSprite;
                }
            }
        }
        #endregion

        #region Keycard UI logic
        public void KeycardCollected()
        {
            hasKeycard = true;
            if (keycardUI)
            {
                keycardUI.SetActive(true);
            }
            else
            {
                Debug.Log("Add the Keycard Canvas to the UI manager (UI Parents section) avoid errors!");
            }
        }

        public void EnabledKeycardUIContainer()
        {
            if (keycardUI)
            {
                DisableUIContainers();
                keycardContainerUI.SetActive(true);
            }
        }

        public void FillKeycardInventorySlot()
        {
            for (int i = 0; i < keycardInventorySlots.Length; i++)
            {
                if (keycardInventorySlots[i].enabled == false)
                {
                    keycardInventorySlots[i].sprite = KeycardInventory.instance.Keycards[i].KeycardSprite;
                    keycardInventorySlots[i].enabled = true;
                    keycardInventoryBGSlots[i].enabled = true;
                    break;
                }
            }
        }

        public void ResetKeycardInventorySlot(int keycardsCollected)
        {
            keycardInventorySlots[keycardsCollected - 1].sprite = null;
            keycardInventorySlots[keycardsCollected - 1].enabled = false;
            keycardInventoryBGSlots[keycardsCollected - 1].enabled = false;

            for (int i = 0; i < keycardInventorySlots.Length; i++)
            {
                if (keycardInventorySlots[i].enabled == true)
                {
                    keycardInventorySlots[i].sprite = KeycardInventory.instance.Keycards[i].KeycardSprite;
                }
            }
        }
        #endregion

        #region Fuse UI Logic
        public void FuseCollected()
        {
            hasFuse = true;
            if (fuseboxUI)
            {
                fuseboxUI.SetActive(true);
            }
            else
            {
                Debug.Log("Add the Fuse Box Canvas to avoid errors!");
            }
        }
        public void UpdateFuseCountUI(int fuses)
        {
            fuseCountUI.text = fuses.ToString("0");
            fuseIcon.color = Color.white;
        }
        #endregion

        #region Safe UI Logic
        public void ShowMainSafeUI(bool active)
        {
            safeCanvasContainerUI.SetActive(active);
            if (active)
            {
                SetInitialSafeUI();
            }
        }

        public void SetInitialSafeUI()
        {
            acceptBtn.onClick.RemoveAllListeners();
            foreach (var btn in selectionBtn) btn.onClick.RemoveAllListeners();
            ResetSafeUI();
        }

        public void ResetEventSystem()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void ResetSafeUI()
        {
            foreach (var numUI in numberUI)
            {
                numUI.text = "0";
            }
            UpdateUIState(0);
        }

        public void SetUIButtons(SafeController _myController)
        {
            acceptBtn.onClick.AddListener(_myController.CheckDialNumber);
            for (int i = 0; i < selectionBtn.Length; i++)
            {
                int index = i + 1;
                selectionBtn[i].onClick.AddListener(() => _myController.MoveDialLogic(index));
            }
        }

        public void PlayerInputCode()
        {
            playerInputNumber = string.Join("", numberUI[0].text, numberUI[1].text, numberUI[2].text);
        }

        public void UpdateNumber(int index, int lockNumber)
        {
            if (index >= 0 && index < numberUI.Length)
            {
                numberUI[index].text = lockNumber.ToString();
            }
            else
            {
                Debug.LogError("Invalid index for UpdateNumber: " + index);
            }
        }

        public void UpdateUIState(int index)
        {
            for (int i = 0; i < numberUI.Length; i++)
            {
                selectionBtn[i].interactable = (i == index);
                numberUI[i].color = (i == index) ? Color.white : Color.gray;

                ColorBlock arrowCB = selectionBtn[i].colors;
                arrowCB.normalColor = (i == index) ? Color.white : Color.gray;
                selectionBtn[i].colors = arrowCB;
            }
        }

        public void SetInteractPrompt(bool on)
        {
            EnableInteractPrompt(on);
        }
        #endregion

        #region Phone UI Logic
        public void SetPhoneController(PhoneController _myController)
        {
            _phoneController = _myController;
        }

        public void ShowPayPhoneCanvas(bool on)
        {
            payPhoneCanvas.SetActive(on);
            _phoneType = PhoneType.Pay;
        }

        public void ShowOfficePhoneCanvas(bool on)
        {
            officePhoneCanvas.SetActive(on);
            _phoneType = PhoneType.Office;
        }

        public void ShowMobilePhoneCanvas(bool on)
        {
            mobilePhoneCanvas.SetActive(on);
            _phoneType = PhoneType.Mobile;
        }

        public void PhoneKeyPressString(string keyString)
        {
            _phoneController.SingleBeepSound();

            if (!firstPhoneClick)
            {
                ClearPhoneInputFields();
                firstPhoneClick = true;
            }

            TMP_InputField activeInputField = GetActivePhoneInputField();
            if (activeInputField != null && activeInputField.characterLimit <= (_phoneController.inputLimit - 1))
            {
                activeInputField.characterLimit++;
                activeInputField.text += keyString;
            }
        }

        public void PhoneKeyPressCall()
        {
            _phoneController.SingleBeepSound();
            TMP_InputField activeInputField = GetActivePhoneInputField();
            if (activeInputField != null)
            {
                _phoneController.CheckCode(activeInputField);
            }
        }

        public void PhoneKeyPressClear()
        {
            _phoneController.SingleBeepSound();
            _phoneController.StopAudio();
            TMP_InputField activeInputField = GetActivePhoneInputField();
            ClearPhoneFieldData(activeInputField);
        }

        public void PhoneKeyPressClose()
        {
            _phoneController.SingleBeepSound();
            _phoneController.StopAudio();
            _phoneController.CloseKeypad();
            firstPhoneClick = false;
        }

        private void ClearPhoneInputFields()
        {
            ClearPhoneFieldData(payPhoneCodeText);
            ClearPhoneFieldData(officePhoneCodeText);
            ClearPhoneFieldData(mobilePhoneCodeText);
        }

        private void ClearPhoneFieldData(TMP_InputField inputField)
        {
            if (inputField != null)
            {
                inputField.characterLimit = 0;
                inputField.text = string.Empty;
            }
        }

        private TMP_InputField GetActivePhoneInputField()
        {
            switch (_phoneType)
            {
                case PhoneType.Pay:
                    return payPhoneCodeText;
                case PhoneType.Office:
                    return officePhoneCodeText;
                case PhoneType.Mobile:
                    return mobilePhoneCodeText;
                default:
                    return null;
            }
        }

        public void SetPhoneInteractPrompt(bool on)
        {
            EnableInteractPrompt(on);
        }
        #endregion

        #region Keypad UI Logic
        public void SetKeypadController(KeypadController _myController)
        {
            _keypadController = _myController;
        }

        public void ShowModernCanvas(bool on)
        {
            modernCanvas.SetActive(on);
            _keypadType = KeypadType.Modern;
        }

        public void ShowScifiCanvas(bool on)
        {
            scifiCanvas.SetActive(on);
            _keypadType = KeypadType.Scifi;
        }

        public void ShowKeyboardCanvas(bool on)
        {
            keyboardCanvas.SetActive(on);
            _keypadType = KeypadType.Keyboard;
        }

        public void KeypadKeyPressString(string keyString)
        {
            _keypadController.SingleBeepSound();

            if (!firstKeypadClick)
            {
                ClearKeypadInputFields();
                firstKeypadClick = true;
            }

            TMP_InputField activeInputField = GetActiveKeypadInputField();
            if (activeInputField != null && activeInputField.characterLimit <= (_keypadController.inputLimit - 1))
            {
                activeInputField.characterLimit++;
                activeInputField.text += keyString;
            }
        }

        public void KeypadKeyPressEnter()
        {
            _keypadController.SingleBeepSound();
            TMP_InputField activeInputField = GetActiveKeypadInputField();
            if (activeInputField != null)
            {
                _keypadController.CheckCode(activeInputField);
            }
        }

        public void KeypadKeyPressClear()
        {
            _keypadController.SingleBeepSound();
            TMP_InputField activeInputField = GetActiveKeypadInputField();
            ClearKeypadFieldData(activeInputField);
        }

        public void KeypadKeyPressClose()
        {
            KeypadKeyPressClear();
            _keypadController.SingleBeepSound();
            _keypadController.CloseKeypad();
        }

        private void ClearKeypadInputFields()
        {
            ClearKeypadFieldData(modernCodeText);
            ClearKeypadFieldData(scifiCodeText);
            ClearKeypadFieldData(keyboardCodeText);
        }

        private void ClearKeypadFieldData(TMP_InputField inputField)
        {
            if (inputField != null)
            {
                inputField.characterLimit = 0;
                inputField.text = string.Empty;
            }
        }

        private TMP_InputField GetActiveKeypadInputField()
        {
            switch (_keypadType)
            {
                case KeypadType.Modern:
                    return modernCodeText;
                case KeypadType.Scifi:
                    return scifiCodeText;
                case KeypadType.Keyboard:
                    return keyboardCodeText;
                default:
                    return null;
            }
        }

        public void SetKeypadInteractPrompt(bool on)
        {
            EnableInteractPrompt(on);
        }
        #endregion

        #region Radial Indicator Enable / Disable
        public void EnableRadialIndicatorUI(float radialTimer)
        {
            radialIndicator.enabled = true;
            radialIndicator.fillAmount = radialTimer;
        }

        public void DisableRadialIndicatorUI(float radialTimer)
        {
            radialIndicator.fillAmount = radialTimer;
            radialIndicator.enabled = false;
        }
        #endregion

        #region Crosshair Show / Hide + Highlight
        public void ShowCrosshair(bool on)
        {
            crosshair.enabled = on;
        }

        public void HighlightCrosshair(bool isHighlighted)
        {
            crosshair.color = isHighlighted ? Color.red : Color.white;
        }
        #endregion

        #region Debugging Field Checks
        void FieldNullCheck()
        {
            #region Highlighting Prompts
            CheckField(highlightNameCanvas, "HighlightNameCanvas");
            CheckField(highlightItemNameUI, "HighlightItemNameUI");

            CheckField(pickupPromptContainer, "Pickup Prompt Container");
            CheckField(pickupPromptTextUI, "Pickup Prompt Text UI");

            CheckField(pickupPromptButtonUI, "Pickup Prompt Button UI");

            CheckField(examinePromptContainer, "Examine Prompt Container");
            CheckField(examinePromptButtonUI, "Examine Prompt Button UI");
            #endregion

            #region Examine
            CheckField(noUICloseButton, "NoUICloseButton");

            CheckField(basicItemNameUI, "BasicItemNameUI");
            CheckField(basicItemDescUI, "BasicItemDescUI");
            CheckField(basicExamineUI, "BasicExamineUI");

            CheckField(rightItemNameUI, "RightItemNameUI");
            CheckField(rightItemDescUI, "RightItemDescUI");
            CheckField(rightExamineUI, "RightExamineUI");

            CheckField(interestPointText, "InterestPointText");
            CheckField(interestPointParentUI, "InterestPointParentUI");
            #endregion

            #region Flashlight
            CheckField(flashlightIndicatorUI, "FlashlightIndicatorUI");
            CheckField(batteryLevelUI, "BatteryLevelUI");
            CheckField(batteryIconUI, "BatteryIconUI");
            CheckField(batteryCountUI, "BatteryCountUI");
            #endregion

            #region Phone
            //CODE TEXT - INPUT FIELDS
            CheckField(payPhoneCodeText, "PayPhoneCodeText");
            CheckField(officePhoneCodeText, "OfficePhoneCodeText");
            CheckField(mobilePhoneCodeText, "MobilePhoneCodeText");

            //CANVASES
            CheckField(payPhoneCanvas, "PayPhoneCanvas");
            CheckField(officePhoneCanvas, "OfficePhoneCanvas");
            CheckField(mobilePhoneCanvas, "MobilePhoneCanvas");
            #endregion

            #region Keypad
            //CODE TEXT - INPUT FIELDS
            CheckField(modernCodeText, "Modern Keypad Canvas");
            CheckField(scifiCodeText, "Scifi Keypad Canvas");
            CheckField(keyboardCodeText, "Keyboard Canvas");

            //CANVASES
            CheckField(modernCanvas, "Modern Keypad Canvas");
            CheckField(scifiCanvas, "Scifi Keypad Canvas");
            CheckField(keyboardCanvas, "Keyboard Canvas");
            #endregion

            #region Fuse Box
            CheckField(fuseboxUI, "Fusebox Parent UI");
            CheckField(fuseIcon, "Fuse Icon UI");
            CheckField(fuseCountUI, "Fuse Count UI");
            #endregion

            #region Safe
            CheckField(safeCanvasContainerUI, "Safe Canvas Container UI");
            CheckField(acceptBtn, "Safe Accept Button UI");
            CheckField(numberUI, "Safe Number UI");
            CheckField(selectionBtn, "Selection Button UI");
            #endregion

            #region Valve
            CheckField(valveInventorySlots, "Valve Inventory Slots UI");
            CheckField(valveInventoryBGSlots, "Valve Inventory Slots BGs UI");
            CheckField(valveProgressUI, "Valve Progress Bar UI");
            CheckField(valveSliderContainerUI, "Valve Progress Bar Slider Parent UI");
            #endregion

            #region Themed Key
            CheckField(tkInventorySlots, "Themed Key Inventory Slots UI");
            CheckField(tkInventoryBGSlots, "Themed Key Inventory BGs UI");
            #endregion

            #region Keycard
            CheckField(keycardInventorySlots, "Keycard Inventory Slots UI");
            CheckField(keycardInventoryBGSlots, "Keycard Inventory Slots BGs UI");
            #endregion

            #region Gas Mask
            CheckField(healthPercTextUI, "Gas Mask Health Percentage Text UI");
            CheckField(healthSliderUI, "Gas Mask Health Slider UI");

            CheckField(_maskIconUI, "Gas Mask Icon UI");
            CheckField(_filterIconUI, "Gas Mask Filter Icon UI");
            CheckField(_filterCountUI, "Gas Mask Filter Count UI");
            CheckField(_filterSliderUI, "Gas Mask Filter Slider UI");

            CheckField(visorImageOverlay, "Gas Mask Glass Overlay UI");
            #endregion

            #region Generator Fields
            CheckField(fuelFillUI, "Fuel Fill Slots UI");
            CheckField(currentFuelText, "Current Fuel Text UI");
            CheckField(maximumFuelText, "Maximum Fuel Text UI");
            #endregion

            #region Chess Fields
            CheckField(_slotWidgets, "Chess Slots UI");
            #endregion

            #region Crosshair
            CheckField(crosshair, "UICrosshair");
            #endregion
        }
        #endregion

        #region Check Field Debugging Helper Method Logic
        void CheckField<T>(T[] fields, string fieldName) where T : Object
        {
            if (fields == null || fields.Length == 0)
            {
                Debug.LogError($"FieldNullCheck: {fieldName} is not set or is empty!");
                return;
            }

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i] == null)
                {
                    Debug.LogError($"FieldNullCheck: {fieldName}[{i}] is not set in the inspector of " + gameObject.name);
                }
            }
        }

        void CheckField(Object field, string fieldName)
        {
            if (field == null)
            {
                Debug.LogError($"FieldNullCheck: {fieldName} is not set in the inspector of " + gameObject.name);
                return;
            }
        }
        #endregion
    }
}
