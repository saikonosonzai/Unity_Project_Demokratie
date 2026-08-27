using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[System.Serializable]
public class EndScreenCreditTeam
{
    public string teamTitle;

    [TextArea(3, 10)]
    public string teamNames;
}

public class BremenEndScreenCreditsUI : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string mainMenuSceneName = "Startscreen";

    [Header("Background")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Color backgroundColor = new Color(0.04f, 0.018f, 0.01f, 1f);
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.45f);

    [Header("Music")]
    [SerializeField] private AudioClip endScreenMusic;
    [SerializeField] private bool playMusic = true;
    [SerializeField] private bool loopMusic = true;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.45f;

    [Header("Title")]
    [SerializeField] private string gameTitle = "Rathaus Escape";
    [SerializeField] private TMP_FontAsset titleFont;
    [SerializeField] private float titleFontSize = 86f;
    [SerializeField] private Color titleColor = new Color(0.82f, 0.68f, 0.36f, 1f);

    [Header("Title Gradient wie Startscreen")]
    [SerializeField] private bool useTitleGradient = true;
    [SerializeField] private Color titleTopLeft = new Color(0.95f, 0.82f, 0.42f, 1f);
    [SerializeField] private Color titleTopRight = new Color(0.82f, 0.68f, 0.34f, 1f);
    [SerializeField] private Color titleBottomLeft = new Color(0.42f, 0.30f, 0.10f, 1f);
    [SerializeField] private Color titleBottomRight = new Color(0.70f, 0.54f, 0.22f, 1f);

    [Header("Names Gradient wie Startscreen")]
    [SerializeField] private bool useNamesGradient = true;
    [SerializeField] private Color namesTopLeft = new Color(0.90f, 0.80f, 0.52f, 1f);
    [SerializeField] private Color namesTopRight = new Color(0.78f, 0.66f, 0.38f, 1f);
    [SerializeField] private Color namesBottomLeft = new Color(0.50f, 0.38f, 0.16f, 1f);
    [SerializeField] private Color namesBottomRight = new Color(0.68f, 0.54f, 0.28f, 1f);

    [Header("Credits Fonts")]
    [SerializeField] private TMP_FontAsset teamTitleFont;
    [SerializeField] private TMP_FontAsset namesFont;

    [Header("Credits Colors")]
    [SerializeField] private Color teamTitleColor = new Color(0.95f, 0.78f, 0.36f, 1f);
    [SerializeField] private Color namesColor = new Color(0.90f, 0.82f, 0.62f, 1f);

    [Header("Credits Text Sizes")]
    [SerializeField] private float teamTitleFontSize = 42f;
    [SerializeField] private float namesFontSize = 30f;

    [Header("Credits Content")]
    [SerializeField] private EndScreenCreditTeam[] creditTeams;

    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 55f;

    // Kleinerer negativer Wert = Titel kommt früher ins Bild
    [SerializeField] private float startYPosition = -480f;

    [SerializeField] private bool autoReturnToMainMenu = true;
    [SerializeField] private float waitAfterCredits = 2f;

    // Wie weit nach dem letzten Namen noch weitergescrollt wird
    [SerializeField] private float extraScrollAfterLastName = 180f;

    [Header("Layout")]
    [SerializeField] private Vector2 titleSize = new Vector2(1200f, 120f);
    [SerializeField] private Vector2 teamTitleSize = new Vector2(900f, 70f);
    [SerializeField] private Vector2 namesTextSize = new Vector2(900f, 180f);

    [SerializeField] private float spaceAfterMainTitle = 120f;
    [SerializeField] private float spaceAfterTeamTitle = 18f;
    [SerializeField] private float spaceAfterNames = 90f;

    private RectTransform creditsContentRect;
    private AudioSource musicSource;

    private bool creditsFinished;
    private float finishedTimer;
    private float finishScrollY;

    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        EnsureEventSystem();

        BuildCanvas();
        BuildBackground();
        BuildCreditsContent();

        SetupMusic();
    }

    private void Update()
    {
        if (creditsContentRect == null)
            return;

        if (!creditsFinished)
        {
            creditsContentRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

            if (creditsContentRect.anchoredPosition.y >= finishScrollY)
            {
                creditsFinished = true;
                finishedTimer = 0f;
            }
        }
        else
        {
            if (autoReturnToMainMenu)
            {
                finishedTimer += Time.deltaTime;

                if (finishedTimer >= waitAfterCredits)
                    LoadMainMenu();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            LoadMainMenu();
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

        StretchFull(rect);
    }

    private void BuildBackground()
    {
        GameObject backgroundObject = CreateUIObject("Background", transform);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        StretchFull(backgroundRect);

        Image backgroundImage = backgroundObject.AddComponent<Image>();
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

        GameObject overlayObject = CreateUIObject("Overlay", transform);
        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        StretchFull(overlayRect);

        Image overlayImage = overlayObject.AddComponent<Image>();
        overlayImage.color = overlayColor;
        overlayImage.raycastTarget = false;
    }

    private void BuildCreditsContent()
    {
        GameObject contentObject = CreateUIObject("CreditsContent", transform);
        creditsContentRect = contentObject.GetComponent<RectTransform>();

        creditsContentRect.anchorMin = new Vector2(0.5f, 0.5f);
        creditsContentRect.anchorMax = new Vector2(0.5f, 0.5f);
        creditsContentRect.pivot = new Vector2(0.5f, 0.5f);
        creditsContentRect.anchoredPosition = new Vector2(0f, startYPosition);
        creditsContentRect.sizeDelta = new Vector2(1400f, 4000f);

        float currentY = 0f;
        float lowestTextY = 0f;

        TMP_Text title = CreateText(
            "GameTitle",
            gameTitle,
            contentObject.transform,
            new Vector2(0f, currentY),
            titleSize,
            titleFontSize,
            titleColor,
            FontStyles.Bold
        );

        if (titleFont != null)
            title.font = titleFont;

        title.alignment = TextAlignmentOptions.Center;
        ApplyTitleGradient(title);
        AddTextShadow(title.gameObject, new Color(0f, 0f, 0f, 0.85f), new Vector2(4f, -4f));

        lowestTextY = currentY - titleSize.y * 0.5f;
        currentY -= spaceAfterMainTitle;

        if (creditTeams == null || creditTeams.Length == 0)
        {
            TMP_Text placeholder = CreateText(
                "NoCreditsText",
                "Noch keine Credits eingetragen.",
                contentObject.transform,
                new Vector2(0f, currentY),
                namesTextSize,
                namesFontSize,
                namesColor,
                FontStyles.Normal
            );

            placeholder.alignment = TextAlignmentOptions.Center;
            ApplyNamesGradient(placeholder);

            lowestTextY = currentY - namesTextSize.y * 0.5f;
            CalculateFinishScrollY(lowestTextY);
            return;
        }

        for (int i = 0; i < creditTeams.Length; i++)
        {
            EndScreenCreditTeam team = creditTeams[i];

            string teamTitle = string.IsNullOrWhiteSpace(team.teamTitle)
                ? "Team " + (i + 1)
                : team.teamTitle;

            string teamNames = string.IsNullOrWhiteSpace(team.teamNames)
                ? "Namen hier eintragen"
                : team.teamNames;

            TMP_Text teamTitleText = CreateText(
                teamTitle + "_Title",
                teamTitle,
                contentObject.transform,
                new Vector2(0f, currentY),
                teamTitleSize,
                teamTitleFontSize,
                teamTitleColor,
                FontStyles.Bold
            );

            if (teamTitleFont != null)
                teamTitleText.font = teamTitleFont;

            teamTitleText.alignment = TextAlignmentOptions.Center;
            ApplyTitleGradient(teamTitleText);
            AddTextShadow(teamTitleText.gameObject, new Color(0f, 0f, 0f, 0.75f), new Vector2(2.5f, -2.5f));

            lowestTextY = currentY - teamTitleSize.y * 0.5f;

            currentY -= spaceAfterTeamTitle + teamTitleSize.y * 0.5f;

            TMP_Text namesText = CreateText(
                teamTitle + "_Names",
                teamNames,
                contentObject.transform,
                new Vector2(0f, currentY),
                namesTextSize,
                namesFontSize,
                namesColor,
                FontStyles.Normal
            );

            if (namesFont != null)
                namesText.font = namesFont;

            namesText.alignment = TextAlignmentOptions.Center;
            namesText.verticalAlignment = VerticalAlignmentOptions.Middle;
            ApplyNamesGradient(namesText);

            lowestTextY = currentY - namesTextSize.y * 0.5f;

            currentY -= namesTextSize.y + spaceAfterNames;
        }

        CalculateFinishScrollY(lowestTextY);
    }

    private void CalculateFinishScrollY(float lowestTextY)
    {
        // 540 ist die halbe Höhe von 1080p.
        // Sobald die Unterkante vom letzten Namen über dem oberen Bildschirmrand ist,
        // sind keine Namen mehr sichtbar.
        float screenTopY = 540f;

        finishScrollY = screenTopY + extraScrollAfterLastName - lowestTextY;
    }

    private void ApplyTitleGradient(TMP_Text text)
    {
        if (text == null || !useTitleGradient)
            return;

        text.enableVertexGradient = true;
        text.colorGradient = new VertexGradient(
            titleTopLeft,
            titleTopRight,
            titleBottomLeft,
            titleBottomRight
        );
    }

    private void ApplyNamesGradient(TMP_Text text)
    {
        if (text == null || !useNamesGradient)
            return;

        text.enableVertexGradient = true;
        text.colorGradient = new VertexGradient(
            namesTopLeft,
            namesTopRight,
            namesBottomLeft,
            namesBottomRight
        );
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

        return text;
    }

    private void SetupMusic()
    {
        if (!playMusic)
            return;

        if (endScreenMusic == null)
            return;

        musicSource = GetComponent<AudioSource>();

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.clip = endScreenMusic;
        musicSource.loop = loopMusic;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;
        musicSource.spatialBlend = 0f;

        musicSource.Play();
    }

    private void LoadMainMenu()
    {
        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogWarning("Kein Main Menu Scene Name eingetragen.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
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