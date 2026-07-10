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

     Die Bereiche sind NICHT mehr quadratisch.
     Alle vier Farbbereiche hängen zusammen.
     Es gibt keine einzelnen Ausreißer.

     Sichtbar von oben nach unten:

     3 3 3 4 4 4
     3 3 3 4 4 4
     1 3 3 4 4 4
     1 1 1 2 2 4
     1 1 2 2 2 2
     1 1 1 2 2 2

     Wichtig:
     In der Mitte gibt es bei x = 2, y = 2 eine 2x2-Stelle
     mit allen vier Farben:

     oben links     = Zone 3
     oben rechts    = Zone 4
     unten links    = Zone 1
     unten rechts   = Zone 2

     Dort passt der Block mit 4x Gleichberechtigung.
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

        FitBoardAndBackground();
        CreatePieceRoot();
        CreateGridSlots();
        CreatePiecesData();
        CreatePiecesVisuals();
        GenerateStartBoard();
        RenderPieces();

        SetInfo("Ziehe die 2x2-Blöcke so, dass jeder Bereich alle vier Werte enthält.");
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

        /*
         Reihenfolge pro 2x2-Block:
         [0] = unten links
         [1] = unten rechts
         [2] = oben links
         [3] = oben rechts

         Eine mögliche Lösung ist:
         Piece 0 -> x 0, y 2
         Piece 1 -> x 2, y 4
         Piece 2 -> x 4, y 1
         Piece 3 -> x 2, y 2

         Piece 3 bleibt der Block mit 4 gleichen Icons.
        */

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