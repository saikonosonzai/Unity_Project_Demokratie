using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum DemocracyResourceType
{
    Freiheit,
    Wasser,
    Gleichberechtigung,
    Wahlen
}

public class BremenDemocracyBlockPuzzleUI : MonoBehaviour
{
    [Header("UI")]
    public Transform boardRoot;
    public Image backgroundImage;
    public TMP_Text infoText;

    [Header("Überschrift")]
    public bool showTitle = true;
    public TMP_Text titleText;
    public string titleContent = "Demokratie-Bausteine";
    public TMP_FontAsset titleFont;
    public float titleFontSize = 38f;
    public Color titleColor = new Color(0.95f, 0.78f, 0.28f, 1f);
    public Vector2 titlePosition = new Vector2(0f, 415f);
    public Vector2 titleSize = new Vector2(900f, 70f);

    [Header("Beschreibung rechts")]
    public bool showDescription = true;
    public TMP_Text descriptionText;

    [TextArea(4, 10)]
    public string descriptionContent =
        "Ziehe die 2x2-Blöcke so, dass jeder farbige Bereich alle vier Werte enthält.\n\n" +
        "Jeder Bereich braucht Freiheit, Wasser, Gleichberechtigung und Wahlen.";

    public TMP_FontAsset descriptionFont;
    public float descriptionFontSize = 21f;
    public Color descriptionColor = new Color(0.92f, 0.86f, 0.72f, 1f);
    public Vector2 descriptionPosition = new Vector2(610f, 60f);
    public Vector2 descriptionSize = new Vector2(420f, 250f);

    [Header("Symbol-Legende links")]
    public bool showLegend = true;

    public TMP_Text legendTitleText;
    public string legendTitleContent = "Werte";
    public TMP_FontAsset legendTitleFont;
    public float legendTitleFontSize = 28f;
    public Color legendTitleColor = new Color(0.95f, 0.78f, 0.28f, 1f);
    public Vector2 legendTitlePosition = new Vector2(-610f, 275f);
    public Vector2 legendTitleSize = new Vector2(360f, 50f);

    [Header("Legende Sprites")]
    public Sprite legendSprite1;
    public Sprite legendSprite2;
    public Sprite legendSprite3;
    public Sprite legendSprite4;

    public string legendName1 = "Freiheit";
    public string legendName2 = "Wasser";
    public string legendName3 = "Gleichberechtigung";
    public string legendName4 = "Wahlen";

    public TMP_FontAsset legendNameFont;
    public float legendNameFontSize = 18f;
    public Color legendNameColor = new Color(0.92f, 0.86f, 0.72f, 1f);

    public Vector2 legendStartPosition = new Vector2(-610f, 195f);
    public Vector2 legendSpriteSize = new Vector2(72f, 72f);
    public Vector2 legendNameSize = new Vector2(220f, 32f);
    public float legendVerticalSpacing = 112f;

    private Image[] legendImages;
    private TMP_Text[] legendNameTexts;

    [Header("Exit")]
    public PuzzleInteractable puzzleInteractable;
    public float closeDelay = 0.2f;

    [Header("Optional Sprites")]
    public Sprite freiheitSprite;
    public Sprite wasserSprite;
    public Sprite gleichberechtigungSprite;
    public Sprite wahlenSprite;

    [Header("Optional Frame Texture")]
    public Sprite frameTextureSprite;

    [Header("Grid")]
    public int width = 6;
    public int height = 6;
    public Vector2 cellSize = new Vector2(76f, 76f);
    public Vector2 spacing = new Vector2(2f, 2f);

    [Header("Background Fit")]
    public float backgroundPadding = 0f;

    [Header("Colors - Zones")]
    public Color zone1Color = new Color(0.2f, 0.45f, 1f, 0.18f);
    public Color zone2Color = new Color(0.2f, 1f, 0.35f, 0.18f);
    public Color zone3Color = new Color(1f, 0.85f, 0.2f, 0.18f);
    public Color zone4Color = new Color(1f, 0.35f, 0.35f, 0.18f);

    [Header("Wood Frame")]
    public Color woodColor = new Color(0.45f, 0.25f, 0.10f, 1f);
    public Color darkWoodColor = new Color(0.18f, 0.09f, 0.03f, 1f);
    public Color selectedColor = new Color(1f, 0.85f, 0.35f, 1f);

    [Header("Solved Event")]
    public UnityEvent OnPuzzleSolved;

    private const int Empty = -1;

    private int[,] pieceAt;
    private DemocracyResourceType[,] resourceAt;

    private Vector2Int[] piecePositions;
    private DemocracyResourceType[][] pieceResources;
    private BremenDemocracyPieceUI[] pieces;

    private RectTransform boardRect;
    private RectTransform pieceRoot;

    private bool puzzleSolved;

    /*
     Schwerere, aber lösbare Zonenverteilung.

     Sichtbar von oben nach unten:

     3 3 3 4 4 4
     3 3 3 4 4 4
     1 3 3 4 4 4
     1 1 1 2 2 4
     1 1 2 2 2 2
     1 1 1 2 2 2
    */
    private readonly int[] zoneIds =
    {
        // y = 0, unterste Reihe
        1, 1, 1, 2, 2, 2,

        // y = 1
        1, 1, 2, 2, 2, 2,

        // y = 2
        1, 1, 1, 2, 2, 4,

        // y = 3
        1, 3, 3, 4, 4, 4,

        // y = 4
        3, 3, 3, 4, 4, 4,

        // y = 5, oberste Reihe
        3, 3, 3, 4, 4, 4
    };

    private void OnEnable()
    {
        StartPuzzle();
    }

    public void StartPuzzle()
    {
        Debug.Log("Bremen Map Puzzle startet.");

        if (boardRoot == null)
        {
            Debug.LogError("BoardRoot fehlt im Inspector.");
            return;
        }

        CancelInvoke();

        puzzleSolved = false;

        CreateExtraUI();
        ApplyExtraUISettings();

        FitBoardAndBackground();
        CreatePieceRoot();
        CreateGridSlots();
        CreatePiecesData();
        CreatePiecesVisuals();
        GenerateStartBoard();
        RenderPieces();

        SetInfo("Ziehe die 2x2-Blöcke so, dass jeder Bereich alle vier Werte enthält.");
    }

    private void CreateExtraUI()
    {
        CreateTitleIfNeeded();
        CreateDescriptionIfNeeded();
        CreateLegendIfNeeded();
    }

    private void CreateTitleIfNeeded()
    {
        if (!showTitle)
        {
            if (titleText != null)
                titleText.gameObject.SetActive(false);

            return;
        }

        if (titleText != null)
        {
            titleText.gameObject.SetActive(true);
            return;
        }

        GameObject titleObject = new GameObject("PuzzleTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(transform, false);

        RectTransform rect = titleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = titlePosition;
        rect.sizeDelta = titleSize;

        titleText = titleObject.GetComponent<TMP_Text>();
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.raycastTarget = false;
        titleText.enableWordWrapping = true;
    }

    private void CreateDescriptionIfNeeded()
    {
        if (!showDescription)
        {
            if (descriptionText != null)
                descriptionText.gameObject.SetActive(false);

            return;
        }

        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(true);
            return;
        }

        GameObject descriptionObject = new GameObject("PuzzleDescriptionText", typeof(RectTransform), typeof(TextMeshProUGUI));
        descriptionObject.transform.SetParent(transform, false);

        RectTransform rect = descriptionObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = descriptionPosition;
        rect.sizeDelta = descriptionSize;

        descriptionText = descriptionObject.GetComponent<TMP_Text>();
        descriptionText.alignment = TextAlignmentOptions.TopLeft;
        descriptionText.raycastTarget = false;
        descriptionText.enableWordWrapping = true;
    }

    private void CreateLegendIfNeeded()
    {
        if (!showLegend)
        {
            if (legendTitleText != null)
                legendTitleText.gameObject.SetActive(false);

            if (legendImages != null)
            {
                for (int i = 0; i < legendImages.Length; i++)
                {
                    if (legendImages[i] != null)
                        legendImages[i].gameObject.SetActive(false);
                }
            }

            if (legendNameTexts != null)
            {
                for (int i = 0; i < legendNameTexts.Length; i++)
                {
                    if (legendNameTexts[i] != null)
                        legendNameTexts[i].gameObject.SetActive(false);
                }
            }

            return;
        }

        CreateLegendTitleIfNeeded();

        if (legendImages == null || legendImages.Length != 4)
            legendImages = new Image[4];

        if (legendNameTexts == null || legendNameTexts.Length != 4)
            legendNameTexts = new TMP_Text[4];

        for (int i = 0; i < 4; i++)
            CreateLegendEntryIfNeeded(i);
    }

    private void CreateLegendTitleIfNeeded()
    {
        if (legendTitleText != null)
        {
            legendTitleText.gameObject.SetActive(true);
            return;
        }

        GameObject legendTitleObject = new GameObject("LegendTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        legendTitleObject.transform.SetParent(transform, false);

        RectTransform rect = legendTitleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = legendTitlePosition;
        rect.sizeDelta = legendTitleSize;

        legendTitleText = legendTitleObject.GetComponent<TMP_Text>();
        legendTitleText.alignment = TextAlignmentOptions.Center;
        legendTitleText.raycastTarget = false;
        legendTitleText.enableWordWrapping = true;
    }

    private void CreateLegendEntryIfNeeded(int index)
    {
        if (legendImages[index] == null)
        {
            GameObject imageObject = new GameObject("LegendSprite_" + index, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(transform, false);

            Image image = imageObject.GetComponent<Image>();
            image.raycastTarget = false;
            image.preserveAspect = true;

            legendImages[index] = image;
        }
        else
        {
            legendImages[index].gameObject.SetActive(true);
        }

        if (legendNameTexts[index] == null)
        {
            GameObject nameObject = new GameObject("LegendName_" + index, typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObject.transform.SetParent(transform, false);

            TMP_Text nameText = nameObject.GetComponent<TMP_Text>();
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.raycastTarget = false;
            nameText.enableWordWrapping = true;

            legendNameTexts[index] = nameText;
        }
        else
        {
            legendNameTexts[index].gameObject.SetActive(true);
        }
    }

    private void ApplyExtraUISettings()
    {
        ApplyTitleSettings();
        ApplyDescriptionSettings();
        ApplyLegendSettings();
    }

    private void ApplyTitleSettings()
    {
        if (titleText == null)
            return;

        titleText.gameObject.SetActive(showTitle);
        titleText.text = titleContent;
        titleText.fontSize = titleFontSize;
        titleText.color = titleColor;
        titleText.alignment = TextAlignmentOptions.Center;

        if (titleFont != null)
            titleText.font = titleFont;

        RectTransform rect = titleText.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = titlePosition;
            rect.sizeDelta = titleSize;
        }
    }

    private void ApplyDescriptionSettings()
    {
        if (descriptionText == null)
            return;

        descriptionText.gameObject.SetActive(showDescription);
        descriptionText.text = descriptionContent;
        descriptionText.fontSize = descriptionFontSize;
        descriptionText.color = descriptionColor;
        descriptionText.alignment = TextAlignmentOptions.TopLeft;

        if (descriptionFont != null)
            descriptionText.font = descriptionFont;

        RectTransform rect = descriptionText.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = descriptionPosition;
            rect.sizeDelta = descriptionSize;
        }
    }

    private void ApplyLegendSettings()
    {
        if (!showLegend)
            return;

        if (legendTitleText != null)
        {
            legendTitleText.gameObject.SetActive(true);
            legendTitleText.text = legendTitleContent;
            legendTitleText.fontSize = legendTitleFontSize;
            legendTitleText.color = legendTitleColor;
            legendTitleText.alignment = TextAlignmentOptions.Center;

            if (legendTitleFont != null)
                legendTitleText.font = legendTitleFont;

            RectTransform titleRect = legendTitleText.GetComponent<RectTransform>();

            if (titleRect != null)
            {
                titleRect.anchorMin = new Vector2(0.5f, 0.5f);
                titleRect.anchorMax = new Vector2(0.5f, 0.5f);
                titleRect.pivot = new Vector2(0.5f, 0.5f);
                titleRect.anchoredPosition = legendTitlePosition;
                titleRect.sizeDelta = legendTitleSize;
            }
        }

        if (legendImages == null || legendNameTexts == null)
            return;

        Sprite[] sprites =
        {
            legendSprite1 != null ? legendSprite1 : freiheitSprite,
            legendSprite2 != null ? legendSprite2 : wasserSprite,
            legendSprite3 != null ? legendSprite3 : gleichberechtigungSprite,
            legendSprite4 != null ? legendSprite4 : wahlenSprite
        };

        string[] names =
        {
            legendName1,
            legendName2,
            legendName3,
            legendName4
        };

        for (int i = 0; i < 4; i++)
        {
            Vector2 spritePosition = legendStartPosition + new Vector2(0f, -legendVerticalSpacing * i);
            Vector2 namePosition = spritePosition + new Vector2(0f, -legendSpriteSize.y * 0.5f - 22f);

            if (legendImages[i] != null)
            {
                legendImages[i].gameObject.SetActive(true);
                legendImages[i].sprite = sprites[i];
                legendImages[i].color = sprites[i] != null ? Color.white : new Color(1f, 1f, 1f, 0f);
                legendImages[i].preserveAspect = true;

                RectTransform imageRect = legendImages[i].GetComponent<RectTransform>();

                if (imageRect != null)
                {
                    imageRect.anchorMin = new Vector2(0.5f, 0.5f);
                    imageRect.anchorMax = new Vector2(0.5f, 0.5f);
                    imageRect.pivot = new Vector2(0.5f, 0.5f);
                    imageRect.anchoredPosition = spritePosition;
                    imageRect.sizeDelta = legendSpriteSize;
                }
            }

            if (legendNameTexts[i] != null)
            {
                legendNameTexts[i].gameObject.SetActive(true);
                legendNameTexts[i].text = names[i];
                legendNameTexts[i].fontSize = legendNameFontSize;
                legendNameTexts[i].color = legendNameColor;
                legendNameTexts[i].alignment = TextAlignmentOptions.Center;

                if (legendNameFont != null)
                    legendNameTexts[i].font = legendNameFont;

                RectTransform nameRect = legendNameTexts[i].GetComponent<RectTransform>();

                if (nameRect != null)
                {
                    nameRect.anchorMin = new Vector2(0.5f, 0.5f);
                    nameRect.anchorMax = new Vector2(0.5f, 0.5f);
                    nameRect.pivot = new Vector2(0.5f, 0.5f);
                    nameRect.anchoredPosition = namePosition;
                    nameRect.sizeDelta = legendNameSize;
                }
            }
        }
    }

    private void FitBoardAndBackground()
    {
        boardRect = boardRoot as RectTransform;

        if (boardRect == null)
        {
            Debug.LogError("BoardRoot braucht einen RectTransform.");
            return;
        }

        float boardWidth = GetBoardWidth();
        float boardHeight = GetBoardHeight();

        boardRect.anchorMin = new Vector2(0.5f, 0.5f);
        boardRect.anchorMax = new Vector2(0.5f, 0.5f);
        boardRect.pivot = new Vector2(0.5f, 0.5f);
        boardRect.anchoredPosition = Vector2.zero;
        boardRect.sizeDelta = new Vector2(boardWidth, boardHeight);
        boardRect.localScale = Vector3.one;

        if (backgroundImage != null)
        {
            RectTransform bgRect = backgroundImage.rectTransform;

            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(
                boardWidth + backgroundPadding * 2f,
                boardHeight + backgroundPadding * 2f
            );

            backgroundImage.color = Color.white;
            backgroundImage.preserveAspect = false;
            backgroundImage.raycastTarget = false;
            backgroundImage.transform.SetAsFirstSibling();
        }
    }

    private void CreatePieceRoot()
    {
        if (pieceRoot != null)
            Destroy(pieceRoot.gameObject);

        GameObject pieceRootObject = new GameObject("PieceRoot", typeof(RectTransform));
        pieceRootObject.transform.SetParent(boardRoot.parent, false);

        pieceRoot = pieceRootObject.GetComponent<RectTransform>();
        pieceRoot.anchorMin = new Vector2(0.5f, 0.5f);
        pieceRoot.anchorMax = new Vector2(0.5f, 0.5f);
        pieceRoot.pivot = new Vector2(0.5f, 0.5f);
        pieceRoot.anchoredPosition = Vector2.zero;
        pieceRoot.sizeDelta = boardRect.sizeDelta;
        pieceRoot.localScale = Vector3.one;

        pieceRoot.SetAsLastSibling();
    }

    private void CreateGridSlots()
    {
        for (int i = boardRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(boardRoot.GetChild(i).gameObject);
        }

        GridLayoutGroup grid = boardRoot.GetComponent<GridLayoutGroup>();

        if (grid == null)
            grid = boardRoot.gameObject.AddComponent<GridLayoutGroup>();

        grid.cellSize = cellSize;
        grid.spacing = spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = width;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;

        for (int visualY = height - 1; visualY >= 0; visualY--)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject slotObject = new GameObject(
                    "Slot_" + x + "_" + visualY,
                    typeof(RectTransform),
                    typeof(Image)
                );

                slotObject.transform.SetParent(boardRoot, false);

                Image image = slotObject.GetComponent<Image>();
                image.color = GetZoneColor(GetZoneId(x, visualY));
                image.raycastTarget = false;
            }
        }
    }

    private void CreatePiecesData()
    {
        pieceAt = new int[width, height];
        resourceAt = new DemocracyResourceType[width, height];

        piecePositions = new Vector2Int[4];
        pieceResources = new DemocracyResourceType[4][];
        pieces = new BremenDemocracyPieceUI[4];

        pieceResources[0] = new DemocracyResourceType[]
        {
            DemocracyResourceType.Freiheit,
            DemocracyResourceType.Wasser,
            DemocracyResourceType.Wahlen,
            DemocracyResourceType.Freiheit
        };

        pieceResources[1] = new DemocracyResourceType[]
        {
            DemocracyResourceType.Wasser,
            DemocracyResourceType.Freiheit,
            DemocracyResourceType.Wahlen,
            DemocracyResourceType.Wasser
        };

        pieceResources[2] = new DemocracyResourceType[]
        {
            DemocracyResourceType.Freiheit,
            DemocracyResourceType.Wasser,
            DemocracyResourceType.Wahlen,
            DemocracyResourceType.Wahlen
        };

        pieceResources[3] = new DemocracyResourceType[]
        {
            DemocracyResourceType.Gleichberechtigung,
            DemocracyResourceType.Gleichberechtigung,
            DemocracyResourceType.Gleichberechtigung,
            DemocracyResourceType.Gleichberechtigung
        };
    }

    private void CreatePiecesVisuals()
    {
        for (int i = pieceRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(pieceRoot.GetChild(i).gameObject);
        }

        for (int pieceId = 0; pieceId < 4; pieceId++)
        {
            GameObject pieceObject = new GameObject(
                "Piece_" + pieceId,
                typeof(RectTransform),
                typeof(Image),
                typeof(CanvasGroup),
                typeof(BremenDemocracyPieceUI)
            );

            pieceObject.transform.SetParent(pieceRoot, false);

            RectTransform rect = pieceObject.GetComponent<RectTransform>();
            rect.sizeDelta = GetPieceSize();

            Image hitbox = pieceObject.GetComponent<Image>();
            hitbox.color = new Color(1f, 1f, 1f, 0.01f);
            hitbox.raycastTarget = true;

            BremenDemocracyPieceUI piece = pieceObject.GetComponent<BremenDemocracyPieceUI>();
            piece.Init(this, pieceId, pieceResources[pieceId]);

            pieces[pieceId] = piece;
        }
    }

    private void GenerateStartBoard()
    {
        ClearBoard();

        PlacePiece(0, new Vector2Int(0, 0));
        PlacePiece(1, new Vector2Int(2, 0));
        PlacePiece(2, new Vector2Int(0, 4));
        PlacePiece(3, new Vector2Int(4, 3));
    }

    private void ClearBoard()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                pieceAt[x, y] = Empty;
            }
        }
    }

    private void PlacePiece(int pieceId, Vector2Int lowerLeft)
    {
        piecePositions[pieceId] = lowerLeft;

        DemocracyResourceType[] resources = pieceResources[pieceId];

        SetCell(lowerLeft.x, lowerLeft.y, pieceId, resources[0]);
        SetCell(lowerLeft.x + 1, lowerLeft.y, pieceId, resources[1]);
        SetCell(lowerLeft.x, lowerLeft.y + 1, pieceId, resources[2]);
        SetCell(lowerLeft.x + 1, lowerLeft.y + 1, pieceId, resources[3]);

        if (pieces[pieceId] != null)
        {
            pieces[pieceId].SetGridPosition(lowerLeft);
            pieces[pieceId].SnapTo(GetPieceAnchoredPosition(lowerLeft));
        }
    }

    private void SetCell(int x, int y, int pieceId, DemocracyResourceType resource)
    {
        pieceAt[x, y] = pieceId;
        resourceAt[x, y] = resource;
    }

    private void ClearPiece(int pieceId)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pieceAt[x, y] == pieceId)
                    pieceAt[x, y] = Empty;
            }
        }
    }

    public bool TryMovePiece(BremenDemocracyPieceUI piece, Vector2 targetAnchoredPosition)
    {
        if (puzzleSolved)
            return false;

        int pieceId = piece.PieceId;
        Vector2Int oldPosition = piecePositions[pieceId];
        Vector2Int targetPosition = AnchoredPositionToLowerLeftGrid(targetAnchoredPosition);

        ClearPiece(pieceId);

        if (!CanPlacePieceAt(targetPosition))
        {
            PlacePiece(pieceId, oldPosition);
            SetInfo("Dort passt der 2x2-Block nicht hin.");
            return false;
        }

        PlacePiece(pieceId, targetPosition);
        SetInfo("Block verschoben.");

        CheckWinCondition();

        return true;
    }

    private bool CanPlacePieceAt(Vector2Int lowerLeft)
    {
        if (lowerLeft.x < 0 || lowerLeft.y < 0)
            return false;

        if (lowerLeft.x > width - 2 || lowerLeft.y > height - 2)
            return false;

        for (int dx = 0; dx < 2; dx++)
        {
            for (int dy = 0; dy < 2; dy++)
            {
                int x = lowerLeft.x + dx;
                int y = lowerLeft.y + dy;

                if (pieceAt[x, y] != Empty)
                    return false;
            }
        }

        return true;
    }

    private void CheckWinCondition()
    {
        Dictionary<int, HashSet<DemocracyResourceType>> zoneResources =
            new Dictionary<int, HashSet<DemocracyResourceType>>();

        Dictionary<int, int> zoneCounts =
            new Dictionary<int, int>();

        for (int zone = 1; zone <= 4; zone++)
        {
            zoneResources[zone] = new HashSet<DemocracyResourceType>();
            zoneCounts[zone] = 0;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pieceAt[x, y] == Empty)
                    continue;

                int zoneId = GetZoneId(x, y);

                if (zoneId < 1 || zoneId > 4)
                    continue;

                zoneResources[zoneId].Add(resourceAt[x, y]);
                zoneCounts[zoneId]++;
            }
        }

        for (int zone = 1; zone <= 4; zone++)
        {
            if (zoneCounts[zone] != 4)
                return;

            if (zoneResources[zone].Count != 4)
                return;
        }

        puzzleSolved = true;

        SetInfo("Gelöst! Jeder Bereich enthält alle vier Werte.");
        Debug.Log("Puzzle gelöst!");

        Invoke(nameof(ClosePuzzleUI), closeDelay);
    }

    private void ClosePuzzleUI()
    {
        if (puzzleInteractable != null)
        {
            puzzleInteractable.ClosePuzzleAfterSolved();
            Debug.Log("PuzzleInteractable.ClosePuzzleAfterSolved wurde aufgerufen.");
        }
        else
        {
            Debug.LogWarning("PuzzleInteractable ist nicht eingetragen.");
        }

        OnPuzzleSolved?.Invoke();
    }

    private void RenderPieces()
    {
        if (pieces == null)
            return;

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null)
                continue;

            pieces[i].Render();
        }
    }

    public Vector2 GetPieceAnchoredPosition(Vector2Int lowerLeft)
    {
        Vector2 slotCenter = GetSlotCenterPosition(lowerLeft);
        Vector2 stride = GetStride();

        return new Vector2(
            slotCenter.x + stride.x / 2f,
            slotCenter.y + stride.y / 2f
        );
    }

    private Vector2 GetSlotCenterPosition(Vector2Int gridPosition)
    {
        float boardWidth = GetBoardWidth();
        float boardHeight = GetBoardHeight();

        Vector2 stride = GetStride();

        float firstX = -boardWidth / 2f + cellSize.x / 2f;
        float firstY = -boardHeight / 2f + cellSize.y / 2f;

        return new Vector2(
            firstX + gridPosition.x * stride.x,
            firstY + gridPosition.y * stride.y
        );
    }

    public Vector2Int AnchoredPositionToLowerLeftGrid(Vector2 anchoredPosition)
    {
        float boardWidth = GetBoardWidth();
        float boardHeight = GetBoardHeight();

        Vector2 stride = GetStride();

        float firstX = -boardWidth / 2f + cellSize.x / 2f;
        float firstY = -boardHeight / 2f + cellSize.y / 2f;

        int x = Mathf.RoundToInt((anchoredPosition.x - firstX - stride.x / 2f) / stride.x);
        int y = Mathf.RoundToInt((anchoredPosition.y - firstY - stride.y / 2f) / stride.y);

        return new Vector2Int(x, y);
    }

    public Vector2 GetCellLocalPositionInPiece(int localX, int localY)
    {
        Vector2 stride = GetStride();

        return new Vector2(
            localX == 0 ? -stride.x / 2f : stride.x / 2f,
            localY == 0 ? -stride.y / 2f : stride.y / 2f
        );
    }

    public Vector2 GetPieceSize()
    {
        return new Vector2(
            cellSize.x * 2f + spacing.x,
            cellSize.y * 2f + spacing.y
        );
    }

    public Vector2 GetStride()
    {
        return new Vector2(
            cellSize.x + spacing.x,
            cellSize.y + spacing.y
        );
    }

    private float GetBoardWidth()
    {
        return width * cellSize.x + (width - 1) * spacing.x;
    }

    private float GetBoardHeight()
    {
        return height * cellSize.y + (height - 1) * spacing.y;
    }

    public Sprite GetSprite(DemocracyResourceType resource)
    {
        switch (resource)
        {
            case DemocracyResourceType.Freiheit:
                return freiheitSprite;

            case DemocracyResourceType.Wasser:
                return wasserSprite;

            case DemocracyResourceType.Gleichberechtigung:
                return gleichberechtigungSprite;

            case DemocracyResourceType.Wahlen:
                return wahlenSprite;

            default:
                return null;
        }
    }

    public string GetShortLabel(DemocracyResourceType resource)
    {
        switch (resource)
        {
            case DemocracyResourceType.Freiheit:
                return "Frei";

            case DemocracyResourceType.Wasser:
                return "Wasser";

            case DemocracyResourceType.Gleichberechtigung:
                return "Gleich";

            case DemocracyResourceType.Wahlen:
                return "Wahl";

            default:
                return "?";
        }
    }

    private int GetZoneId(int x, int y)
    {
        int index = y * width + x;
        return zoneIds[index];
    }

    private Color GetZoneColor(int zoneId)
    {
        switch (zoneId)
        {
            case 1:
                return zone1Color;

            case 2:
                return zone2Color;

            case 3:
                return zone3Color;

            case 4:
                return zone4Color;

            default:
                return new Color(1f, 1f, 1f, 0.2f);
        }
    }

    private void SetInfo(string message)
    {
        if (infoText != null)
            infoText.text = message;

        Debug.Log(message);
    }
}