using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[System.Serializable]
public class BremenMenuLevelEntry
{
    public string levelName;
    public Sprite levelImage;
    public string sceneName;
}

public class BremenStartMenuDesignUI : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string mainGameSceneName = "Main";

    [Header("Main Menu Background Music")]
    [SerializeField] private AudioClip mainMenuBackgroundMusic;
    [SerializeField] private bool playBackgroundMusic = true;
    [SerializeField] private bool loopBackgroundMusic = true;
    [SerializeField] [Range(0f, 1f)] private float backgroundMusicVolume = 0.45f;

    [Header("Background")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Sprite submenuBlurredBackgroundSprite;
    [SerializeField] private Color backgroundColor = new Color(0.06f, 0.025f, 0.015f, 1f);
    [SerializeField] private Color darkOverlayColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Color submenuOverlayColor = new Color(0f, 0f, 0f, 0.55f);

    [Header("Main Title")]
    [SerializeField] private string gameTitle = "Rathaus Escape";
    [SerializeField] private TMP_FontAsset titleFont;
    [SerializeField] private float titleFontSize = 82f;

    [Header("Fonts")]
    [SerializeField] private TMP_FontAsset buttonFont;
    [SerializeField] private TMP_FontAsset textFont;

    [Header("Button Sprites")]
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private Sprite backButtonSprite;
    [SerializeField] private Sprite leftArrowSprite;
    [SerializeField] private Sprite rightArrowSprite;

    [Header("Main Menu Button Texts")]
    [SerializeField] private string mainStoryButtonText = "Hauptstory spielen";
    [SerializeField] private string levelSelectButtonText = "Levelauswahl";
    [SerializeField] private string settingsButtonText = "Einstellungen";
    [SerializeField] private string controlsButtonText = "Steuerung";
    [SerializeField] private string creditsButtonText = "Mitwirkende";
    [SerializeField] private string quitButtonText = "Spiel beenden";

    [Header("General Button Texts")]
    [SerializeField] private string chooseLevelButtonText = "Starten";

    [Header("Page Titles")]
    [SerializeField] private string settingsPageTitle = "Einstellungen";
    [SerializeField] private string controlsPageTitle = "Steuerung";
    [SerializeField] private string levelSelectPageTitle = "Levelauswahl";
    [SerializeField] private string creditsPageTitle = "Mitwirkende";

    [Header("Page Texts")]
    [TextArea(6, 14)]
    [SerializeField] private string controlsPageText =
        "W / A / S / D  -  Bewegen\n" +
        "Maus  -  Umsehen\n" +
        "E  -  Interagieren\n" +
        "Leertaste  -  Springen\n" +
        "Shift  -  Rennen\n" +
        "Esc  -  Menü schließen";

    [TextArea(6, 16)]
    [SerializeField] private string creditsPageText =
        "Story & Konzept\n" +
        "Marlon, Franck, Fyn\n\n" +
        "Rätsel & Spielideen\n" +
        "Story-Team\n\n" +
        "Programmierung\n" +
        "Fyn\n\n" +
        "Dokumentation\n" +
        "Marlon\n\n" +
        "Präsentation & Video\n" +
        "Franck";

    [Header("Settings Texts")]
    [SerializeField] private string soundLabelText = "Sound";
    [SerializeField] private string graphicsLabelText = "Grafik";

    [Header("Graphics Options")]
    [SerializeField] private string graphicsLowText = "Niedrig";
    [SerializeField] private string graphicsMediumText = "Mittel";
    [SerializeField] private string graphicsHighText = "Hoch";
    [SerializeField] private string graphicsUltraText = "Ultra";

    [Header("Levelauswahl")]
    [SerializeField] private BremenMenuLevelEntry[] levels;
    [SerializeField] private string noLevelsText = "Noch keine Räume eingetragen.";
    [SerializeField] private string defaultRoomText = "Raum";

    [Header("Levelauswahl Layout")]
    [SerializeField] private Vector2 levelImagePosition = new Vector2(0f, 60f);
    [SerializeField] private Vector2 levelImageSize = new Vector2(520f, 270f);
    [SerializeField] private Vector2 levelTitlePosition = new Vector2(0f, -120f);
    [SerializeField] private Vector2 levelTitleSize = new Vector2(700f, 70f);
    [SerializeField] private float levelTitleFontSize = 34f;

    [SerializeField] private Vector2 leftArrowPosition = new Vector2(-360f, 55f);
    [SerializeField] private Vector2 rightArrowPosition = new Vector2(360f, 55f);
    [SerializeField] private Vector2 arrowButtonSize = new Vector2(80f, 80f);
    [SerializeField] private Vector2 chooseLevelButtonPosition = new Vector2(0f, -210f);

    [Header("Level Image Style")]
    [SerializeField] private Color levelImageFallbackColor = new Color(0.08f, 0.04f, 0.02f, 0.75f);
    [SerializeField] private bool preserveLevelImageAspect = true;

    [Header("Colors")]
    [SerializeField] private Color titleColor = new Color(0.82f, 0.68f, 0.36f, 1f);
    [SerializeField] private Color buttonColor = new Color(0.08f, 0.04f, 0.02f, 0.70f);
    [SerializeField] private Color buttonHoverColor = new Color(0.32f, 0.16f, 0.06f, 0.88f);
    [SerializeField] private Color buttonTextColor = new Color(0.88f, 0.76f, 0.50f, 1f);
    [SerializeField] private Color borderColor = new Color(0.65f, 0.48f, 0.20f, 1f);
    [SerializeField] private Color normalTextColor = new Color(0.88f, 0.78f, 0.50f, 1f);

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

    [Header("Headline Fake Outline")]
    [SerializeField] private bool useHeadlineOutline = true;
    [SerializeField] private Color headlineOutlineColor = new Color(1f, 0.86f, 0.28f, 0.85f);
    [SerializeField] private float headlineOutlineThickness = 2.5f;

    [Header("Main Layout")]
    [SerializeField] private Vector2 titlePosition = new Vector2(0f, 265f);
    [SerializeField] private Vector2 buttonStartPosition = new Vector2(0f, 120f);
    [SerializeField] private float buttonSpacing = 68f;
    [SerializeField] private Vector2 buttonSize = new Vector2(430f, 58f);

    [Header("Submenu Layout")]
    [SerializeField] private Vector2 submenuTitlePosition = new Vector2(0f, 270f);
    [SerializeField] private Vector2 submenuContentPosition = new Vector2(0f, 35f);

    [Header("Back Button")]
    [SerializeField] private Vector2 backIconPosition = new Vector2(-850f, 455f);
    [SerializeField] private Vector2 backIconSize = new Vector2(72f, 72f);

    [Header("Submenu Text")]
    [SerializeField] private float controlsTextFontSize = 30f;
    [SerializeField] private float creditsTextFontSize = 30f;

    [Header("Submenu Auto Text Layout")]
    [SerializeField] private Vector2 controlsTextTopPosition = new Vector2(0f, 140f);
    [SerializeField] private Vector2 creditsTextTopPosition = new Vector2(0f, 170f);

    [SerializeField] private float controlsLineSpacing = 42f;
    [SerializeField] private float creditsLineSpacing = 42f;

    [SerializeField] private float submenuTextLineWidth = 900f;
    [SerializeField] private float submenuTextLineHeight = 42f;

    private GameObject mainMenuRoot;
    private GameObject settingsRoot;
    private GameObject controlsRoot;
    private GameObject levelSelectRoot;
    private GameObject creditsRoot;

    private Image backgroundImage;
    private Image overlayImage;

    private Slider volumeSlider;
    private TMP_Dropdown graphicsDropdown;

    private Image levelPreviewImage;
    private TMP_Text levelPreviewTitle;

    private AudioSource backgroundMusicSource;

    private int currentLevelIndex;

    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        EnsureEventSystem();
        SetupBackgroundMusic();

        BuildCanvas();
        BuildBackground();

        BuildMainMenu();
        BuildSettingsMenu();
        BuildControlsMenu();
        BuildLevelSelectMenu();
        BuildCreditsMenu();

        ShowMainMenu();
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

    private void SetupBackgroundMusic()
    {
        if (!playBackgroundMusic)
            return;

        if (mainMenuBackgroundMusic == null)
        {
            Debug.LogWarning("Keine Hintergrundmusik eingetragen.");
            return;
        }

        backgroundMusicSource = GetComponent<AudioSource>();

        if (backgroundMusicSource == null)
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();

        backgroundMusicSource.clip = mainMenuBackgroundMusic;
        backgroundMusicSource.loop = loopBackgroundMusic;
        backgroundMusicSource.playOnAwake = false;
        backgroundMusicSource.volume = backgroundMusicVolume;
        backgroundMusicSource.spatialBlend = 0f;

        backgroundMusicSource.Play();
    }

    private void BuildCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

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

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void BuildBackground()
    {
        GameObject backgroundObject = CreateUIObject("Background", transform);

        RectTransform rect = backgroundObject.GetComponent<RectTransform>();
        StretchFull(rect);

        backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.raycastTarget = false;

        if (backgroundSprite != null)
        {
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = Color.white;
            backgroundImage.preserveAspect = false;
        }
        else
        {
            backgroundImage.color = backgroundColor;
        }

        GameObject overlayObject = CreateUIObject("DarkOverlay", transform);

        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        StretchFull(overlayRect);

        overlayImage = overlayObject.AddComponent<Image>();
        overlayImage.color = darkOverlayColor;
        overlayImage.raycastTarget = false;
    }

    private void SetNormalBackground()
    {
        if (backgroundImage != null)
        {
            if (backgroundSprite != null)
            {
                backgroundImage.sprite = backgroundSprite;
                backgroundImage.color = Color.white;
                backgroundImage.preserveAspect = false;
            }
            else
            {
                backgroundImage.sprite = null;
                backgroundImage.color = backgroundColor;
            }
        }

        if (overlayImage != null)
            overlayImage.color = darkOverlayColor;
    }

    private void SetSubmenuBackground()
    {
        if (backgroundImage != null)
        {
            if (submenuBlurredBackgroundSprite != null)
            {
                backgroundImage.sprite = submenuBlurredBackgroundSprite;
                backgroundImage.color = Color.white;
                backgroundImage.preserveAspect = false;
            }
            else if (backgroundSprite != null)
            {
                backgroundImage.sprite = backgroundSprite;
                backgroundImage.color = Color.white;
                backgroundImage.preserveAspect = false;
            }
            else
            {
                backgroundImage.sprite = null;
                backgroundImage.color = backgroundColor;
            }
        }

        if (overlayImage != null)
            overlayImage.color = submenuOverlayColor;
    }

    private void BuildMainMenu()
    {
        mainMenuRoot = CreateUIObject("MainMenuRoot", transform);
        StretchFull(mainMenuRoot.GetComponent<RectTransform>());

        TMP_Text title = CreateText(
            "GameTitle",
            gameTitle,
            mainMenuRoot.transform,
            titlePosition,
            new Vector2(900f, 120f),
            titleFontSize,
            titleColor,
            FontStyles.Bold
        );

        if (titleFont != null)
            title.font = titleFont;

        title.alignment = TextAlignmentOptions.Center;

        AddFakeHeadlineOutline(title);
        ApplyTitleGradient(title);
        AddTextShadow(title.gameObject, new Color(0f, 0f, 0f, 0.78f), new Vector2(3f, -3f));

        CreateButton(mainStoryButtonText, mainMenuRoot.transform, buttonStartPosition + new Vector2(0f, 0f), StartMainGame);
        CreateButton(levelSelectButtonText, mainMenuRoot.transform, buttonStartPosition + new Vector2(0f, -buttonSpacing), ShowLevelSelect);
        CreateButton(settingsButtonText, mainMenuRoot.transform, buttonStartPosition + new Vector2(0f, -buttonSpacing * 2f), ShowSettings);
        CreateButton(controlsButtonText, mainMenuRoot.transform, buttonStartPosition + new Vector2(0f, -buttonSpacing * 3f), ShowControls);
        CreateButton(creditsButtonText, mainMenuRoot.transform, buttonStartPosition + new Vector2(0f, -buttonSpacing * 4f), ShowCredits);
        CreateButton(quitButtonText, mainMenuRoot.transform, buttonStartPosition + new Vector2(0f, -buttonSpacing * 5f), OnQuitClicked);
    }

    private void BuildSettingsMenu()
    {
        settingsRoot = CreateUIObject("SettingsRoot", transform);
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
        volumeSlider.value = backgroundMusicVolume;
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
        graphicsDropdown.value = 2;
        graphicsDropdown.RefreshShownValue();
        graphicsDropdown.onValueChanged.AddListener(OnGraphicsChanged);
    }

    private void BuildControlsMenu()
    {
        controlsRoot = CreateUIObject("ControlsRoot", transform);
        StretchFull(controlsRoot.GetComponent<RectTransform>());

        CreateBackIconButton(controlsRoot.transform);
        CreateHeadline(controlsPageTitle, controlsRoot.transform);

        CreateTextLinesFromTop(
            "ControlsLine",
            controlsPageText,
            controlsRoot.transform,
            controlsTextTopPosition,
            controlsTextFontSize,
            controlsLineSpacing,
            FontStyles.Normal
        );
    }

    private void BuildLevelSelectMenu()
    {
        levelSelectRoot = CreateUIObject("LevelSelectRoot", transform);
        StretchFull(levelSelectRoot.GetComponent<RectTransform>());

        CreateBackIconButton(levelSelectRoot.transform);
        CreateHeadline(levelSelectPageTitle, levelSelectRoot.transform);

        if (levels == null || levels.Length == 0)
        {
            TMP_Text noLevelText = CreateText(
                "NoLevelsText",
                noLevelsText,
                levelSelectRoot.transform,
                new Vector2(0f, 50f),
                new Vector2(700f, 90f),
                32f,
                normalTextColor,
                FontStyles.Normal
            );

            noLevelText.alignment = TextAlignmentOptions.Center;
            ApplyNormalGradient(noLevelText);
        }
        else
        {
            CreateLevelPreview();

            CreateSpriteIconButton(
                "LeftLevelArrow",
                leftArrowSprite,
                levelSelectRoot.transform,
                leftArrowPosition,
                arrowButtonSize,
                ShowPreviousLevel
            );

            CreateSpriteIconButton(
                "RightLevelArrow",
                rightArrowSprite,
                levelSelectRoot.transform,
                rightArrowPosition,
                arrowButtonSize,
                ShowNextLevel
            );

            CreateButton(
                chooseLevelButtonText,
                levelSelectRoot.transform,
                chooseLevelButtonPosition,
                OnCurrentLevelClicked
            );

            currentLevelIndex = 0;
            UpdateLevelPreview();
        }
    }

    private void CreateLevelPreview()
    {
        GameObject imageObject = CreateUIObject("LevelPreviewImage", levelSelectRoot.transform);

        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = levelImagePosition;
        imageRect.sizeDelta = levelImageSize;

        levelPreviewImage = imageObject.AddComponent<Image>();
        levelPreviewImage.color = levelImageFallbackColor;
        levelPreviewImage.preserveAspect = preserveLevelImageAspect;
        levelPreviewImage.raycastTarget = false;

        levelPreviewTitle = CreateText(
            "LevelPreviewTitle",
            "",
            levelSelectRoot.transform,
            levelTitlePosition,
            levelTitleSize,
            levelTitleFontSize,
            normalTextColor,
            FontStyles.Bold
        );

        levelPreviewTitle.alignment = TextAlignmentOptions.Center;
        ApplyNormalGradient(levelPreviewTitle);
    }

    private void BuildCreditsMenu()
    {
        creditsRoot = CreateUIObject("CreditsRoot", transform);
        StretchFull(creditsRoot.GetComponent<RectTransform>());

        CreateBackIconButton(creditsRoot.transform);
        CreateHeadline(creditsPageTitle, creditsRoot.transform);

        CreateTextLinesFromTop(
            "CreditsLine",
            creditsPageText,
            creditsRoot.transform,
            creditsTextTopPosition,
            creditsTextFontSize,
            creditsLineSpacing,
            FontStyles.Normal
        );
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

        AddFakeHeadlineOutline(headline);
        ApplyTitleGradient(headline);
        AddTextShadow(headline.gameObject, new Color(0f, 0f, 0f, 0.78f), new Vector2(3f, -3f));
    }

    private void AddFakeHeadlineOutline(TMP_Text sourceText)
    {
        if (!useHeadlineOutline || sourceText == null)
            return;

        Vector2[] offsets =
        {
            new Vector2(headlineOutlineThickness, 0f),
            new Vector2(-headlineOutlineThickness, 0f),
            new Vector2(0f, headlineOutlineThickness),
            new Vector2(0f, -headlineOutlineThickness),

            new Vector2(headlineOutlineThickness, headlineOutlineThickness),
            new Vector2(-headlineOutlineThickness, headlineOutlineThickness),
            new Vector2(headlineOutlineThickness, -headlineOutlineThickness),
            new Vector2(-headlineOutlineThickness, -headlineOutlineThickness)
        };

        RectTransform sourceRect = sourceText.GetComponent<RectTransform>();
        int sourceIndex = sourceText.transform.GetSiblingIndex();

        foreach (Vector2 offset in offsets)
        {
            GameObject outlineObject = CreateUIObject(sourceText.gameObject.name + "_Outline", sourceText.transform.parent);

            RectTransform outlineRect = outlineObject.GetComponent<RectTransform>();
            outlineRect.anchorMin = sourceRect.anchorMin;
            outlineRect.anchorMax = sourceRect.anchorMax;
            outlineRect.pivot = sourceRect.pivot;
            outlineRect.sizeDelta = sourceRect.sizeDelta;
            outlineRect.anchoredPosition = sourceRect.anchoredPosition + offset;

            TMP_Text outlineText = outlineObject.AddComponent<TextMeshProUGUI>();
            outlineText.text = sourceText.text;
            outlineText.font = sourceText.font;
            outlineText.fontSize = sourceText.fontSize;
            outlineText.fontStyle = sourceText.fontStyle;
            outlineText.alignment = sourceText.alignment;
            outlineText.verticalAlignment = sourceText.verticalAlignment;
            outlineText.enableWordWrapping = sourceText.enableWordWrapping;
            outlineText.color = headlineOutlineColor;
            outlineText.raycastTarget = false;
            outlineText.enableVertexGradient = false;

            if (titleFont != null)
                outlineText.font = titleFont;

            outlineObject.transform.SetSiblingIndex(sourceIndex);
        }

        sourceText.transform.SetAsLastSibling();
    }

    private void UpdateLevelPreview()
    {
        if (levels == null || levels.Length == 0)
            return;

        if (currentLevelIndex < 0)
            currentLevelIndex = levels.Length - 1;

        if (currentLevelIndex >= levels.Length)
            currentLevelIndex = 0;

        BremenMenuLevelEntry currentLevel = levels[currentLevelIndex];

        string title = string.IsNullOrWhiteSpace(currentLevel.levelName)
            ? defaultRoomText + " " + (currentLevelIndex + 1)
            : currentLevel.levelName;

        if (levelPreviewTitle != null)
        {
            levelPreviewTitle.text = title;
            ApplyNormalGradient(levelPreviewTitle);
        }

        if (levelPreviewImage != null)
        {
            if (currentLevel.levelImage != null)
            {
                levelPreviewImage.sprite = currentLevel.levelImage;
                levelPreviewImage.color = Color.white;
                levelPreviewImage.preserveAspect = preserveLevelImageAspect;
            }
            else
            {
                levelPreviewImage.sprite = null;
                levelPreviewImage.color = levelImageFallbackColor;
            }
        }
    }

    private void ShowPreviousLevel()
    {
        if (levels == null || levels.Length == 0)
            return;

        currentLevelIndex--;

        if (currentLevelIndex < 0)
            currentLevelIndex = levels.Length - 1;

        UpdateLevelPreview();
    }

    private void ShowNextLevel()
    {
        if (levels == null || levels.Length == 0)
            return;

        currentLevelIndex++;

        if (currentLevelIndex >= levels.Length)
            currentLevelIndex = 0;

        UpdateLevelPreview();
    }

    private void OnCurrentLevelClicked()
    {
        OnLevelClicked(currentLevelIndex);
    }

    private void CreateBackIconButton(Transform parent)
    {
        CreateSpriteIconButton(
            "BackButton",
            backButtonSprite,
            parent,
            backIconPosition,
            backIconSize,
            ShowMainMenu
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

    private void CreateTextLinesFromTop(
        string objectNamePrefix,
        string fullText,
        Transform parent,
        Vector2 topPosition,
        float fontSize,
        float lineSpacing,
        FontStyles fontStyle)
    {
        if (string.IsNullOrWhiteSpace(fullText))
            return;

        string[] lines = fullText.Split('\n');

        float currentY = topPosition.y;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                currentY -= lineSpacing;
                continue;
            }

            TMP_Text textLine = CreateText(
                objectNamePrefix + "_" + i,
                line,
                parent,
                new Vector2(topPosition.x, currentY),
                new Vector2(submenuTextLineWidth, submenuTextLineHeight),
                fontSize,
                normalTextColor,
                fontStyle
            );

            textLine.alignment = TextAlignmentOptions.Center;
            textLine.verticalAlignment = VerticalAlignmentOptions.Middle;
            textLine.enableWordWrapping = false;

            ApplyNormalGradient(textLine);

            currentY -= lineSpacing;
        }
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

    private void StartMainGame()
    {
        LoadSceneByName(mainGameSceneName);
    }

    private void OnLevelClicked(int index)
    {
        if (levels == null || index < 0 || index >= levels.Length)
            return;

        string sceneName = levels[index].sceneName;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Für dieses Level wurde kein Scene Name eingetragen: " + levels[index].levelName);
            return;
        }

        LoadSceneByName(sceneName);
    }

    private void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Scene Name fehlt.");
            return;
        }

        SceneManager.LoadScene(sceneName);
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

    private void HideAllMenus()
    {
        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(false);

        if (settingsRoot != null)
            settingsRoot.SetActive(false);

        if (controlsRoot != null)
            controlsRoot.SetActive(false);

        if (levelSelectRoot != null)
            levelSelectRoot.SetActive(false);

        if (creditsRoot != null)
            creditsRoot.SetActive(false);
    }

    private void ShowMainMenu()
    {
        SetNormalBackground();
        HideAllMenus();

        if (mainMenuRoot != null)
            mainMenuRoot.SetActive(true);
    }

    private void ShowSettings()
    {
        SetSubmenuBackground();
        HideAllMenus();

        if (settingsRoot != null)
            settingsRoot.SetActive(true);
    }

    private void ShowControls()
    {
        SetSubmenuBackground();
        HideAllMenus();

        if (controlsRoot != null)
            controlsRoot.SetActive(true);
    }

    private void ShowLevelSelect()
    {
        SetSubmenuBackground();
        HideAllMenus();

        if (levelSelectRoot != null)
            levelSelectRoot.SetActive(true);
    }

    private void ShowCredits()
    {
        SetSubmenuBackground();
        HideAllMenus();

        if (creditsRoot != null)
            creditsRoot.SetActive(true);
    }

    private void OnQuitClicked()
    {
        Debug.Log("Spiel beenden wurde geklickt.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnVolumeChanged(float value)
    {
        backgroundMusicVolume = value;
        AudioListener.volume = value;

        if (backgroundMusicSource != null)
            backgroundMusicSource.volume = value;

        Debug.Log("Sound geändert: " + value);
    }

    private void OnGraphicsChanged(int index)
    {
        Debug.Log("Grafik geändert: " + index);
    }
}