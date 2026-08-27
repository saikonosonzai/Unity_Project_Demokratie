using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BremenPauseMenuUI : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string mainMenuSceneName = "Startscreen";

    [Header("Pause Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("Player Control Scripts To Disable")]
    [Tooltip("Hier deine FPS Controller / Mouse Look / Player Movement Scripts reinziehen.")]
    [SerializeField] private MonoBehaviour[] scriptsToDisableOnPause;

    [Header("Background")]
    [SerializeField] private Color backgroundOverlayColor = new Color(0f, 0f, 0f, 0.78f);
    [SerializeField] private Sprite fullscreenBackgroundSprite;

    [Header("Fonts")]
    [SerializeField] private TMP_FontAsset titleFont;
    [SerializeField] private TMP_FontAsset buttonFont;
    [SerializeField] private TMP_FontAsset textFont;

    [Header("Button Sprites")]
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private Sprite backButtonSprite;

    [Header("Pause Menu Texts")]
    [SerializeField] private string pauseTitle = "Pause";
    [SerializeField] private string resumeButtonText = "Weiterspielen";
    [SerializeField] private string settingsButtonText = "Einstellungen";
    [SerializeField] private string controlsButtonText = "Steuerung";
    [SerializeField] private string mainMenuButtonText = "Zurück zum Hauptmenü";

    [Header("Page Titles")]
    [SerializeField] private string settingsPageTitle = "Einstellungen";
    [SerializeField] private string controlsPageTitle = "Steuerung";

    [Header("Settings Texts")]
    [SerializeField] private string soundLabelText = "Sound";
    [SerializeField] private string graphicsLabelText = "Grafik";

    [Header("Graphics Options")]
    [SerializeField] private string graphicsLowText = "Niedrig";
    [SerializeField] private string graphicsMediumText = "Mittel";
    [SerializeField] private string graphicsHighText = "Hoch";
    [SerializeField] private string graphicsUltraText = "Ultra";

    [Header("Controls Text")]
    [TextArea(6, 14)]
    [SerializeField] private string controlsPageText =
        "W / A / S / D  -  Bewegen\n" +
        "Maus  -  Umsehen\n" +
        "E  -  Interagieren\n" +
        "Leertaste  -  Springen\n" +
        "Shift  -  Rennen\n" +
        "Esc  -  Pausemenü";

    [Header("Colors")]
    [SerializeField] private Color titleColor = new Color(0.82f, 0.68f, 0.36f, 1f);
    [SerializeField] private Color normalTextColor = new Color(0.88f, 0.78f, 0.50f, 1f);
    [SerializeField] private Color buttonColor = new Color(0.08f, 0.04f, 0.02f, 0.78f);
    [SerializeField] private Color buttonHoverColor = new Color(0.32f, 0.16f, 0.06f, 0.88f);
    [SerializeField] private Color buttonTextColor = new Color(0.88f, 0.76f, 0.50f, 1f);
    [SerializeField] private Color borderColor = new Color(0.65f, 0.48f, 0.20f, 1f);

    [Header("Text Gradient")]
    [SerializeField] private bool useTextGradient = true;

    [SerializeField] private Color titleTopLeft = new Color(0.95f, 0.82f, 0.42f, 1f);
    [SerializeField] private Color titleTopRight = new Color(0.82f, 0.68f, 0.34f, 1f);
    [SerializeField] private Color titleBottomLeft = new Color(0.42f, 0.30f, 0.10f, 1f);
    [SerializeField] private Color titleBottomRight = new Color(0.70f, 0.54f, 0.22f, 1f);

    [SerializeField] private Color normalTopLeft = new Color(0.90f, 0.80f, 0.52f, 1f);
    [SerializeField] private Color normalTopRight = new Color(0.78f, 0.66f, 0.38f, 1f);
    [SerializeField] private Color normalBottomLeft = new Color(0.50f, 0.38f, 0.16f, 1f);
    [SerializeField] private Color normalBottomRight = new Color(0.68f, 0.54f, 0.28f, 1f);

    [Header("Layout")]
    [SerializeField] private Vector2 titlePosition = new Vector2(0f, 250f);
    [SerializeField] private Vector2 buttonStartPosition = new Vector2(0f, 110f);
    [SerializeField] private float buttonSpacing = 72f;
    [SerializeField] private Vector2 buttonSize = new Vector2(430f, 58f);

    [Header("Submenu Layout")]
    [SerializeField] private Vector2 submenuTitlePosition = new Vector2(0f, 250f);
    [SerializeField] private Vector2 submenuContentPosition = new Vector2(0f, 20f);

    [Header("Back Button")]
    [SerializeField] private Vector2 backIconPosition = new Vector2(-850f, 455f);
    [SerializeField] private Vector2 backIconSize = new Vector2(72f, 72f);

    [Header("Text Box Sizes")]
    [SerializeField] private Vector2 controlsTextBoxSize = new Vector2(700f, 380f);
    [SerializeField] private float controlsTextFontSize = 30f;

    private GameObject pauseRoot;
    private GameObject mainPauseRoot;
    private GameObject settingsRoot;
    private GameObject controlsRoot;

    private Slider volumeSlider;
    private TMP_Dropdown graphicsDropdown;

    private bool isPaused;
    private bool[] previousScriptStates;

    private void Awake()
    {
        EnsureEventSystem();
        BuildCanvas();
        BuildPauseMenu();

        pauseRoot.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                ResumeGame();
            else
                OpenPauseMenu();
        }
    }

    private void EnsureEventSystem()
    {
        EventSystem existingEventSystem = FindObjectOfType<EventSystem>();

        if (existingEventSystem != null)
            return;

        GameObject eventSystemObject = new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule)
        );

        DontDestroyOnLoad(eventSystemObject);
    }

    private void BuildCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 3000;

        CanvasScaler scaler = GetComponent<CanvasScaler>();

        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();

        if (raycaster == null)
            gameObject.AddComponent<GraphicRaycaster>();

        RectTransform rect = GetComponent<RectTransform>();

        if (rect == null)
            rect = gameObject.AddComponent<RectTransform>();

        StretchFull(rect);
    }

    private void BuildPauseMenu()
    {
        pauseRoot = CreateUIObject("PauseRoot", transform);
        StretchFull(pauseRoot.GetComponent<RectTransform>());

        GameObject backgroundObject = CreateUIObject("FullscreenPauseBackground", pauseRoot.transform);
        StretchFull(backgroundObject.GetComponent<RectTransform>());

        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = backgroundOverlayColor;
        backgroundImage.raycastTarget = true;

        if (fullscreenBackgroundSprite != null)
        {
            backgroundImage.sprite = fullscreenBackgroundSprite;
            backgroundImage.color = Color.white;
            backgroundImage.preserveAspect = false;
        }

        BuildMainPausePage();
        BuildSettingsPage();
        BuildControlsPage();

        ShowMainPausePage();
    }

    private void BuildMainPausePage()
    {
        mainPauseRoot = CreateUIObject("MainPausePage", pauseRoot.transform);
        StretchFull(mainPauseRoot.GetComponent<RectTransform>());

        TMP_Text title = CreateText(
            "PauseTitle",
            pauseTitle,
            mainPauseRoot.transform,
            titlePosition,
            new Vector2(900f, 100f),
            64f,
            titleColor,
            FontStyles.Bold
        );

        if (titleFont != null)
            title.font = titleFont;

        title.alignment = TextAlignmentOptions.Center;
        ApplyTitleGradient(title);
        AddTextShadow(title.gameObject, new Color(0f, 0f, 0f, 0.85f), new Vector2(3f, -3f));

        CreateButton(resumeButtonText, mainPauseRoot.transform, buttonStartPosition, ResumeGame);
        CreateButton(settingsButtonText, mainPauseRoot.transform, buttonStartPosition + new Vector2(0f, -buttonSpacing), ShowSettingsPage);
        CreateButton(controlsButtonText, mainPauseRoot.transform, buttonStartPosition + new Vector2(0f, -buttonSpacing * 2f), ShowControlsPage);
        CreateButton(mainMenuButtonText, mainPauseRoot.transform, buttonStartPosition + new Vector2(0f, -buttonSpacing * 3f), LoadMainMenu);
    }

    private void BuildSettingsPage()
    {
        settingsRoot = CreateUIObject("SettingsPage", pauseRoot.transform);
        StretchFull(settingsRoot.GetComponent<RectTransform>());

        CreateBackIconButton(settingsRoot.transform);
        CreateHeadline(settingsPageTitle, settingsRoot.transform);

        TMP_Text soundLabel = CreateText(
            "SoundLabel",
            soundLabelText,
            settingsRoot.transform,
            new Vector2(-230f, 80f),
            new Vector2(240f, 55f),
            30f,
            normalTextColor,
            FontStyles.Bold
        );

        ApplyNormalGradient(soundLabel);

        volumeSlider = CreateSlider(settingsRoot.transform, new Vector2(130f, 80f), new Vector2(360f, 40f));
        volumeSlider.value = AudioListener.volume;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        TMP_Text graphicsLabel = CreateText(
            "GraphicsLabel",
            graphicsLabelText,
            settingsRoot.transform,
            new Vector2(-230f, -20f),
            new Vector2(240f, 55f),
            30f,
            normalTextColor,
            FontStyles.Bold
        );

        ApplyNormalGradient(graphicsLabel);

        graphicsDropdown = CreateDropdown(settingsRoot.transform, new Vector2(130f, -20f), new Vector2(360f, 60f));
        graphicsDropdown.ClearOptions();
        graphicsDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            graphicsLowText,
            graphicsMediumText,
            graphicsHighText,
            graphicsUltraText
        });

        graphicsDropdown.value = Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, graphicsDropdown.options.Count - 1);
        graphicsDropdown.RefreshShownValue();
        graphicsDropdown.onValueChanged.AddListener(OnGraphicsChanged);
    }

    private void BuildControlsPage()
    {
        controlsRoot = CreateUIObject("ControlsPage", pauseRoot.transform);
        StretchFull(controlsRoot.GetComponent<RectTransform>());

        CreateBackIconButton(controlsRoot.transform);
        CreateHeadline(controlsPageTitle, controlsRoot.transform);

        TMP_Text controlsText = CreateText(
            "ControlsText",
            controlsPageText,
            controlsRoot.transform,
            submenuContentPosition,
            controlsTextBoxSize,
            controlsTextFontSize,
            normalTextColor,
            FontStyles.Normal
        );

        controlsText.alignment = TextAlignmentOptions.Center;
        controlsText.verticalAlignment = VerticalAlignmentOptions.Middle;
        ApplyNormalGradient(controlsText);
    }

    private void OpenPauseMenu()
    {
        isPaused = true;

        pauseRoot.SetActive(true);
        ShowMainPausePage();

        Time.timeScale = 0f;

        DisablePlayerControls();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ResumeGame()
    {
        isPaused = false;

        pauseRoot.SetActive(false);

        Time.timeScale = 1f;

        RestorePlayerControls();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void DisablePlayerControls()
    {
        if (scriptsToDisableOnPause == null || scriptsToDisableOnPause.Length == 0)
            return;

        previousScriptStates = new bool[scriptsToDisableOnPause.Length];

        for (int i = 0; i < scriptsToDisableOnPause.Length; i++)
        {
            if (scriptsToDisableOnPause[i] == null)
                continue;

            previousScriptStates[i] = scriptsToDisableOnPause[i].enabled;
            scriptsToDisableOnPause[i].enabled = false;
        }
    }

    private void RestorePlayerControls()
    {
        if (scriptsToDisableOnPause == null || previousScriptStates == null)
            return;

        for (int i = 0; i < scriptsToDisableOnPause.Length; i++)
        {
            if (scriptsToDisableOnPause[i] == null)
                continue;

            if (i < previousScriptStates.Length)
                scriptsToDisableOnPause[i].enabled = previousScriptStates[i];
        }
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;

        RestorePlayerControls();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogWarning("Kein Main Menu Scene Name eingetragen.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void ShowMainPausePage()
    {
        HideAllPages();

        if (mainPauseRoot != null)
            mainPauseRoot.SetActive(true);
    }

    private void ShowSettingsPage()
    {
        HideAllPages();

        if (settingsRoot != null)
            settingsRoot.SetActive(true);
    }

    private void ShowControlsPage()
    {
        HideAllPages();

        if (controlsRoot != null)
            controlsRoot.SetActive(true);
    }

    private void HideAllPages()
    {
        if (mainPauseRoot != null)
            mainPauseRoot.SetActive(false);

        if (settingsRoot != null)
            settingsRoot.SetActive(false);

        if (controlsRoot != null)
            controlsRoot.SetActive(false);
    }

    private void CreateHeadline(string headlineText, Transform parent)
    {
        TMP_Text headline = CreateText(
            headlineText + "Headline",
            headlineText,
            parent,
            submenuTitlePosition,
            new Vector2(900f, 100f),
            56f,
            titleColor,
            FontStyles.Bold
        );

        if (titleFont != null)
            headline.font = titleFont;

        headline.alignment = TextAlignmentOptions.Center;
        ApplyTitleGradient(headline);
        AddTextShadow(headline.gameObject, new Color(0f, 0f, 0f, 0.85f), new Vector2(3f, -3f));
    }

    private void CreateBackIconButton(Transform parent)
    {
        CreateSpriteIconButton(
            "BackButton",
            backButtonSprite,
            parent,
            backIconPosition,
            backIconSize,
            ShowMainPausePage
        );
    }

    private Button CreateSpriteIconButton(
        string objectName,
        Sprite iconSprite,
        Transform parent,
        Vector2 position,
        Vector2 size,
        UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.AddComponent<Image>();
        image.raycastTarget = true;
        image.color = iconSprite == null ? buttonColor : Color.white;

        if (iconSprite != null)
        {
            image.sprite = iconSprite;
            image.preserveAspect = true;
            image.type = Image.Type.Simple;
        }

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        if (action != null)
            button.onClick.AddListener(action);

        return button;
    }

    private Button CreateButton(string label, Transform parent, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = CreateUIObject(label + "_Button", parent);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = buttonSize;
        rect.anchoredPosition = position;

        Image image = buttonObject.AddComponent<Image>();
        image.color = buttonColor;

        if (buttonSprite != null)
        {
            image.sprite = buttonSprite;
            image.color = Color.white;
            image.type = Image.Type.Sliced;
        }

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = buttonSprite != null ? Color.white : buttonColor;
        colors.highlightedColor = buttonHoverColor;
        colors.pressedColor = borderColor;
        colors.selectedColor = buttonHoverColor;
        colors.disabledColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        if (action != null)
            button.onClick.AddListener(action);

        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        TMP_Text text = CreateText(
            label + "_Text",
            label,
            buttonObject.transform,
            Vector2.zero,
            new Vector2(buttonSize.x - 30f, buttonSize.y - 8f),
            25f,
            buttonTextColor,
            FontStyles.Bold
        );

        if (buttonFont != null)
            text.font = buttonFont;

        text.alignment = TextAlignmentOptions.Center;
        ApplyNormalGradient(text);

        return button;
    }

    private TMP_Text CreateText(
        string objectName,
        string content,
        Transform parent,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color,
        FontStyles fontStyle)
    {
        GameObject textObject = CreateUIObject(objectName, parent);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        text.enableWordWrapping = true;
        text.raycastTarget = false;

        if (textFont != null)
            text.font = textFont;

        return text;
    }

    private Slider CreateSlider(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject sliderObject = CreateUIObject("SoundSlider", parent);

        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;

        GameObject background = CreateUIObject("Background", sliderObject.transform);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.06f, 0.03f, 0.015f, 0.85f);

        GameObject fillArea = CreateUIObject("Fill Area", sliderObject.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(8f, 8f);
        fillAreaRect.offsetMax = new Vector2(-8f, -8f);

        GameObject fill = CreateUIObject("Fill", fillArea.transform);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = borderColor;

        GameObject handle = CreateUIObject("Handle", sliderObject.transform);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(26f, 42f);

        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = buttonTextColor;

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;

        return slider;
    }

    private TMP_Dropdown CreateDropdown(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject dropdownObject = CreateUIObject("GraphicsDropdown", parent);

        RectTransform rect = dropdownObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = dropdownObject.AddComponent<Image>();
        image.color = buttonColor;

        TMP_Dropdown dropdown = dropdownObject.AddComponent<TMP_Dropdown>();

        TMP_Text label = CreateText(
            "DropdownLabel",
            graphicsHighText,
            dropdownObject.transform,
            Vector2.zero,
            size,
            24f,
            buttonTextColor,
            FontStyles.Bold
        );

        ApplyNormalGradient(label);
        dropdown.captionText = label;

        TMP_Text itemText = CreateText(
            "DropdownItemText",
            "Option",
            dropdownObject.transform,
            Vector2.zero,
            size,
            22f,
            buttonTextColor,
            FontStyles.Normal
        );

        ApplyNormalGradient(itemText);
        dropdown.itemText = itemText;
        itemText.gameObject.SetActive(false);

        return dropdown;
    }

    private void ApplyTitleGradient(TMP_Text text)
    {
        if (text == null || !useTextGradient)
            return;

        text.enableVertexGradient = true;
        text.colorGradient = new VertexGradient(
            titleTopLeft,
            titleTopRight,
            titleBottomLeft,
            titleBottomRight
        );
    }

    private void ApplyNormalGradient(TMP_Text text)
    {
        if (text == null || !useTextGradient)
            return;

        text.enableVertexGradient = true;
        text.colorGradient = new VertexGradient(
            normalTopLeft,
            normalTopRight,
            normalBottomLeft,
            normalBottomRight
        );
    }

    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        Debug.Log("Sound geändert: " + value);
    }

    private void OnGraphicsChanged(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        Debug.Log("Grafik geändert: " + index);
    }

    private void AddTextShadow(GameObject obj, Color color, Vector2 distance)
    {
        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = color;
        shadow.effectDistance = distance;
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject obj = new GameObject(objectName, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}