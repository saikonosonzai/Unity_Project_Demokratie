using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatueAssignmentPuzzle : MonoBehaviour
{
    [System.Serializable]
    public class StatueSlot
    {
        [Header("Interner Name nur für Console")]
        public string slotName = "Statue 1";

        [Header("Sprite-Anzeige im 3D-Raum")]
        public SpriteRenderer symbolRenderer;

        [Header("Kamera-Position für diese Statue")]
        public Transform cameraViewPoint;

        [Header("Sprites")]
        public Sprite emptySprite;
        public Sprite[] symbolSprites = new Sprite[5];

        [Header("Anzeige-Namen im UI")]
        public string[] symbolDisplayNames = new string[5]
        {
            "Symbol 1",
            "Symbol 2",
            "Symbol 3",
            "Symbol 4",
            "Symbol 5"
        };

        [Header("Lösung")]
        [Tooltip("Welches Symbol ist richtig? 0 = erstes Symbol, 1 = zweites Symbol, 2 = drittes Symbol, 3 = viertes Symbol, 4 = fünftes Symbol")]
        [Range(0, 4)]
        public int correctSymbolIndex = 0;

        [HideInInspector] public int currentSymbolIndex = -1;
        [HideInInspector] public bool confirmed = false;
    }

    [Header("Statue Slots")]
    [SerializeField] private StatueSlot[] statueSlots;

    [Header("Events")]
    [SerializeField] private UnityEvent onPuzzleSolved;
    [SerializeField] private UnityEvent onWrongAssignment;

    [Header("Canvas")]
    [SerializeField] private Canvas puzzleCanvas;
    [SerializeField] private int sortingOrder = 500;

    [Header("Button Sprites")]
    [SerializeField] private Sprite leftArrowSprite;
    [SerializeField] private Sprite rightArrowSprite;
    [SerializeField] private Sprite confirmButtonSprite;

    [Header("Farben")]
    [SerializeField] private Color selectionColor = new Color(0.95f, 0.90f, 0.75f, 1f);
    [SerializeField] private Color buttonTextColor = new Color(0.95f, 0.90f, 0.75f, 1f);
    [SerializeField] private Color buttonImageColor = Color.white;

    [Header("Layout")]
    [SerializeField] private Vector2 leftArrowPosition = new Vector2(-260f, -40f);
    [SerializeField] private Vector2 rightArrowPosition = new Vector2(260f, -40f);
    [SerializeField] private Vector2 selectionPosition = new Vector2(0f, -110f);
    [SerializeField] private Vector2 confirmButtonPosition = new Vector2(0f, -210f);

    [SerializeField] private Vector2 selectionSize = new Vector2(600f, 45f);
    [SerializeField] private Vector2 leftArrowSize = new Vector2(70f, 70f);
    [SerializeField] private Vector2 rightArrowSize = new Vector2(70f, 70f);
    [SerializeField] private Vector2 confirmButtonSize = new Vector2(280f, 60f);

    [Header("Textgrößen")]
    [SerializeField] private float selectionFontSize = 22f;
    [SerializeField] private float buttonFontSize = 21f;

    [Header("Fonts")]
    [SerializeField] private TMP_FontAsset textFont;
    [SerializeField] private TMP_FontAsset buttonFont;

    [Header("Input im Canvas")]
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;
    [SerializeField] private KeyCode confirmKey = KeyCode.Return;

    [Header("Cursor")]
    [SerializeField] private bool unlockCursorWhileOpen = true;

    [Header("Gameplay-Skripte deaktivieren")]
    [SerializeField] private MonoBehaviour[] scriptsToDisableWhileOpen;

    [Header("Kamera-Fokus")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool moveMainCameraToViewPoint = true;
    [SerializeField] private bool restoreCameraAfterClose = true;

    private GameObject rootObject;

    private TMP_Text currentSelectionText;

    private Button leftButton;
    private Button rightButton;
    private Button confirmButton;

    private bool uiOpen;
    private bool puzzleSolved;
    private int currentSlotIndex = -1;

    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;
    private Transform savedCameraParent;
    private bool cameraTransformSaved;

    private CursorLockMode savedCursorLockMode;
    private bool savedCursorVisible;

    private readonly Dictionary<MonoBehaviour, bool> previousScriptStates = new Dictionary<MonoBehaviour, bool>();

    private void Awake()
    {
        BuildUIIfNeeded();
        InitializeSlots();
        HideUIInstant();
    }

    private void Update()
    {
        if (!uiOpen)
            return;

        if (Input.GetKeyDown(closeKey))
        {
            ClosePuzzleUI();
            return;
        }

        if (Input.GetKeyDown(confirmKey))
        {
            ConfirmCurrentSelection();
        }
    }

    public void OpenSlot(int slotIndex)
    {
        if (puzzleSolved)
        {
            Debug.Log("STATUE TEST: Puzzle ist bereits gelöst.");
            return;
        }

        if (statueSlots == null || statueSlots.Length == 0)
        {
            Debug.LogWarning("STATUE TEST: Keine Statue Slots eingetragen.");
            return;
        }

        if (slotIndex < 0 || slotIndex >= statueSlots.Length)
        {
            Debug.LogWarning("STATUE TEST: Ungültiger Slot Index: " + slotIndex);
            return;
        }

        currentSlotIndex = slotIndex;
        uiOpen = true;

        Debug.Log("STATUE TEST: Slot geöffnet: " + slotIndex + " | " + statueSlots[slotIndex].slotName);

        ShowUI();
        SetGameplayScriptsActive(false);
        SetCursorForUI(true);
        MoveCameraToSlot(statueSlots[slotIndex]);
        UpdateUIForCurrentSlot();
    }

    public void ClosePuzzleUI()
    {
        uiOpen = false;
        currentSlotIndex = -1;

        HideUIInstant();

        if (restoreCameraAfterClose)
            RestoreCamera();

        SetGameplayScriptsActive(true);
        SetCursorForUI(false);

        Debug.Log("STATUE TEST: Puzzle UI geschlossen.");
    }

    public void SelectPreviousSymbol()
    {
        if (!CanEditCurrentSlot())
            return;

        StatueSlot slot = statueSlots[currentSlotIndex];

        slot.currentSymbolIndex--;

        if (slot.currentSymbolIndex < -1)
            slot.currentSymbolIndex = 4;

        slot.confirmed = false;

        ApplyCurrentSprite(slot);
        UpdateUIForCurrentSlot();

        Debug.Log("STATUE TEST: Vorheriges Symbol gewählt für " + slot.slotName + " | Index: " + slot.currentSymbolIndex);
    }

    public void SelectNextSymbol()
    {
        if (!CanEditCurrentSlot())
            return;

        StatueSlot slot = statueSlots[currentSlotIndex];

        slot.currentSymbolIndex++;

        if (slot.currentSymbolIndex > 4)
            slot.currentSymbolIndex = -1;

        slot.confirmed = false;

        ApplyCurrentSprite(slot);
        UpdateUIForCurrentSlot();

        Debug.Log("STATUE TEST: Nächstes Symbol gewählt für " + slot.slotName + " | Index: " + slot.currentSymbolIndex);
    }

    public void ConfirmCurrentSelection()
    {
        if (!CanEditCurrentSlot())
        {
            Debug.LogWarning("STATUE TEST: Bestätigen gedrückt, aber kein aktiver Slot offen.");
            return;
        }

        StatueSlot slot = statueSlots[currentSlotIndex];
        slot.confirmed = true;

        bool isCorrect = slot.currentSymbolIndex == slot.correctSymbolIndex;

        if (isCorrect)
            Debug.Log(slot.slotName + ": richtig");
        else
            Debug.Log(slot.slotName + ": falsch");

        Debug.Log(
            "STATUE TEST: " + slot.slotName +
            " | Ausgewählt: " + slot.currentSymbolIndex +
            " | Richtig wäre: " + slot.correctSymbolIndex +
            " | Confirmed: " + slot.confirmed
        );

        ClosePuzzleUI();
        CheckPuzzleState();
    }

    private bool CanEditCurrentSlot()
    {
        return uiOpen &&
               !puzzleSolved &&
               statueSlots != null &&
               currentSlotIndex >= 0 &&
               currentSlotIndex < statueSlots.Length;
    }

    private void CheckPuzzleState()
    {
        if (statueSlots == null || statueSlots.Length == 0)
        {
            Debug.LogWarning("STATUE TEST: Keine Statue Slots eingetragen.");
            return;
        }

        Debug.Log("STATUE TEST: Prüfe gesamtes Statuen-Symbol-Rätsel...");

        for (int i = 0; i < statueSlots.Length; i++)
        {
            StatueSlot slot = statueSlots[i];

            Debug.Log(
                "STATUE TEST: Slot " + i +
                " | Name: " + slot.slotName +
                " | Bestätigt: " + slot.confirmed +
                " | Ausgewählt: " + slot.currentSymbolIndex +
                " | Richtig: " + slot.correctSymbolIndex
            );
        }

        for (int i = 0; i < statueSlots.Length; i++)
        {
            if (!statueSlots[i].confirmed)
            {
                Debug.Log("STATUE TEST: Noch nicht alle Symbole wurden bestätigt.");
                return;
            }
        }

        for (int i = 0; i < statueSlots.Length; i++)
        {
            if (statueSlots[i].currentSymbolIndex != statueSlots[i].correctSymbolIndex)
            {
                Debug.Log(
                    "STATUE TEST: GESAMTES RÄTSEL FALSCH. Fehler bei " +
                    statueSlots[i].slotName +
                    " | Ausgewählt: " + statueSlots[i].currentSymbolIndex +
                    " | Richtig wäre: " + statueSlots[i].correctSymbolIndex
                );

                onWrongAssignment?.Invoke();
                return;
            }
        }

        puzzleSolved = true;

        Debug.Log("STATUE TEST: DAS STATUEN-SYMBOL-RÄTSEL WURDE GELÖST!");

        onPuzzleSolved?.Invoke();
    }

    private void InitializeSlots()
    {
        if (statueSlots == null)
        {
            Debug.LogWarning("STATUE TEST: Statue Slots Array ist null.");
            return;
        }

        for (int i = 0; i < statueSlots.Length; i++)
        {
            StatueSlot slot = statueSlots[i];

            if (slot == null)
                continue;

            slot.currentSymbolIndex = -1;
            slot.confirmed = false;

            ApplyCurrentSprite(slot);

            Debug.Log("STATUE TEST: Slot initialisiert: " + i + " | " + slot.slotName);
        }
    }

    private void ApplyCurrentSprite(StatueSlot slot)
    {
        if (slot == null)
            return;

        if (slot.symbolRenderer == null)
        {
            Debug.LogWarning("STATUE TEST: Symbol Renderer fehlt bei Slot: " + slot.slotName);
            return;
        }

        Sprite spriteToApply = GetSpriteForSlot(slot);

        slot.symbolRenderer.sprite = spriteToApply;

        Debug.Log("STATUE TEST: Sprite gewechselt bei " + slot.slotName + " | Symbolindex: " + slot.currentSymbolIndex);
    }

    private Sprite GetSpriteForSlot(StatueSlot slot)
    {
        if (slot.currentSymbolIndex == -1)
            return slot.emptySprite;

        if (slot.symbolSprites == null)
            return null;

        if (slot.currentSymbolIndex < 0 || slot.currentSymbolIndex >= slot.symbolSprites.Length)
            return null;

        return slot.symbolSprites[slot.currentSymbolIndex];
    }

    private void UpdateUIForCurrentSlot()
    {
        if (currentSlotIndex < 0 || currentSlotIndex >= statueSlots.Length)
            return;

        StatueSlot slot = statueSlots[currentSlotIndex];

        if (currentSelectionText != null)
        {
            if (slot.currentSymbolIndex == -1)
            {
                currentSelectionText.text = "Kein Symbol ausgewählt";
            }
            else
            {
                string displayName = "Symbol " + (slot.currentSymbolIndex + 1);

                if (slot.symbolDisplayNames != null &&
                    slot.currentSymbolIndex >= 0 &&
                    slot.currentSymbolIndex < slot.symbolDisplayNames.Length &&
                    !string.IsNullOrWhiteSpace(slot.symbolDisplayNames[slot.currentSymbolIndex]))
                {
                    displayName = slot.symbolDisplayNames[slot.currentSymbolIndex];
                }

                currentSelectionText.text = displayName;
            }
        }
    }

    private void MoveCameraToSlot(StatueSlot slot)
    {
        if (!moveMainCameraToViewPoint)
            return;

        if (mainCamera == null)
        {
            Debug.LogWarning("STATUE TEST: Main Camera ist nicht eingetragen.");
            return;
        }

        if (slot == null || slot.cameraViewPoint == null)
        {
            Debug.LogWarning("STATUE TEST: Camera View Point fehlt bei diesem Slot.");
            return;
        }

        if (!cameraTransformSaved)
        {
            savedCameraParent = mainCamera.transform.parent;
            savedCameraPosition = mainCamera.transform.position;
            savedCameraRotation = mainCamera.transform.rotation;
            cameraTransformSaved = true;
        }

        mainCamera.transform.SetParent(null);
        mainCamera.transform.position = slot.cameraViewPoint.position;
        mainCamera.transform.rotation = slot.cameraViewPoint.rotation;

        Debug.Log("STATUE TEST: Kamera auf ViewPoint gesetzt für " + slot.slotName);
    }

    private void RestoreCamera()
    {
        if (!cameraTransformSaved)
            return;

        if (mainCamera == null)
            return;

        mainCamera.transform.SetParent(savedCameraParent);
        mainCamera.transform.position = savedCameraPosition;
        mainCamera.transform.rotation = savedCameraRotation;

        cameraTransformSaved = false;

        Debug.Log("STATUE TEST: Kamera zurückgesetzt.");
    }

    private void SetGameplayScriptsActive(bool active)
    {
        if (scriptsToDisableWhileOpen == null || scriptsToDisableWhileOpen.Length == 0)
            return;

        if (!active)
        {
            previousScriptStates.Clear();

            foreach (MonoBehaviour script in scriptsToDisableWhileOpen)
            {
                if (script == null)
                    continue;

                previousScriptStates[script] = script.enabled;
                script.enabled = false;
            }
        }
        else
        {
            foreach (MonoBehaviour script in scriptsToDisableWhileOpen)
            {
                if (script == null)
                    continue;

                if (previousScriptStates.TryGetValue(script, out bool wasEnabled))
                    script.enabled = wasEnabled;
                else
                    script.enabled = true;
            }

            previousScriptStates.Clear();
        }
    }

    private void SetCursorForUI(bool visibleState)
    {
        if (visibleState)
        {
            savedCursorVisible = Cursor.visible;
            savedCursorLockMode = Cursor.lockState;

            if (unlockCursorWhileOpen)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else
        {
            Cursor.visible = savedCursorVisible;
            Cursor.lockState = savedCursorLockMode;
        }
    }

    private void BuildUIIfNeeded()
    {
        EnsureEventSystem();

        if (puzzleCanvas == null)
        {
            GameObject canvasObject = new GameObject(
                "StatueSymbolPuzzleCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

            puzzleCanvas = canvasObject.GetComponent<Canvas>();
            puzzleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            puzzleCanvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        rootObject = new GameObject("Root", typeof(RectTransform));
        rootObject.transform.SetParent(puzzleCanvas.transform, false);

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        currentSelectionText = CreateText(
            rootObject.transform,
            "SelectionText",
            "",
            selectionFontSize,
            selectionColor,
            FontStyles.Bold,
            selectionPosition,
            selectionSize,
            TextAlignmentOptions.Center
        );

        leftButton = CreateButton(
            rootObject.transform,
            "LeftButton",
            leftArrowPosition,
            leftArrowSize,
            leftArrowSprite,
            "←"
        );

        leftButton.onClick.AddListener(SelectPreviousSymbol);

        rightButton = CreateButton(
            rootObject.transform,
            "RightButton",
            rightArrowPosition,
            rightArrowSize,
            rightArrowSprite,
            "→"
        );

        rightButton.onClick.AddListener(SelectNextSymbol);

        confirmButton = CreateButton(
            rootObject.transform,
            "ConfirmButton",
            confirmButtonPosition,
            confirmButtonSize,
            confirmButtonSprite,
            "BESTÄTIGEN"
        );

        confirmButton.onClick.AddListener(() =>
        {
            Debug.Log("STATUE TEST: Bestätigen-Button wurde geklickt.");
            ConfirmCurrentSelection();
        });
    }

    private TMP_Text CreateText(
        Transform parent,
        string objectName,
        string text,
        float fontSize,
        Color color,
        FontStyles fontStyle,
        Vector2 anchoredPosition,
        Vector2 size,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = fontStyle;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;

        if (textFont != null)
            tmp.font = textFont;

        return tmp;
    }

    private Button CreateButton(
        Transform parent,
        string objectName,
        Vector2 anchoredPosition,
        Vector2 size,
        Sprite sprite,
        string label)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.GetComponent<Image>();
        image.raycastTarget = true;

        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.color = buttonImageColor;
            image.preserveAspect = true;
        }
        else
        {
            image.color = new Color(0f, 0f, 0f, 0.25f);
        }

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.interactable = true;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.85f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.4f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
        tmp.text = sprite != null && (label == "←" || label == "→") ? "" : label;
        tmp.fontSize = buttonFontSize;
        tmp.color = buttonTextColor;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        if (buttonFont != null)
            tmp.font = buttonFont;

        return button;
    }

    private void ShowUI()
    {
        if (rootObject != null)
            rootObject.SetActive(true);
    }

    private void HideUIInstant()
    {
        if (rootObject != null)
            rootObject.SetActive(false);
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
            return;

        GameObject eventSystemObject = new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule)
        );

        Debug.Log("STATUE TEST: EventSystem wurde automatisch erstellt.");
    }
}