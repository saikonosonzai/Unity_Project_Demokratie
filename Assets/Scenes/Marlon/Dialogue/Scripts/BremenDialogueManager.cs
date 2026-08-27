using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BremenDialogueManager : MonoBehaviour
{
    public static BremenDialogueManager Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private KeyCode nextKey = KeyCode.E;
    [SerializeField] private KeyCode altNextKey = KeyCode.Space;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;
    [SerializeField] private bool allowMouseClick = true;

    [Header("Typewriter Effect")]
    [SerializeField] private bool useTypewriterEffect = true;
    [SerializeField] private float typewriterSpeed = 0.035f;

    [Header("Scripts während Dialog deaktivieren")]
    [SerializeField] private MonoBehaviour[] scriptsToDisableDuringDialogue;

    [Header("Cursor")]
    [SerializeField] private bool showCursorDuringDialogue = true;
    [SerializeField] private bool lockCursorAfterDialogue = true;

    [Header("Canvas")]
    [SerializeField] private Canvas dialogueCanvas;

    [Header("Optional Sprites")]
    [SerializeField] private Sprite mainDialogueBoxSprite;
    [SerializeField] private Sprite nameBoxSprite;
    [SerializeField] private Sprite nextArrowSprite;

    [Header("Main Dialogue Box Sprite Settings")]
    [SerializeField] private bool useMainDialogueBoxSprite = true;
    [SerializeField] private bool useSlicedDialogueBoxSprite = true;
    [SerializeField] private Color mainDialogueBoxSpriteTint = Color.white;

    [Header("Optional Fonts")]
    [SerializeField] private TMP_FontAsset dialogueFont;
    [SerializeField] private TMP_FontAsset nameFont;

    [Header("Colors")]
    [SerializeField] private Color rootOverlayColor = new Color(0f, 0f, 0f, 0.08f);
    [SerializeField] private Color dialogueBoxColor = new Color(0.88f, 0.84f, 0.76f, 0.98f);
    [SerializeField] private Color nameBoxColor = new Color(0.12f, 0.08f, 0.04f, 0.96f);
    [SerializeField] private Color borderColor = new Color(0.92f, 0.66f, 0.16f, 1f);
    [SerializeField] private Color borderShadowColor = new Color(0.20f, 0.12f, 0.02f, 1f);

    [Header("Text Gradient wie Startscreen")]
    [SerializeField] private bool useTextGradient = true;

    [Header("Sprechername Gradient")]
    [SerializeField] private Color titleTopLeft = new Color(0.95f, 0.82f, 0.42f, 1f);
    [SerializeField] private Color titleTopRight = new Color(0.82f, 0.68f, 0.34f, 1f);
    [SerializeField] private Color titleBottomLeft = new Color(0.42f, 0.30f, 0.10f, 1f);
    [SerializeField] private Color titleBottomRight = new Color(0.70f, 0.54f, 0.22f, 1f);

    [Header("Dialogtext Gradient")]
    [SerializeField] private Color normalTopLeft = new Color(0.90f, 0.80f, 0.52f, 1f);
    [SerializeField] private Color normalTopRight = new Color(0.78f, 0.66f, 0.38f, 1f);
    [SerializeField] private Color normalBottomLeft = new Color(0.50f, 0.38f, 0.16f, 1f);
    [SerializeField] private Color normalBottomRight = new Color(0.68f, 0.54f, 0.28f, 1f);

    [Header("Dialogue Box Border Gradient")]
    [SerializeField] private bool useGradientBorder = true;
    [SerializeField] private float dialogueBorderThickness = 4f;

    [Header("Layout")]
    [SerializeField] private bool hideNameBoxCompletely = true;

    [SerializeField] private Vector2 dialogueBoxSize = new Vector2(1150f, 300f);
    [SerializeField] private Vector2 dialogueBoxPosition = new Vector2(0f, 70f);

    [SerializeField] private Vector2 portraitSize = new Vector2(250f, 330f);
    [SerializeField] private Vector2 portraitPosition = new Vector2(-190f, 0f);

    [SerializeField] private Vector2 nameBoxSize = new Vector2(260f, 62f);
    [SerializeField] private Vector2 nameBoxPosition = new Vector2(35f, -18f);

    [SerializeField] private Vector2 nextArrowSize = new Vector2(34f, 34f);
    [SerializeField] private Vector2 nextArrowPosition = new Vector2(-18f, 16f);

    [Header("Text")]
    [SerializeField] private float dialogueFontSize = 30f;
    [SerializeField] private float nameFontSize = 28f;

    [Header("Name Inside Textbox")]
    [SerializeField] private bool showSpeakerNameAboveText = true;

    [Header("Manual Text Position In Dialogue Box")]
    [SerializeField] private Vector2 speakerNamePosition = new Vector2(0f, 215f);
    [SerializeField] private Vector2 speakerNameSize = new Vector2(1030f, 45f);

    [SerializeField] private Vector2 dialogueTextPosition = new Vector2(0f, 135f);
    [SerializeField] private Vector2 dialogueTextSize = new Vector2(1030f, 150f);

    private GameObject rootObject;

    private Image rootImage;
    private Image portraitImage;
    private Image dialogueBoxImage;
    private Image nameBoxImage;
    private Image arrowImage;

    private TMP_Text nameText;
    private TMP_Text dialogueText;
    private TMP_Text inlineNameText;

    private BremenDialogueData currentDialogue;
    private int currentLineIndex;
    private bool dialogueActive;

    private Coroutine typewriterCoroutine;
    private bool isTyping;
    private string fullCurrentText = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildUI();
        HideDialogue();
    }

    private void Update()
    {
        if (!dialogueActive)
            return;

        bool nextPressed =
            Input.GetKeyDown(nextKey) ||
            Input.GetKeyDown(altNextKey) ||
            (allowMouseClick && Input.GetMouseButtonDown(0));

        if (nextPressed)
        {
            if (isTyping)
                CompleteCurrentText();
            else
                ShowNextLine();
        }

        if (Input.GetKeyDown(closeKey))
            EndDialogue();
    }

    public void StartDialogue(BremenDialogueData dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("DialogueData fehlt.");
            return;
        }

        if (dialogue.lines == null || dialogue.lines.Length == 0)
        {
            Debug.LogWarning("Dialogue hat keine Lines.");
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        dialogueActive = true;

        ShowDialogue();
        SetPlayerScriptsActive(false);
        SetCursorState(true);
        RenderCurrentLine();
    }

    public void ShowNextLine()
    {
        if (!dialogueActive)
            return;

        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        RenderCurrentLine();
    }

    public void EndDialogue()
    {
        dialogueActive = false;
        currentDialogue = null;
        currentLineIndex = 0;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;
        fullCurrentText = "";

        HideDialogue();
        SetPlayerScriptsActive(true);
        SetCursorState(false);
    }

    private void RenderCurrentLine()
    {
        if (currentDialogue == null)
            return;

        if (currentLineIndex < 0 || currentLineIndex >= currentDialogue.lines.Length)
            return;

        BremenDialogueLine line = currentDialogue.lines[currentLineIndex];

        bool hasName = !string.IsNullOrWhiteSpace(line.speakerName);
        bool showInlineName = hasName && showSpeakerNameAboveText;
        bool showSeparateNameBox = hasName && !hideNameBoxCompletely && !showSpeakerNameAboveText;

        if (nameBoxImage != null)
            nameBoxImage.gameObject.SetActive(showSeparateNameBox);

        if (nameText != null)
        {
            nameText.gameObject.SetActive(showSeparateNameBox);
            nameText.text = showSeparateNameBox ? line.speakerName : "";
            nameText.color = Color.white;
            ApplyTitleGradient(nameText);

            if (nameFont != null)
                nameText.font = nameFont;
        }

        if (inlineNameText != null)
        {
            inlineNameText.gameObject.SetActive(showInlineName);
            inlineNameText.text = showInlineName ? line.speakerName : "";
            inlineNameText.fontStyle = FontStyles.Bold;
            inlineNameText.color = Color.white;
            ApplyTitleGradient(inlineNameText);

            if (nameFont != null)
                inlineNameText.font = nameFont;
        }

        bool hasPortrait = line.portrait != null;

        if (portraitImage != null)
        {
            portraitImage.gameObject.SetActive(hasPortrait);
            portraitImage.sprite = line.portrait;
            portraitImage.preserveAspect = true;
            portraitImage.color = Color.white;

            ApplyPortraitLayout(line);
        }

        if (arrowImage != null)
            arrowImage.gameObject.SetActive(false);

        fullCurrentText = line.text;

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        if (useTypewriterEffect)
        {
            typewriterCoroutine = StartCoroutine(TypeText(fullCurrentText));
        }
        else
        {
            if (dialogueText != null)
            {
                dialogueText.color = Color.white;
                ApplyNormalGradient(dialogueText);
                dialogueText.text = fullCurrentText;
            }

            isTyping = false;

            if (arrowImage != null)
                arrowImage.gameObject.SetActive(true);
        }
    }

    private void ApplyPortraitLayout(BremenDialogueLine line)
    {
        if (portraitImage == null)
            return;

        RectTransform rect = portraitImage.rectTransform;

        if (line != null && line.overridePortraitLayout)
        {
            rect.sizeDelta = line.customPortraitSize;
            rect.anchoredPosition = line.customPortraitPosition;
        }
        else
        {
            rect.sizeDelta = portraitSize;
            rect.anchoredPosition = portraitPosition;
        }
    }

    private IEnumerator TypeText(string textToType)
    {
        isTyping = true;

        if (dialogueText != null)
        {
            dialogueText.color = Color.white;
            ApplyNormalGradient(dialogueText);
            dialogueText.text = "";
        }

        for (int i = 0; i < textToType.Length; i++)
        {
            if (dialogueText != null)
                dialogueText.text += textToType[i];

            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }

        isTyping = false;
        typewriterCoroutine = null;

        if (arrowImage != null)
            arrowImage.gameObject.SetActive(true);
    }

    private void CompleteCurrentText()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.color = Color.white;
            ApplyNormalGradient(dialogueText);
            dialogueText.text = fullCurrentText;
        }

        isTyping = false;

        if (arrowImage != null)
            arrowImage.gameObject.SetActive(true);
    }

    private void ShowDialogue()
    {
        if (rootObject != null)
            rootObject.SetActive(true);
    }

    private void HideDialogue()
    {
        if (rootObject != null)
            rootObject.SetActive(false);
    }

    private void SetPlayerScriptsActive(bool active)
    {
        if (scriptsToDisableDuringDialogue == null)
            return;

        foreach (MonoBehaviour script in scriptsToDisableDuringDialogue)
        {
            if (script == null)
                continue;

            script.enabled = active;
        }
    }

    private void SetCursorState(bool dialogueIsActive)
    {
        if (dialogueIsActive)
        {
            if (showCursorDuringDialogue)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else
        {
            Cursor.visible = !lockCursorAfterDialogue;
            Cursor.lockState = lockCursorAfterDialogue ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    private void BuildUI()
    {
        if (dialogueCanvas == null)
        {
            GameObject canvasObject = new GameObject(
                "DialogueCanvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

            canvasObject.transform.SetParent(transform, false);

            dialogueCanvas = canvasObject.GetComponent<Canvas>();
            dialogueCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            dialogueCanvas.sortingOrder = 500;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        rootObject = new GameObject("DialogueRoot", typeof(RectTransform), typeof(Image));
        rootObject.transform.SetParent(dialogueCanvas.transform, false);

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        rootImage = rootObject.GetComponent<Image>();
        rootImage.color = rootOverlayColor;
        rootImage.raycastTarget = false;

        CreateDialogueBox(rootObject.transform);
        CreatePortrait();
        CreateNameBox();
        CreateNextArrow();
    }

    private void CreateDialogueBox(Transform parent)
    {
        GameObject boxObject = new GameObject("DialogueBox", typeof(RectTransform), typeof(Image));
        boxObject.transform.SetParent(parent, false);

        RectTransform rect = boxObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = dialogueBoxSize;
        rect.anchoredPosition = dialogueBoxPosition;

        dialogueBoxImage = boxObject.GetComponent<Image>();
        ApplyMainDialogueBoxBackground(dialogueBoxImage);
        dialogueBoxImage.raycastTarget = false;

        AddBorder(boxObject.transform, dialogueBorderThickness);

        GameObject inlineNameObject = new GameObject("InlineNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
        inlineNameObject.transform.SetParent(boxObject.transform, false);

        RectTransform inlineNameRect = inlineNameObject.GetComponent<RectTransform>();
        inlineNameRect.anchorMin = new Vector2(0.5f, 0f);
        inlineNameRect.anchorMax = new Vector2(0.5f, 0f);
        inlineNameRect.pivot = new Vector2(0.5f, 0.5f);
        inlineNameRect.anchoredPosition = speakerNamePosition;
        inlineNameRect.sizeDelta = speakerNameSize;

        inlineNameText = inlineNameObject.GetComponent<TMP_Text>();
        inlineNameText.text = "";
        inlineNameText.fontSize = nameFontSize;
        inlineNameText.fontStyle = FontStyles.Bold;
        inlineNameText.color = Color.white;
        inlineNameText.alignment = TextAlignmentOptions.TopLeft;
        inlineNameText.raycastTarget = false;
        inlineNameText.enableWordWrapping = false;
        inlineNameText.richText = false;
        ApplyTitleGradient(inlineNameText);

        if (nameFont != null)
            inlineNameText.font = nameFont;

        GameObject textObject = new GameObject("DialogueText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(boxObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0f);
        textRect.anchorMax = new Vector2(0.5f, 0f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = dialogueTextPosition;
        textRect.sizeDelta = dialogueTextSize;

        dialogueText = textObject.GetComponent<TMP_Text>();
        dialogueText.text = "";
        dialogueText.fontSize = dialogueFontSize;
        dialogueText.fontStyle = FontStyles.Normal;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        dialogueText.enableWordWrapping = true;
        dialogueText.overflowMode = TextOverflowModes.Overflow;
        dialogueText.raycastTarget = false;
        dialogueText.richText = false;
        ApplyNormalGradient(dialogueText);

        if (dialogueFont != null)
            dialogueText.font = dialogueFont;

        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.18f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
    }

    private void CreatePortrait()
    {
        GameObject portraitObject = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
        portraitObject.transform.SetParent(dialogueBoxImage.transform, false);

        RectTransform rect = portraitObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = portraitSize;
        rect.anchoredPosition = portraitPosition;

        portraitImage = portraitObject.GetComponent<Image>();
        portraitImage.preserveAspect = true;
        portraitImage.raycastTarget = false;
    }

    private void CreateNameBox()
    {
        GameObject nameObject = new GameObject("NameBox", typeof(RectTransform), typeof(Image));
        nameObject.transform.SetParent(dialogueBoxImage.transform, false);

        RectTransform rect = nameObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = nameBoxSize;
        rect.anchoredPosition = nameBoxPosition;

        nameBoxImage = nameObject.GetComponent<Image>();
        ApplySpriteOrColor(nameBoxImage, nameBoxSprite, nameBoxColor);
        nameBoxImage.raycastTarget = false;

        AddBorder(nameObject.transform, dialogueBorderThickness);

        GameObject textObject = new GameObject("NameText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(nameObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 8f);
        textRect.offsetMax = new Vector2(-18f, -8f);

        nameText = textObject.GetComponent<TMP_Text>();
        nameText.text = "";
        nameText.fontSize = nameFontSize;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.raycastTarget = false;
        ApplyTitleGradient(nameText);

        if (nameFont != null)
            nameText.font = nameFont;

        nameObject.SetActive(false);
    }

    private void CreateNextArrow()
    {
        GameObject arrowObject = new GameObject("NextArrow", typeof(RectTransform), typeof(Image));
        arrowObject.transform.SetParent(dialogueBoxImage.transform, false);

        RectTransform rect = arrowObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.sizeDelta = nextArrowSize;
        rect.anchoredPosition = nextArrowPosition;

        arrowImage = arrowObject.GetComponent<Image>();
        arrowImage.raycastTarget = false;

        if (nextArrowSprite != null)
        {
            arrowImage.sprite = nextArrowSprite;
            arrowImage.color = Color.white;
            arrowImage.preserveAspect = true;
        }
        else
        {
            arrowImage.sprite = null;
            arrowImage.color = useGradientBorder ? titleTopRight : borderColor;
        }

        arrowObject.SetActive(false);
    }

    private void ApplyMainDialogueBoxBackground(Image image)
    {
        if (image == null)
            return;

        if (useMainDialogueBoxSprite && mainDialogueBoxSprite != null)
        {
            image.sprite = mainDialogueBoxSprite;
            image.color = mainDialogueBoxSpriteTint;

            image.type = useSlicedDialogueBoxSprite
                ? Image.Type.Sliced
                : Image.Type.Simple;
        }
        else
        {
            image.sprite = null;
            image.color = dialogueBoxColor;
            image.type = Image.Type.Simple;
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

    private void ApplySpriteOrColor(Image image, Sprite sprite, Color fallbackColor)
    {
        if (image == null)
            return;

        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Sliced;
        }
        else
        {
            image.sprite = null;
            image.color = fallbackColor;
            image.type = Image.Type.Simple;
        }
    }

    private void AddBorder(Transform parent, float thickness)
    {
        Color topColor = useGradientBorder ? titleTopLeft : borderColor;
        Color rightColor = useGradientBorder ? titleTopRight : borderColor;
        Color bottomColor = useGradientBorder ? titleBottomRight : borderColor;
        Color leftColor = useGradientBorder ? titleBottomLeft : borderColor;

        CreateBorderBar(
            parent,
            "TopBorder",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, thickness),
            topColor
        );

        CreateBorderBar(
            parent,
            "BottomBorder",
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, thickness),
            bottomColor
        );

        CreateBorderBar(
            parent,
            "LeftBorder",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0.5f),
            new Vector2(thickness, 0f),
            leftColor
        );

        CreateBorderBar(
            parent,
            "RightBorder",
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0.5f),
            new Vector2(thickness, 0f),
            rightColor
        );
    }

    private void CreateBorderBar(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 sizeDelta,
        Color barColor)
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
        image.color = barColor;
        image.raycastTarget = false;

        Outline outline = bar.GetComponent<Outline>();
        outline.effectColor = borderShadowColor;
        outline.effectDistance = new Vector2(1f, -1f);
    }
}