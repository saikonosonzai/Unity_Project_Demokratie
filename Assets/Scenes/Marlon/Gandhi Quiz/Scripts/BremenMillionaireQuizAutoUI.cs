using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class BremenAutoQuizQuestion
{
    [TextArea(2, 5)]
    public string question;

    public string answerA;
    public string answerB;
    public string answerC;
    public string answerD;

    [Tooltip("0 = A, 1 = B, 2 = C, 3 = D")]
    [Range(0, 3)]
    public int correctAnswerIndex;
}

public class BremenMillionaireQuizAutoUI : MonoBehaviour
{
    [Header("Quiz")]
    public List<BremenAutoQuizQuestion> questions = new List<BremenAutoQuizQuestion>();

    [Header("Exit")]
    public PuzzleInteractable puzzleInteractable;
    public float closeDelayAfterSolved = 1f;

    [Header("Settings")]
    public bool restartOnWrongAnswer = true;
    public float nextQuestionDelay = 0.8f;

    [Header("Optional Full Box Sprites")]
    public Sprite topBarSprite;
    public Sprite questionBoxSprite;
    public Sprite answerBoxSprite;
    public Sprite answerLetterBoxSprite;
    public Sprite progressPanelSprite;
    public Sprite progressItemSprite;
    public Sprite phoneJokerSprite;
    public Sprite infoBoxSprite;

    [Header("Optional Inner Textures")]
    public Sprite topBarInnerTexture;
    public Sprite questionInnerTexture;
    public Sprite answerInnerTexture;
    public Sprite answerLetterInnerTexture;
    public Sprite progressPanelInnerTexture;
    public Sprite progressItemInnerTexture;
    public Sprite phoneJokerInnerTexture;

    [Header("Sprite / Texture Settings")]
    public bool useSlicedSprites = true;
    public bool useSlicedInnerTextures = false;
    public bool tileInnerTextures = false;
    public bool hideGeneratedBordersWhenSpriteExists = true;
    public bool useInnerFillWhenSpriteExists = false;
    public bool forceCenteredLayout = true;

    [Header("Clean Layout")]
    public float mainContentXOffset = 0f;

    public Vector2 topBarSize = new Vector2(820f, 72f);
    public Vector2 topBarPosition = new Vector2(0f, -18f);

    public Vector2 questionBoxSize = new Vector2(920f, 105f);
    public Vector2 questionBoxPosition = new Vector2(0f, -62f);

    public Vector2 answerButtonSize = new Vector2(390f, 60f);
    public float answerHorizontalGap = 42f;
    public float answerVerticalGap = 18f;
    public float answersStartY = -175f;

    public Vector2 progressPanelSize = new Vector2(240f, 585f);
    public Vector2 progressPanelPosition = new Vector2(-48f, -28f);

    public Vector2 phoneJokerSize = new Vector2(82f, 82f);
    public Vector2 phoneJokerPosition = new Vector2(46f, 80f);

    [Header("Theme - Overlay")]
    public Color rootOverlayColor = new Color(0f, 0f, 0f, 0.05f);

    [Header("Theme - Wood")]
    public Color woodDark = new Color(0.10f, 0.055f, 0.025f, 0.96f);
    public Color woodMid = new Color(0.20f, 0.11f, 0.045f, 0.98f);
    public Color woodLight = new Color(0.34f, 0.20f, 0.085f, 1f);

    [Header("Theme - Gold")]
    public Color gold = new Color(0.95f, 0.66f, 0.12f, 1f);
    public Color darkGold = new Color(0.38f, 0.23f, 0.04f, 1f);

    [Header("Theme - Parchment")]
    public Color parchment = new Color(0.86f, 0.80f, 0.68f, 0.98f);
    public Color parchmentDark = new Color(0.66f, 0.55f, 0.39f, 1f);

    [Header("Theme - Text")]
    public Color lightText = new Color(0.97f, 0.93f, 0.86f, 1f);
    public Color darkText = new Color(0.13f, 0.08f, 0.035f, 1f);

    [Header("Answer States")]
    public Color normalAnswerColor = new Color(0.055f, 0.05f, 0.038f, 0.98f);
    public Color selectedAnswerColor = new Color(0.50f, 0.33f, 0.12f, 1f);
    public Color correctAnswerColor = new Color(0.23f, 0.52f, 0.24f, 1f);
    public Color wrongAnswerColor = new Color(0.58f, 0.18f, 0.16f, 1f);

    [Header("Progress States")]
    public Color progressNormalColor = new Color(0.065f, 0.08f, 0.26f, 0.95f);
    public Color progressCurrentColor = new Color(0.54f, 0.35f, 0.13f, 1f);
    public Color progressDoneColor = new Color(0.26f, 0.50f, 0.23f, 1f);

    [Header("Solved Event")]
    public UnityEvent OnQuizSolved;

    private RectTransform rootRect;

    private TMP_Text questionText;

    private Button[] answerButtons;
    private TMP_Text[] answerTexts;
    private TMP_Text[] answerLetterTexts;
    private Image[] answerImages;
    private Image[] answerInnerImages;
    private Image[] answerLetterImages;

    private Image[] progressImages;
    private TMP_Text[] progressTexts;

    private Button phoneJokerButton;
    private Image phoneJokerImage;

    private int currentQuestionIndex;
    private bool waiting;
    private bool quizSolved;
    private bool uiCreated;
    private bool phoneJokerUsed;

    private void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        if (forceCenteredLayout)
            ApplyCenteredLayout();

        EnsureEventSystem();
        EnsureCanvasSetup();

        if (!uiCreated)
            CreateUI();

        StartQuiz();
    }

    private void ApplyCenteredLayout()
    {
        mainContentXOffset = 0f;
        questionBoxPosition = new Vector2(0f, -62f);
        topBarPosition = new Vector2(0f, -18f);
    }

    public void StartQuiz()
    {
        CancelInvoke();

        currentQuestionIndex = 0;
        waiting = false;
        quizSolved = false;
        phoneJokerUsed = false;

        if (questions == null || questions.Count == 0)
        {
            SetButtonsInteractable(false);
            UpdateProgressUI();
            UpdatePhoneJokerUI();
            return;
        }

        ShowQuestion();
    }

    private void EnsureCanvasSetup()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = GetComponent<CanvasScaler>();

        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();

        if (raycaster == null)
            gameObject.AddComponent<GraphicRaycaster>();

        rootRect = GetComponent<RectTransform>();

        if (rootRect == null)
            rootRect = gameObject.AddComponent<RectTransform>();

        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
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

    private void CreateUI()
    {
        uiCreated = true;

        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        Image rootImage = GetComponent<Image>();

        if (rootImage == null)
            rootImage = gameObject.AddComponent<Image>();

        rootImage.color = rootOverlayColor;
        rootImage.raycastTarget = true;

        CreateTopBar();
        CreateProgressPanel();
        CreatePhoneJokerButton();
        CreateQuestionPanel();
        CreateAnswerButtons();
    }

    private void CreateTopBar()
    {
        Image outer;
        Image inner;

        GameObject topBar = CreateFramedPanel(
            "TopBar",
            transform,
            woodMid,
            woodDark,
            8f,
            true,
            topBarSprite,
            topBarInnerTexture,
            out outer,
            out inner
        );

        RectTransform rect = topBar.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = topBarSize;
        rect.anchoredPosition = topBarPosition;

        AddGoldBorderIfNeeded(topBar.transform, 3f, topBarSprite);

        TMP_Text title = CreateText(
            "Title",
            topBar.transform,
            "QUIZ",
            32f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            gold
        );

        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }

    private void CreateProgressPanel()
    {
        Image outer;
        Image inner;

        GameObject progressPanel = CreateFramedPanel(
            "ProgressPanel",
            transform,
            woodMid,
            woodDark,
            10f,
            true,
            progressPanelSprite,
            progressPanelInnerTexture,
            out outer,
            out inner
        );

        RectTransform rect = progressPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.sizeDelta = progressPanelSize;
        rect.anchoredPosition = progressPanelPosition;

        AddGoldBorderIfNeeded(progressPanel.transform, 3f, progressPanelSprite);

        TMP_Text header = CreateText(
            "ProgressHeader",
            progressPanel.transform,
            "FORTSCHRITT",
            18f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            gold
        );

        RectTransform headerRect = header.rectTransform;
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.sizeDelta = new Vector2(0f, 36f);
        headerRect.anchoredPosition = new Vector2(0f, -10f);

        GameObject listRoot = new GameObject("ProgressList", typeof(RectTransform));
        listRoot.transform.SetParent(progressPanel.transform, false);

        RectTransform listRect = listRoot.GetComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0f, 0f);
        listRect.anchorMax = new Vector2(1f, 1f);
        listRect.offsetMin = new Vector2(16f, 16f);
        listRect.offsetMax = new Vector2(-16f, -52f);

        VerticalLayoutGroup vlg = listRoot.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5f;
        vlg.padding = new RectOffset(0, 0, 0, 0);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        progressImages = new Image[12];
        progressTexts = new TMP_Text[12];

        for (int displayIndex = 12; displayIndex >= 1; displayIndex--)
        {
            int arrayIndex = displayIndex - 1;

            Image itemOuter;
            Image itemInner;

            GameObject item = CreateFramedPanel(
                "ProgressItem_" + displayIndex,
                listRoot.transform,
                woodLight,
                progressNormalColor,
                3f,
                true,
                progressItemSprite,
                progressItemInnerTexture,
                out itemOuter,
                out itemInner
            );

            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0f, 35f);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 35f;

            AddGoldBorderIfNeeded(item.transform, 2f, progressItemSprite);

            TMP_Text txt = CreateText(
                "ProgressText_" + displayIndex,
                item.transform,
                "Frage " + displayIndex,
                16f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                lightText
            );

            RectTransform txtRect = txt.rectTransform;
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            progressImages[arrayIndex] = itemOuter;
            progressTexts[arrayIndex] = txt;
        }
    }

    private void CreatePhoneJokerButton()
    {
        GameObject jokerObject = new GameObject(
            "PhoneJokerButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        jokerObject.transform.SetParent(transform, false);

        RectTransform rect = jokerObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = phoneJokerSize;
        rect.anchoredPosition = phoneJokerPosition;

        phoneJokerImage = jokerObject.GetComponent<Image>();
        phoneJokerImage.sprite = phoneJokerSprite;
        phoneJokerImage.type = Image.Type.Simple;
        phoneJokerImage.preserveAspect = true;
        phoneJokerImage.raycastTarget = true;

        if (phoneJokerSprite != null)
            phoneJokerImage.color = Color.white;
        else
            phoneJokerImage.color = Color.clear;

        phoneJokerButton = jokerObject.GetComponent<Button>();
        phoneJokerButton.transition = Selectable.Transition.None;
        phoneJokerButton.onClick.RemoveAllListeners();
        phoneJokerButton.onClick.AddListener(UsePhoneJoker);
    }

    private void CreateQuestionPanel()
    {
        Image outer;
        Image inner;

        GameObject questionPanel = CreateFramedPanel(
            "QuestionPanel",
            transform,
            woodMid,
            parchment,
            10f,
            true,
            questionBoxSprite,
            questionInnerTexture,
            out outer,
            out inner
        );

        RectTransform rect = questionPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = questionBoxSize;
        rect.anchoredPosition = questionBoxPosition;

        AddGoldBorderIfNeeded(questionPanel.transform, 3f, questionBoxSprite);

        if (questionBoxSprite == null || useInnerFillWhenSpriteExists)
            AddInnerBorder(inner.transform, parchmentDark, 2f);

        questionText = CreateText(
            "QuestionText",
            questionPanel.transform,
            "Frage steht hier",
            23f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            darkText
        );

        RectTransform qRect = questionText.rectTransform;
        qRect.anchorMin = Vector2.zero;
        qRect.anchorMax = Vector2.one;
        qRect.offsetMin = new Vector2(36f, 10f);
        qRect.offsetMax = new Vector2(-36f, -10f);
    }

    private void CreateAnswerButtons()
    {
        answerButtons = new Button[4];
        answerTexts = new TMP_Text[4];
        answerLetterTexts = new TMP_Text[4];
        answerImages = new Image[4];
        answerInnerImages = new Image[4];
        answerLetterImages = new Image[4];

        float totalWidth = answerButtonSize.x * 2f + answerHorizontalGap;
        float leftX = mainContentXOffset - totalWidth * 0.5f + answerButtonSize.x * 0.5f;
        float rightX = mainContentXOffset + totalWidth * 0.5f - answerButtonSize.x * 0.5f;

        float topY = answersStartY;
        float bottomY = answersStartY - answerButtonSize.y - answerVerticalGap;

        CreateAnswerButton(0, "A", new Vector2(leftX, topY));
        CreateAnswerButton(1, "B", new Vector2(rightX, topY));
        CreateAnswerButton(2, "C", new Vector2(leftX, bottomY));
        CreateAnswerButton(3, "D", new Vector2(rightX, bottomY));
    }

    private void CreateAnswerButton(int index, string label, Vector2 anchoredPosition)
    {
        Image outer;
        Image inner;

        GameObject buttonObject = CreateFramedPanel(
            "Answer" + label + "Button",
            transform,
            woodLight,
            woodDark,
            6f,
            true,
            answerBoxSprite,
            answerInnerTexture,
            out outer,
            out inner
        );

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = answerButtonSize;
        rect.anchoredPosition = anchoredPosition;

        AddGoldBorderIfNeeded(buttonObject.transform, 3f, answerBoxSprite);

        Button button = buttonObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;

        int capturedIndex = index;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SelectAnswer(capturedIndex));

        GameObject letterBox = new GameObject("LetterArea", typeof(RectTransform), typeof(Image));
        letterBox.transform.SetParent(buttonObject.transform, false);

        RectTransform lbRect = letterBox.GetComponent<RectTransform>();
        lbRect.anchorMin = new Vector2(0f, 0f);
        lbRect.anchorMax = new Vector2(0f, 1f);
        lbRect.pivot = new Vector2(0f, 0.5f);
        lbRect.sizeDelta = new Vector2(56f, -12f);
        lbRect.anchoredPosition = new Vector2(6f, 0f);

        Image lbImage = letterBox.GetComponent<Image>();
        ApplySpriteOrColor(lbImage, answerLetterInnerTexture, new Color(0.12f, 0.08f, 0.04f, 0.94f), true);
        lbImage.raycastTarget = false;

        TMP_Text letterText = CreateText(
            "LetterText",
            letterBox.transform,
            label,
            22f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            gold
        );

        RectTransform ltRect = letterText.rectTransform;
        ltRect.anchorMin = Vector2.zero;
        ltRect.anchorMax = Vector2.one;
        ltRect.offsetMin = Vector2.zero;
        ltRect.offsetMax = Vector2.zero;

        GameObject separator = new GameObject("Separator", typeof(RectTransform), typeof(Image));
        separator.transform.SetParent(buttonObject.transform, false);

        RectTransform sepRect = separator.GetComponent<RectTransform>();
        sepRect.anchorMin = new Vector2(0f, 0f);
        sepRect.anchorMax = new Vector2(0f, 1f);
        sepRect.pivot = new Vector2(0f, 0.5f);
        sepRect.sizeDelta = new Vector2(2f, -14f);
        sepRect.anchoredPosition = new Vector2(68f, 0f);

        Image sepImage = separator.GetComponent<Image>();
        sepImage.color = gold;
        sepImage.raycastTarget = false;

        TMP_Text text = CreateText(
            "Answer" + label + "Text",
            buttonObject.transform,
            "Antwort",
            17f,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineLeft,
            lightText
        );

        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(84f, 6f);
        textRect.offsetMax = new Vector2(-16f, -6f);

        answerButtons[index] = button;
        answerTexts[index] = text;
        answerLetterTexts[index] = letterText;
        answerImages[index] = outer;
        answerInnerImages[index] = inner;
        answerLetterImages[index] = lbImage;
    }

    private GameObject CreateFramedPanel(
        string objectName,
        Transform parent,
        Color outerColor,
        Color innerColor,
        float inset,
        bool raycastTarget,
        Sprite backgroundSprite,
        Sprite innerTextureSprite,
        out Image outerImage,
        out Image innerImage)
    {
        GameObject root = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        root.transform.SetParent(parent, false);

        outerImage = root.GetComponent<Image>();
        ApplySpriteOrColor(outerImage, backgroundSprite, outerColor, false);
        outerImage.raycastTarget = raycastTarget;

        GameObject inner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
        inner.transform.SetParent(root.transform, false);

        RectTransform innerRect = inner.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.offsetMin = new Vector2(inset, inset);
        innerRect.offsetMax = new Vector2(-inset, -inset);

        innerImage = inner.GetComponent<Image>();
        ApplyInnerTextureOrColor(innerImage, innerTextureSprite, innerColor, backgroundSprite);
        innerImage.raycastTarget = false;

        return root;
    }

    private void ApplyInnerTextureOrColor(Image image, Sprite textureSprite, Color fallbackColor, Sprite outerSprite)
    {
        if (image == null)
            return;

        if (textureSprite != null)
        {
            image.sprite = textureSprite;
            image.color = Color.white;
            image.preserveAspect = false;

            if (tileInnerTextures)
                image.type = Image.Type.Tiled;
            else
                image.type = useSlicedInnerTextures ? Image.Type.Sliced : Image.Type.Simple;

            return;
        }

        if (outerSprite != null && !useInnerFillWhenSpriteExists)
        {
            image.sprite = null;
            image.color = Color.clear;
            image.type = Image.Type.Simple;
        }
        else
        {
            image.sprite = null;
            image.color = fallbackColor;
            image.type = Image.Type.Simple;
        }
    }

    private void ApplySpriteOrColor(Image image, Sprite sprite, Color fallbackColor, bool isInnerTexture)
    {
        if (image == null)
            return;

        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = false;

            if (isInnerTexture && tileInnerTextures)
                image.type = Image.Type.Tiled;
            else if (isInnerTexture)
                image.type = useSlicedInnerTextures ? Image.Type.Sliced : Image.Type.Simple;
            else
                image.type = useSlicedSprites ? Image.Type.Sliced : Image.Type.Simple;
        }
        else
        {
            image.sprite = null;
            image.color = fallbackColor;
            image.type = Image.Type.Simple;
        }
    }

    private TMP_Text CreateText(
        string objectName,
        Transform parent,
        string content,
        float fontSize,
        FontStyles style,
        TextAlignmentOptions alignment,
        Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Ellipsis;

        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);

        return text;
    }

    private void AddGoldBorderIfNeeded(Transform parent, float thickness, Sprite usedSprite)
    {
        if (usedSprite != null && hideGeneratedBordersWhenSpriteExists)
            return;

        AddGoldBorder(parent, thickness);
    }

    private void AddGoldBorder(Transform parent, float thickness)
    {
        CreateBorderBar(parent, "TopBorder", gold, darkGold, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, thickness));
        CreateBorderBar(parent, "BottomBorder", gold, darkGold, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, thickness));
        CreateBorderBar(parent, "LeftBorder", gold, darkGold, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(thickness, 0f));
        CreateBorderBar(parent, "RightBorder", gold, darkGold, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(thickness, 0f));
    }

    private void AddInnerBorder(Transform parent, Color borderColor, float thickness)
    {
        CreateBorderBar(parent, "TopInnerBorder", borderColor, borderColor, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, thickness));
        CreateBorderBar(parent, "BottomInnerBorder", borderColor, borderColor, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, thickness));
        CreateBorderBar(parent, "LeftInnerBorder", borderColor, borderColor, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(thickness, 0f));
        CreateBorderBar(parent, "RightInnerBorder", borderColor, borderColor, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(thickness, 0f));
    }

    private void CreateBorderBar(
        Transform parent,
        string name,
        Color color,
        Color outlineColor,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 sizeDelta)
    {
        GameObject bar = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));
        bar.transform.SetParent(parent, false);

        RectTransform rect = bar.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = Vector2.zero;

        Image image = bar.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        Outline outline = bar.GetComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(1f, -1f);
    }

    private void ShowQuestion()
    {
        waiting = false;

        if (questions == null || questions.Count == 0)
            return;

        if (currentQuestionIndex < 0 || currentQuestionIndex >= questions.Count)
            return;

        BremenAutoQuizQuestion question = questions[currentQuestionIndex];

        if (questionText != null)
            questionText.text = question.question;

        SetAnswerText(0, question.answerA);
        SetAnswerText(1, question.answerB);
        SetAnswerText(2, question.answerC);
        SetAnswerText(3, question.answerD);

        ResetAnswerColors();
        UpdateProgressUI();
        UpdatePhoneJokerUI();
        SetButtonsInteractable(true);
    }

    private void SetAnswerText(int index, string value)
    {
        if (answerTexts == null || index < 0 || index >= answerTexts.Length)
            return;

        if (answerTexts[index] != null)
            answerTexts[index].text = value;
    }

    private void UsePhoneJoker()
    {
        if (phoneJokerUsed || quizSolved || waiting)
            return;

        if (questions == null || currentQuestionIndex < 0 || currentQuestionIndex >= questions.Count)
            return;

        phoneJokerUsed = true;

        BremenAutoQuizQuestion question = questions[currentQuestionIndex];
        int correctIndex = question.correctAnswerIndex;

        SetAnswerColor(correctIndex, selectedAnswerColor);

        UpdatePhoneJokerUI();
    }

    private void UpdatePhoneJokerUI()
    {
        if (phoneJokerButton != null)
            phoneJokerButton.interactable = !phoneJokerUsed && !quizSolved;

        if (phoneJokerImage == null)
            return;

        if (phoneJokerSprite == null)
        {
            phoneJokerImage.color = Color.clear;
            return;
        }

        phoneJokerImage.color = phoneJokerUsed
            ? new Color(0.35f, 0.35f, 0.35f, 0.65f)
            : Color.white;
    }

    private void SelectAnswer(int answerIndex)
    {
        if (waiting || quizSolved)
            return;

        if (questions == null || currentQuestionIndex >= questions.Count)
            return;

        waiting = true;
        SetButtonsInteractable(false);

        BremenAutoQuizQuestion question = questions[currentQuestionIndex];
        bool correct = answerIndex == question.correctAnswerIndex;

        if (correct)
        {
            SetAnswerColor(answerIndex, correctAnswerColor);

            currentQuestionIndex++;

            if (currentQuestionIndex >= questions.Count)
            {
                quizSolved = true;
                UpdateProgressUI();
                UpdatePhoneJokerUI();
                Invoke(nameof(SolveQuiz), closeDelayAfterSolved);
            }
            else
            {
                UpdateProgressUI();
                Invoke(nameof(ShowQuestion), nextQuestionDelay);
            }
        }
        else
        {
            SetAnswerColor(answerIndex, wrongAnswerColor);
            SetAnswerColor(question.correctAnswerIndex, correctAnswerColor);

            if (restartOnWrongAnswer)
            {
                currentQuestionIndex = 0;
                UpdateProgressUI();
                Invoke(nameof(ShowQuestion), nextQuestionDelay + 0.5f);
            }
            else
            {
                UpdateProgressUI();
                Invoke(nameof(ShowQuestion), nextQuestionDelay + 0.5f);
            }
        }
    }

    private void SolveQuiz()
    {
        OnQuizSolved?.Invoke();

        if (puzzleInteractable != null)
        {
            puzzleInteractable.ClosePuzzleAfterSolved();
        }
        else
        {
            gameObject.SetActive(false);
            Debug.LogWarning("PuzzleInteractable nicht eingetragen. Quiz wurde nur deaktiviert.");
        }
    }

    private void UpdateProgressUI()
    {
        if (progressImages == null || progressTexts == null)
            return;

        for (int i = 0; i < progressImages.Length; i++)
        {
            if (progressImages[i] == null || progressTexts[i] == null)
                continue;

            if (i < currentQuestionIndex)
            {
                SetProgressColor(i, progressDoneColor);
                progressTexts[i].color = lightText;
            }
            else if (i == currentQuestionIndex && !quizSolved)
            {
                SetProgressColor(i, progressCurrentColor);
                progressTexts[i].color = lightText;
            }
            else
            {
                SetProgressColor(i, progressNormalColor);
                progressTexts[i].color = lightText;
            }

            progressTexts[i].text = "Frage " + (i + 1);
        }
    }

    private void SetProgressColor(int index, Color color)
    {
        if (progressImages == null || index < 0 || index >= progressImages.Length)
            return;

        if (progressImages[index] == null)
            return;

        progressImages[index].color = color;
    }

    private void ResetAnswerColors()
    {
        if (answerImages == null)
            return;

        for (int i = 0; i < answerImages.Length; i++)
            SetAnswerColor(i, normalAnswerColor);
    }

    private void SetAnswerColor(int index, Color color)
    {
        if (answerImages == null || index < 0 || index >= answerImages.Length)
            return;

        if (answerImages[index] != null)
            answerImages[index].color = color;

        if (answerInnerImages != null && index < answerInnerImages.Length && answerInnerImages[index] != null)
        {
            if (answerInnerTexture != null)
            {
                answerInnerImages[index].color = Color.white;
            }
            else if (answerBoxSprite != null && !useInnerFillWhenSpriteExists)
            {
                answerInnerImages[index].color = Color.clear;
            }
            else
            {
                answerInnerImages[index].color = Color.Lerp(color, Color.black, 0.25f);
            }
        }

        if (answerLetterImages != null && index < answerLetterImages.Length && answerLetterImages[index] != null)
        {
            if (answerLetterInnerTexture != null)
                answerLetterImages[index].color = Color.white;
            else
                answerLetterImages[index].color = Color.Lerp(color, woodDark, 0.35f);
        }

        if (answerTexts != null && index < answerTexts.Length && answerTexts[index] != null)
            answerTexts[index].color = lightText;

        if (answerLetterTexts != null && index < answerLetterTexts.Length && answerLetterTexts[index] != null)
            answerLetterTexts[index].color = gold;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (answerButtons == null)
            return;

        foreach (Button btn in answerButtons)
        {
            if (btn != null)
                btn.interactable = interactable;
        }
    }
}