using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Book UI wird automatisch gebaut.
/// Blättern und Schließen funktioniert nur noch über UI-Buttons.
/// Keine Steuerung mehr über Escape, A oder D.
/// </summary>
public class BookUI : MonoBehaviour
{
    [Header("Reference")]
    public BookInteractable bookInteractable;

    [Header("Button Sprites")]
    [SerializeField] private Sprite previousPageSprite;
    [SerializeField] private Sprite nextPageSprite;
    [SerializeField] private Sprite closeBookSprite;

    [Header("Fonts")]
    [SerializeField] private TMP_FontAsset headlineFont;
    [SerializeField] private TMP_FontAsset normalTextFont;
    [SerializeField] private TMP_FontAsset buttonFont;

    [Header("Colors")]
    [SerializeField] private Color bookBackgroundColor = new Color(0.10f, 0.07f, 0.04f, 0.95f);
    [SerializeField] private Color headlineColor = new Color(0.95f, 0.82f, 0.42f, 1f);
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color buttonColor = new Color(0.25f, 0.18f, 0.08f, 1f);
    [SerializeField] private Color buttonTextColor = new Color(0.9f, 0.8f, 0.5f, 1f);
    [SerializeField] private Color separatorColor = new Color(0.8f, 0.7f, 0.4f, 0.6f);

    [Header("Button Layout")]
    [SerializeField] private Vector2 previousButtonAnchorMin = new Vector2(0.08f, 0.02f);
    [SerializeField] private Vector2 previousButtonAnchorMax = new Vector2(0.18f, 0.10f);

    [SerializeField] private Vector2 nextButtonAnchorMin = new Vector2(0.82f, 0.02f);
    [SerializeField] private Vector2 nextButtonAnchorMax = new Vector2(0.92f, 0.10f);

    [SerializeField] private Vector2 closeButtonAnchorMin = new Vector2(0.43f, 0.02f);
    [SerializeField] private Vector2 closeButtonAnchorMax = new Vector2(0.57f, 0.10f);

    [Header("Sprite Settings")]
    [SerializeField] private bool hideButtonTextWhenSpriteIsUsed = true;
    [SerializeField] private bool preserveSpriteAspect = true;

    private GameObject bookPanel;
    private TextMeshProUGUI chapterTitle;
    private TextMeshProUGUI pageText;
    private TextMeshProUGUI pageIndicator;
    private TextMeshProUGUI hintText;
    private Button btnPrev;
    private Button btnNext;
    private Button btnClose;

    private struct Page
    {
        public string title;
        public string body;
    }

    private Page[] pages = new Page[]
    {
        new Page
        {
            title = "Kapitel 1 - Friedrich I. Barbarossa",
            body =
                "Friedrich I. Barbarossa war ein Herrscher aus dem Haus der Staufer und regierte von 1155 bis 1190.\n\n" +
                "Für Bremen ist er besonders wichtig, weil er im Jahr 1186 ein Privileg bestätigte, das der Stadt mehr Freiheit und Eigenständigkeit gab. Dadurch wurde Bremen auf seinem Weg zu einer selbstbewussten Stadt deutlich gestärkt.\n\n" +
                "Mit diesem Privileg wurde auch der bekannte Gedanke verbunden: „Stadtluft macht frei.“ Wer lange genug in der Stadt lebte, konnte sich aus bestimmten Abhängigkeiten lösen.\n\n" +
                "Um Friedrich I. Barbarossa auf dem Porträt zu erkennen, achte besonders auf seinen roten Bart und den kräftigen, warmen Gesichtston. Sein Beiname Barbarossa bedeutet genau das: Rotbart."
        },

        new Page
        {
            title = "Kapitel 2 - Karl V.",
            body =
                "Karl V. wurde im Jahr 1500 geboren und herrschte über ein riesiges Reich, über dem man sagte, dass dort die Sonne niemals untergeht.\n\n" +
                "Auch Bremen spielte in seiner Herrschaft eine wichtige Rolle. Die Stadt war protestantisch geprägt und stand deshalb in Konflikt mit der katholischen Reichspolitik Karls V.\n\n" +
                "Trotz dieser Spannungen verlieh Karl V. Bremen im Jahr 1541 das Recht, eigene Münzen zu prägen. Dieses Münzrecht war für die Stadt sehr bedeutend, weil es Bremens wirtschaftliche Stellung und Selbstständigkeit stärkte.\n\n" +
                "Um Karl V. zu erkennen, achte besonders auf seinen grauen Bart und sein ernstes Gesicht. Diese Merkmale helfen dir, sein Porträt von den anderen zu unterscheiden."
        },

        new Page
        {
            title = "Kapitel 3 - Leopold I.",
            body =
                "Leopold I. regierte besonders lange, nämlich von 1658 bis 1705. Seine Zeit war geprägt von Kriegen, religiösen Spannungen und Machtkämpfen in Europa.\n\n" +
                "Für Bremen war diese Epoche wichtig, weil die Stadt ihre Stellung als freie Reichsstadt behaupten musste. Bremen wollte möglichst eigenständig bleiben und seine politischen Rechte gegenüber größeren Mächten verteidigen.\n\n" +
                "Unter Leopold I. blieb Bremen Teil des Heiligen Römischen Reiches, konnte aber weiterhin seine städtischen Freiheiten und seine eigene Verwaltung bewahren. Das war wichtig für Bremens Selbstverständnis als unabhängige Handelsstadt.\n\n" +
                "Um Leopold I. auf dem Porträt zu erkennen, achte auf die große, voluminöse Perücke. Sie ist typisch für die Barockzeit und macht ihn besonders auffällig."
        }
    };

    private int currentPage = 0;

    private void Awake()
    {
        if (bookInteractable == null)
            bookInteractable = FindFirstObjectByType<BookInteractable>();

        EnsureEventSystem();
        BuildUI();
        Close();
    }

    private void Update()
    {
        // Keine Tastatursteuerung mehr.
        // Kein Escape, kein A, kein D.
    }

    public void Open()
    {
        currentPage = 0;

        if (bookPanel != null)
            bookPanel.SetActive(true);

        RefreshPage();

        Debug.Log("BOOK UI: Buch geöffnet.");
    }

    public void Close()
    {
        if (bookPanel != null)
            bookPanel.SetActive(false);

        Debug.Log("BOOK UI: Buch geschlossen.");
    }

    public void ShowHint(string text)
    {
        // Optional leer gelassen, weil das Buch-Panel erst beim Öffnen sichtbar ist.
    }

    public void HideHint()
    {
        // Optional leer gelassen.
    }

    private void BuildUI()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();

        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();

        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (gameObject.GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        bookPanel = new GameObject("BookPanel", typeof(RectTransform), typeof(Image));
        bookPanel.transform.SetParent(transform, false);

        Stretch(bookPanel.GetComponent<RectTransform>());

        Image panelImage = bookPanel.GetComponent<Image>();
        panelImage.color = bookBackgroundColor;

        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(bookPanel.transform, false);

        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.1f, 0.08f);
        contentRect.anchorMax = new Vector2(0.9f, 0.92f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        chapterTitle = MakeText(
            content.transform,
            "ChapterTitle",
            new Vector2(0f, 0.88f),
            new Vector2(0.7f, 1f),
            34,
            true,
            headlineFont,
            headlineColor
        );

        chapterTitle.alignment = TextAlignmentOptions.Left;

        pageIndicator = MakeText(
            content.transform,
            "PageIndicator",
            new Vector2(0.7f, 0.88f),
            new Vector2(1f, 1f),
            20,
            false,
            normalTextFont,
            normalTextColor
        );

        pageIndicator.alignment = TextAlignmentOptions.Right;

        GameObject separator = new GameObject("Separator", typeof(RectTransform), typeof(Image));
        separator.transform.SetParent(content.transform, false);

        RectTransform separatorRect = separator.GetComponent<RectTransform>();
        separatorRect.anchorMin = new Vector2(0f, 0.855f);
        separatorRect.anchorMax = new Vector2(1f, 0.858f);
        separatorRect.offsetMin = Vector2.zero;
        separatorRect.offsetMax = Vector2.zero;

        Image separatorImage = separator.GetComponent<Image>();
        separatorImage.color = separatorColor;

        pageText = MakeText(
            content.transform,
            "PageText",
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.84f),
            24,
            false,
            normalTextFont,
            normalTextColor
        );

        pageText.alignment = TextAlignmentOptions.TopLeft;

        hintText = MakeText(
            content.transform,
            "HintText",
            new Vector2(0f, 0f),
            new Vector2(1f, 0.10f),
            18,
            false,
            normalTextFont,
            buttonTextColor
        );

        hintText.alignment = TextAlignmentOptions.Center;

        btnPrev = MakeButton(
            bookPanel.transform,
            "BtnPrev",
            "◀",
            previousButtonAnchorMin,
            previousButtonAnchorMax,
            previousPageSprite
        );

        btnPrev.onClick.AddListener(PrevPage);

        btnNext = MakeButton(
            bookPanel.transform,
            "BtnNext",
            "▶",
            nextButtonAnchorMin,
            nextButtonAnchorMax,
            nextPageSprite
        );

        btnNext.onClick.AddListener(NextPage);

        btnClose = MakeButton(
            bookPanel.transform,
            "BtnClose",
            "Buch schließen",
            closeButtonAnchorMin,
            closeButtonAnchorMax,
            closeBookSprite
        );

        btnClose.onClick.AddListener(() =>
        {
            if (bookInteractable != null)
                bookInteractable.CloseBook();
            else
                Close();
        });

        Debug.Log("BOOK UI: UI wurde gebaut.");
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private TextMeshProUGUI MakeText(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        int fontSize,
        bool bold,
        TMP_FontAsset font,
        Color color)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();

        if (font != null)
            text.font = font;
        else
            text.font = TMP_Settings.defaultFontAsset;

        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        return text;
    }

    private Button MakeButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Sprite sprite)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = new Vector2(4f, 4f);
        rect.offsetMax = new Vector2(-4f, -4f);

        Image image = go.GetComponent<Image>();
        image.raycastTarget = true;

        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveSpriteAspect;
        }
        else
        {
            image.color = buttonColor;
        }

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;

        GameObject textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);

        Stretch(textGo.GetComponent<RectTransform>());

        TextMeshProUGUI text = textGo.GetComponent<TextMeshProUGUI>();

        if (buttonFont != null)
            text.font = buttonFont;
        else
            text.font = TMP_Settings.defaultFontAsset;

        text.fontSize = 20;
        text.color = buttonTextColor;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        if (sprite != null && hideButtonTextWhenSpriteIsUsed)
            text.text = "";
        else
            text.text = label;

        return button;
    }

    private void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshPage();
        }
    }

    private void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            RefreshPage();
        }
    }

    private void RefreshPage()
    {
        Page page = pages[currentPage];

        if (chapterTitle != null)
            chapterTitle.text = page.title;

        if (pageText != null)
            pageText.text = page.body;

        if (pageIndicator != null)
            pageIndicator.text = "Seite " + (currentPage + 1) + " / " + pages.Length;

        if (hintText != null)
            hintText.text = "";

        if (btnPrev != null)
            btnPrev.interactable = currentPage > 0;

        if (btnNext != null)
            btnNext.interactable = currentPage < pages.Length - 1;
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

        Debug.Log("BOOK UI: EventSystem wurde automatisch erstellt.");
    }
}   