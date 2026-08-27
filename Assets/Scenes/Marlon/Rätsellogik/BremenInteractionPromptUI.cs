using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BremenInteractionPromptUI : MonoBehaviour
{
    public static BremenInteractionPromptUI Instance { get; private set; }

    [Header("Text")]
    [SerializeField] private string defaultText = "E  Interagieren";
    [SerializeField] private float fontSize = 30f;

    [Header("Position")]
    [SerializeField] private Vector2 promptPosition = new Vector2(0f, 120f);
    [SerializeField] private Vector2 promptSize = new Vector2(460f, 70f);

    [Header("Style")]
    [SerializeField] private Color boxColor = new Color(0.05f, 0.035f, 0.02f, 0.82f);
    [SerializeField] private Color borderColor = new Color(0.92f, 0.66f, 0.16f, 1f);
    [SerializeField] private Color textColor = new Color(1f, 0.9f, 0.68f, 1f);

    private GameObject rootObject;
    private TMP_Text promptText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildUI();
        HidePrompt();
    }

    private void BuildUI()
    {
        GameObject canvasObject = new GameObject(
            "InteractionPromptCanvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        rootObject = new GameObject("InteractionPromptBox", typeof(RectTransform), typeof(Image));
        rootObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0f);
        rootRect.anchorMax = new Vector2(0.5f, 0f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = promptPosition;
        rootRect.sizeDelta = promptSize;

        Image boxImage = rootObject.GetComponent<Image>();
        boxImage.color = boxColor;
        boxImage.raycastTarget = false;

        Outline outline = rootObject.AddComponent<Outline>();
        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject textObject = new GameObject("PromptText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(rootObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 6f);
        textRect.offsetMax = new Vector2(-18f, -6f);

        promptText = textObject.GetComponent<TMP_Text>();
        promptText.text = defaultText;
        promptText.fontSize = fontSize;
        promptText.fontStyle = FontStyles.Bold;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = textColor;
        promptText.raycastTarget = false;
    }

    public void ShowPrompt()
    {
        ShowPrompt(defaultText);
    }

    public void ShowPrompt(string text)
    {
        if (promptText != null)
            promptText.text = text;

        if (rootObject != null)
            rootObject.SetActive(true);
    }

    public void HidePrompt()
    {
        if (rootObject != null)
            rootObject.SetActive(false);
    }
}