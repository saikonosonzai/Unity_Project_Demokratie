using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PortraitAssignmentPuzzle : MonoBehaviour
{
    [System.Serializable]
    public class PortraitSlot
    {
        [Header("Info")]
        public string personName = "Karl V.";

        [Header("Bildfläche im 3D-Raum")]
        public Renderer pictureRenderer;
        public int materialIndex = 0;

        [Header("Kamera-Position für diesen Rahmen")]
        public Transform cameraViewPoint;

        [Header("Materialien")]
        public Material emptyMaterial;
        public Material[] portraitMaterials = new Material[3];
        public string[] portraitDisplayNames = new string[3] { "Bild 1", "Bild 2", "Bild 3" };

        [Header("Lösung")]
        [Range(0, 2)]
        public int correctImageIndex = 0;

        [HideInInspector] public int currentImageIndex = -1;
        [HideInInspector] public bool confirmed = false;
    }

    [Header("Portrait Slots")]
    [SerializeField] private PortraitSlot[] portraitSlots;

    [Header("Events")]
    [SerializeField] private UnityEvent onPuzzleSolved;
    [SerializeField] private UnityEvent onWrongAssignment;

    [Header("Canvas")]
    [SerializeField] private Canvas puzzleCanvas;
    [SerializeField] private int sortingOrder = 500;

    [Header("Optional Button Sprites")]
    [SerializeField] private Sprite leftArrowSprite;
    [SerializeField] private Sprite rightArrowSprite;
    [SerializeField] private Sprite confirmButtonSprite;

    [Header("Farben")]
    [SerializeField] private Color nameColor = new Color(0.92f, 0.80f, 0.38f, 1f);
    [SerializeField] private Color selectionColor = new Color(0.95f, 0.90f, 0.75f, 1f);
    [SerializeField] private Color buttonTextColor = new Color(0.95f, 0.90f, 0.75f, 1f);
    [SerializeField] private Color buttonImageColor = Color.white;

    [Header("Layout")]
    [SerializeField] private Vector2 namePosition = new Vector2(0f, 380f);
    [SerializeField] private Vector2 leftArrowPosition = new Vector2(-260f, -40f);
    [SerializeField] private Vector2 rightArrowPosition = new Vector2(260f, -40f);
    [SerializeField] private Vector2 selectionPosition = new Vector2(0f, -110f);
    [SerializeField] private Vector2 confirmButtonPosition = new Vector2(0f, -210f);

    [SerializeField] private Vector2 nameSize = new Vector2(700f, 60f);
    [SerializeField] private Vector2 selectionSize = new Vector2(600f, 45f);
    [SerializeField] private Vector2 leftArrowSize = new Vector2(70f, 70f);
    [SerializeField] private Vector2 rightArrowSize = new Vector2(70f, 70f);
    [SerializeField] private Vector2 confirmButtonSize = new Vector2(280f, 60f);

    [Header("Textgrößen")]
    [SerializeField] private float nameFontSize = 30f;
    [SerializeField] private float selectionFontSize = 18f;
    [SerializeField] private float buttonFontSize = 21f;

    [Header("Fonts")]
    [SerializeField] private TMP_FontAsset titleFont;
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

    private TMP_Text personNameText;
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
            Debug.Log("TEST: Enter wurde gedrückt, bestätige aktuelle Auswahl.");
            ConfirmCurrentSelection();
        }
    }

    public void OpenSlot(int slotIndex)
    {
        if (puzzleSolved)
        {
            Debug.Log("TEST: Puzzle ist bereits gelöst.");
            return;
        }

        if (portraitSlots == null || portraitSlots.Length == 0)
        {
            Debug.LogWarning("TEST: Keine Portrait Slots eingetragen.");
            return;
        }

        if (slotIndex < 0 || slotIndex >= portraitSlots.Length)
        {
            Debug.LogWarning("TEST: Ungültiger Slot Index: " + slotIndex);
            return;
        }

        currentSlotIndex = slotIndex;
        uiOpen = true;

        Debug.Log("TEST: Slot geöffnet: " + slotIndex + " | " + portraitSlots[slotIndex].personName);

        ShowUI();
        SetGameplayScriptsActive(false);
        SetCursorForUI(true);
        MoveCameraToSlot(portraitSlots[slotIndex]);
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

        Debug.Log("TEST: Puzzle UI geschlossen.");
    }

    public void SelectPreviousImage()
    {
        if (!CanEditCurrentSlot())
        {
            Debug.LogWarning("TEST: Linker Pfeil gedrückt, aber kein aktiver Slot offen.");
            return;
        }

        PortraitSlot slot = portraitSlots[currentSlotIndex];

        slot.currentImageIndex--;

        if (slot.currentImageIndex < -1)
            slot.currentImageIndex = 2;

        slot.confirmed = false;

        ApplyCurrentMaterial(slot);
        UpdateUIForCurrentSlot();

        Debug.Log("TEST: Vorheriges Bild gewählt für " + slot.personName + " | Bildindex: " + slot.currentImageIndex);
    }

    public void SelectNextImage()
    {
        if (!CanEditCurrentSlot())
        {
            Debug.LogWarning("TEST: Rechter Pfeil gedrückt, aber kein aktiver Slot offen.");
            return;
        }

        PortraitSlot slot = portraitSlots[currentSlotIndex];

        slot.currentImageIndex++;

        if (slot.currentImageIndex > 2)
            slot.currentImageIndex = -1;

        slot.confirmed = false;

        ApplyCurrentMaterial(slot);
        UpdateUIForCurrentSlot();

        Debug.Log("TEST: Nächstes Bild gewählt für " + slot.personName + " | Bildindex: " + slot.currentImageIndex);
    }

    public void ConfirmCurrentSelection()
    {
        Debug.Log("TEST: ConfirmCurrentSelection wurde aufgerufen.");

        if (!CanEditCurrentSlot())
        {
            Debug.LogWarning("TEST: Bestätigen gedrückt, aber kein aktiver Slot offen.");
            return;
        }

        PortraitSlot slot = portraitSlots[currentSlotIndex];

        slot.confirmed = true;

        bool isCorrect = slot.currentImageIndex == slot.correctImageIndex;

        if (isCorrect)
            Debug.Log(slot.personName + ": richtig");
        else
            Debug.Log(slot.personName + ": falsch");

        Debug.Log(
            "TEST: " + slot.personName +
            " | Ausgewählt: " + slot.currentImageIndex +
            " | Richtig wäre: " + slot.correctImageIndex +
            " | Confirmed: " + slot.confirmed
        );

        ClosePuzzleUI();
        CheckPuzzleState();
    }

    private bool CanEditCurrentSlot()
    {
        return uiOpen &&
               !puzzleSolved &&
               portraitSlots != null &&
               currentSlotIndex >= 0 &&
               currentSlotIndex < portraitSlots.Length;
    }

    private void CheckPuzzleState()
    {
        if (portraitSlots == null || portraitSlots.Length == 0)
        {
            Debug.LogWarning("TEST: Keine Portrait Slots eingetragen. Prüfung abgebrochen.");
            return;
        }

        Debug.Log("TEST: Prüfe gesamtes Kaiserbilder-Rätsel...");

        for (int i = 0; i < portraitSlots.Length; i++)
        {
            PortraitSlot slot = portraitSlots[i];

            Debug.Log(
                "TEST: Slot " + i +
                " | Name: " + slot.personName +
                " | Bestätigt: " + slot.confirmed +
                " | Ausgewählt: " + slot.currentImageIndex +
                " | Richtig: " + slot.correctImageIndex
            );
        }

        for (int i = 0; i < portraitSlots.Length; i++)
        {
            if (!portraitSlots[i].confirmed)
            {
                Debug.Log("TEST: Noch nicht alle Portraits wurden bestätigt.");
                return;
            }
        }

        for (int i = 0; i < portraitSlots.Length; i++)
        {
            if (portraitSlots[i].currentImageIndex != portraitSlots[i].correctImageIndex)
            {
                Debug.Log(
                    "TEST: GESAMTES RÄTSEL FALSCH. Fehler bei " +
                    portraitSlots[i].personName +
                    " | Ausgewählt: " + portraitSlots[i].currentImageIndex +
                    " | Richtig wäre: " + portraitSlots[i].correctImageIndex
                );

                onWrongAssignment?.Invoke();
                return;
            }
        }

        puzzleSolved = true;

        Debug.Log("TEST: DAS KAISERBILDER-RÄTSEL WURDE GELÖST!");

        onPuzzleSolved?.Invoke();
    }

    private void InitializeSlots()
    {
        if (portraitSlots == null)
        {
            Debug.LogWarning("TEST: Portrait Slots Array ist null.");
            return;
        }

        for (int i = 0; i < portraitSlots.Length; i++)
        {
            PortraitSlot slot = portraitSlots[i];

            if (slot == null)
                continue;

            slot.currentImageIndex = -1;
            slot.confirmed = false;

            ApplyCurrentMaterial(slot);

            Debug.Log("TEST: Slot initialisiert: " + i + " | " + slot.personName);
        }
    }

    private void ApplyCurrentMaterial(PortraitSlot slot)
    {
        if (slot == null)
            return;

        if (slot.pictureRenderer == null)
        {
            Debug.LogWarning("TEST: Picture Renderer fehlt bei Slot: " + slot.personName);
            return;
        }

        Material materialToApply = GetMaterialForSlot(slot);

        if (materialToApply == null)
        {
            Debug.LogWarning("TEST: Kein Material für Slot eingetragen: " + slot.personName + " | Bildindex: " + slot.currentImageIndex);
            return;
        }

        Material[] materials = slot.pictureRenderer.materials;

        if (materials == null || materials.Length == 0)
        {
            slot.pictureRenderer.material = materialToApply;
            Debug.Log("TEST: Material direkt gesetzt bei " + slot.personName + " | Bildindex: " + slot.currentImageIndex);
            return;
        }

        int index = Mathf.Clamp(slot.materialIndex, 0, materials.Length - 1);
        materials[index] = materialToApply;
        slot.pictureRenderer.materials = materials;

        Debug.Log("TEST: Material gewechselt bei " + slot.personName + " | Material Index: " + index + " | Bildindex: " + slot.currentImageIndex);
    }

    private Material GetMaterialForSlot(PortraitSlot slot)
    {
        if (slot.currentImageIndex == -1)
            return slot.emptyMaterial;

        if (slot.portraitMaterials == null)
            return null;

        if (slot.currentImageIndex < 0 || slot.currentImageIndex >= slot.portraitMaterials.Length)
            return null;

        return slot.portraitMaterials[slot.currentImageIndex];
    }

    private void UpdateUIForCurrentSlot()
    {
        if (currentSlotIndex < 0 || currentSlotIndex >= portraitSlots.Length)
            return;

        PortraitSlot slot = portraitSlots[currentSlotIndex];

        if (personNameText != null)
            personNameText.text = slot.personName.ToUpper();

        if (currentSelectionText != null)
        {
            if (slot.currentImageIndex == -1)
            {
                currentSelectionText.text = "Kein Bild ausgewählt";
            }
            else
            {
                string displayName = "Bild " + (slot.currentImageIndex + 1);

                if (slot.portraitDisplayNames != null &&
                    slot.currentImageIndex >= 0 &&
                    slot.currentImageIndex < slot.portraitDisplayNames.Length &&
                    !string.IsNullOrWhiteSpace(slot.portraitDisplayNames[slot.currentImageIndex]))
                {
                    displayName = slot.portraitDisplayNames[slot.currentImageIndex];
                }

                currentSelectionText.text = displayName;
            }
        }
    }

    private void MoveCameraToSlot(PortraitSlot slot)
    {
        if (!moveMainCameraToViewPoint)
            return;

        if (mainCamera == null)
        {
            Debug.LogWarning("TEST: Main Camera ist nicht eingetragen.");
            return;
        }

        if (slot == null || slot.cameraViewPoint == null)
        {
            Debug.LogWarning("TEST: Camera View Point fehlt bei diesem Slot.");
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

        Debug.Log("TEST: Kamera auf ViewPoint gesetzt für " + slot.personName);
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

        Debug.Log("TEST: Kamera zurückgesetzt.");
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

                Debug.Log("TEST: Script deaktiviert: " + script.GetType().Name);
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

                Debug.Log("TEST: Script wieder aktiviert: " + script.GetType().Name);
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
                "PortraitPuzzleCanvas",
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

        personNameText = CreateText(
            rootObject.transform,
            "PersonName",
            "",
            nameFontSize,
            nameColor,
            FontStyles.Bold,
            namePosition,
            nameSize,
            TextAlignmentOptions.Center
        );

        currentSelectionText = CreateText(
            rootObject.transform,
            "SelectionText",
            "",
            selectionFontSize,
            selectionColor,
            FontStyles.Normal,
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
        leftButton.onClick.AddListener(SelectPreviousImage);

        rightButton = CreateButton(
            rootObject.transform,
            "RightButton",
            rightArrowPosition,
            rightArrowSize,
            rightArrowSprite,
            "→"
        );
        rightButton.onClick.AddListener(SelectNextImage);

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
            Debug.Log("TEST: Bestätigen-Button wurde wirklich geklickt.");
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

        if (fontStyle == FontStyles.Bold && titleFont != null)
            tmp.font = titleFont;
        else if (textFont != null)
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

        Debug.Log("TEST: EventSystem wurde automatisch erstellt.");
    }
}