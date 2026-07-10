using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BremenDemocracyPieceUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public int PieceId { get; private set; }

    private BremenDemocracyBlockPuzzleUI game;
    private DemocracyResourceType[] resources;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 oldAnchoredPosition;
    private Vector2Int currentGridPosition;

    private const float IconSize = 48f;

    // Außenrahmen
    private const float OuterFrameThickness = 7f;

    // Innenlinien zwischen den 4 Feldern
    private const float InnerDividerThickness = 4f;

    // Outline / Schatten für Holzlook
    private const float FrameOutlineThickness = 1.5f;

    public void Init(BremenDemocracyBlockPuzzleUI owner, int pieceId, DemocracyResourceType[] pieceResources)
    {
        game = owner;
        PieceId = pieceId;
        resources = pieceResources;

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        SetupPieceBackground();
        CreateVisuals();
    }

    private void SetupPieceBackground()
    {
        Image rootImage = GetComponent<Image>();

        if (rootImage != null)
        {
            // Sehr dezente Fläche, damit der Block als zusammenhängendes Teil lesbar bleibt.
            // Nicht zu stark, damit die Bremen-Karte im Hintergrund sichtbar bleibt.
            rootImage.color = new Color(1f, 1f, 1f, 0.02f);
            rootImage.raycastTarget = true;
        }
    }

    private void CreateVisuals()
    {
        // Die vier Felder selbst
        CreateCell("BottomLeft", 0, 0, resources[0]);
        CreateCell("BottomRight", 1, 0, resources[1]);
        CreateCell("TopLeft", 0, 1, resources[2]);
        CreateCell("TopRight", 1, 1, resources[3]);

        // Gemeinsamer Außenrahmen
        CreateOuterWoodFrame(transform);

        // Innenlinien, damit man die 4 Teilfelder sauber erkennt
        CreateInnerDividers(transform);
    }

    private void CreateCell(string name, int localX, int localY, DemocracyResourceType resource)
    {
        GameObject cellObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        cellObject.transform.SetParent(transform, false);

        RectTransform cellRect = cellObject.GetComponent<RectTransform>();
        cellRect.sizeDelta = game.cellSize;
        cellRect.anchoredPosition = game.GetCellLocalPositionInPiece(localX, localY);

        Image cellImage = cellObject.GetComponent<Image>();

        // Komplett clean: keine sichtbaren Einzelhintergründe pro Feld
        cellImage.color = new Color(1f, 1f, 1f, 0f);
        cellImage.raycastTarget = false;

        CreateIconOrText(cellObject.transform, resource);
    }

    private void CreateOuterWoodFrame(Transform parent)
    {
        CreateFrameBar(
            parent,
            "OuterTopFrame",
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, OuterFrameThickness)
        );

        CreateFrameBar(
            parent,
            "OuterBottomFrame",
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, OuterFrameThickness)
        );

        CreateFrameBar(
            parent,
            "OuterLeftFrame",
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0.5f),
            new Vector2(OuterFrameThickness, 0f)
        );

        CreateFrameBar(
            parent,
            "OuterRightFrame",
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0.5f),
            new Vector2(OuterFrameThickness, 0f)
        );
    }

    private void CreateInnerDividers(Transform parent)
    {
        // Vertikale Innenlinie
        CreateFrameBar(
            parent,
            "InnerVerticalDivider",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 0.5f),
            new Vector2(InnerDividerThickness, 0f)
        );

        // Horizontale Innenlinie
        CreateFrameBar(
            parent,
            "InnerHorizontalDivider",
            new Vector2(0f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, InnerDividerThickness)
        );
    }

    private void CreateFrameBar(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 sizeDelta)
    {
        GameObject barObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));
        barObject.transform.SetParent(parent, false);

        RectTransform rect = barObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = Vector2.zero;

        Image image = barObject.GetComponent<Image>();
        image.raycastTarget = false;

        // Optional: Rahmen-Textur verwenden, wenn vorhanden
        if (game.frameTextureSprite != null)
        {
            image.sprite = game.frameTextureSprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.preserveAspect = false;
        }
        else
        {
            image.sprite = null;
            image.color = game.woodColor;
        }

        Outline outline = barObject.GetComponent<Outline>();
        outline.effectColor = game.darkWoodColor;
        outline.effectDistance = new Vector2(FrameOutlineThickness, -FrameOutlineThickness);
    }

    private void CreateIconOrText(Transform parent, DemocracyResourceType resource)
    {
        Sprite sprite = game.GetSprite(resource);

        if (sprite != null)
        {
            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(parent, false);

            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(IconSize, IconSize);
            iconRect.anchoredPosition = Vector2.zero;

            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = sprite;

            // Damit dein PNG genauso aussieht wie gespeichert
            icon.color = Color.white;

            icon.preserveAspect = true;
            icon.raycastTarget = false;
        }
        else
        {
            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(2f, 2f);
            textRect.offsetMax = new Vector2(-2f, -2f);

            Text text = textObject.GetComponent<Text>();
            text.text = game.GetShortLabel(resource);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.fontSize = 11;
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }

    public void Render()
    {
        transform.localScale = Vector3.one;
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        currentGridPosition = gridPosition;
    }

    public void SnapTo(Vector2 anchoredPosition)
    {
        rectTransform.anchoredPosition = anchoredPosition;
        oldAnchoredPosition = anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.SetAsLastSibling();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        oldAnchoredPosition = rectTransform.anchoredPosition;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        transform.SetAsLastSibling();
        transform.localScale = Vector3.one * 1.04f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null)
            return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        transform.localScale = Vector3.one;

        bool moved = game.TryMovePiece(this, rectTransform.anchoredPosition);

        if (!moved)
        {
            rectTransform.anchoredPosition = oldAnchoredPosition;
        }
        else
        {
            oldAnchoredPosition = rectTransform.anchoredPosition;
        }
    }
}