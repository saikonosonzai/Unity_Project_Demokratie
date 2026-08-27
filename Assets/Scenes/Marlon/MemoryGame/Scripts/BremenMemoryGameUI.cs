using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BremenMemoryGameUI : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite backCardSprite;

    [Header("Begriffkarten")]
    [Tooltip("Hier genau 6 Begriffkarten eintragen.")]
    [SerializeField] private Sprite[] begriffKarten = new Sprite[6];

    [Header("Erklärungskarten")]
    [Tooltip("Hier genau 6 Erklärungskarten eintragen. Die Reihenfolge muss zu den Begriffkarten passen.")]
    [SerializeField] private Sprite[] erklaerungKarten = new Sprite[6];

    [Header("Fullscreen Background")]
    [Tooltip("Optional: Hier kannst du ein rotes Hintergrund-Sprite / eine Textur einfügen.")]
    [SerializeField] private Sprite fullscreenBackgroundSprite;

    [SerializeField] private Color backgroundColor = new Color(0.70f, 0.02f, 0.02f, 1f);

    [Header("Puzzle Verbindung")]
    [SerializeField] private PuzzleInteractable puzzleInteractable;
    [SerializeField] private float closeDelayAfterSolved = 0.8f;

    [Header("Layout")]
    [SerializeField] private Vector2 cardSize = new Vector2(125f, 165f);
    [SerializeField] private float spacingX = 28f;
    [SerializeField] private float spacingY = 34f;
    [SerializeField] private float middleGap = 80f;
    [SerializeField] private Vector2 cardRootOffset = Vector2.zero;

    [Header("Gameplay")]
    [SerializeField] private float wrongPairDelay = 0.75f;

    [Tooltip("Wenn aktiv, werden die Positionen der 12 Karten gemischt.")]
    [SerializeField] private bool shuffleCardPositions = true;

    [Header("Solved Event")]
    public UnityEvent OnPuzzleSolved;

    private RectTransform cardRoot;

    private readonly List<BremenMemoryCardUI> cards = new List<BremenMemoryCardUI>();

    private BremenMemoryCardUI firstSelectedCard;
    private BremenMemoryCardUI secondSelectedCard;

    private bool inputLocked;
    private bool puzzleSolved;
    private int foundPairs;

    private const int totalPairs = 6;
    private const int cardsPerBlock = 6;
    private const int columnsPerBlock = 3;
    private const int rowsPerBlock = 2;

    private void OnEnable()
    {
        StartMemoryGame();
    }

    public void StartMemoryGame()
    {
        StopAllCoroutines();
        CancelInvoke();

        firstSelectedCard = null;
        secondSelectedCard = null;
        inputLocked = false;
        puzzleSolved = false;
        foundPairs = 0;

        SetupCanvas();
        BuildFullscreenBackground();
        CreateCards();
    }

    private void SetupCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;

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
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void BuildFullscreenBackground()
    {
        ClearChildren();

        GameObject backgroundObject = CreateUIObject("FullscreenBackground", transform);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        StretchFull(backgroundRect);

        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.raycastTarget = true;

        if (fullscreenBackgroundSprite != null)
        {
            backgroundImage.sprite = fullscreenBackgroundSprite;
            backgroundImage.color = Color.white;
            backgroundImage.preserveAspect = false;
            backgroundImage.type = Image.Type.Simple;
        }
        else
        {
            backgroundImage.sprite = null;
            backgroundImage.color = backgroundColor;
            backgroundImage.preserveAspect = false;
            backgroundImage.type = Image.Type.Simple;
        }

        GameObject cardRootObject = CreateUIObject("CardRoot", transform);
        cardRoot = cardRootObject.GetComponent<RectTransform>();

        cardRoot.anchorMin = new Vector2(0.5f, 0.5f);
        cardRoot.anchorMax = new Vector2(0.5f, 0.5f);
        cardRoot.pivot = new Vector2(0.5f, 0.5f);
        cardRoot.anchoredPosition = cardRootOffset;

        float blockWidth = columnsPerBlock * cardSize.x + (columnsPerBlock - 1) * spacingX;
        float totalWidth = blockWidth * 2f + middleGap;
        float totalHeight = rowsPerBlock * cardSize.y + (rowsPerBlock - 1) * spacingY;

        cardRoot.sizeDelta = new Vector2(totalWidth, totalHeight);
    }

    private void CreateCards()
    {
        if (backCardSprite == null)
        {
            Debug.LogError("Back Card Sprite fehlt.");
            return;
        }

        if (begriffKarten == null || begriffKarten.Length != totalPairs)
        {
            Debug.LogError("Begriff Karten müssen genau 6 Sprites enthalten.");
            return;
        }

        if (erklaerungKarten == null || erklaerungKarten.Length != totalPairs)
        {
            Debug.LogError("Erklärung Karten müssen genau 6 Sprites enthalten.");
            return;
        }

        List<CardSetupData> setupCards = new List<CardSetupData>();

        for (int i = 0; i < totalPairs; i++)
        {
            if (begriffKarten[i] == null)
            {
                Debug.LogError("Begriff Karte " + i + " fehlt.");
                return;
            }

            if (erklaerungKarten[i] == null)
            {
                Debug.LogError("Erklärung Karte " + i + " fehlt.");
                return;
            }

            setupCards.Add(new CardSetupData(i, MemoryCardType.Begriff, begriffKarten[i]));
            setupCards.Add(new CardSetupData(i, MemoryCardType.Erklaerung, erklaerungKarten[i]));
        }

        if (shuffleCardPositions)
            Shuffle(setupCards);

        cards.Clear();

        for (int i = 0; i < setupCards.Count; i++)
        {
            GameObject cardObject = new GameObject(
                "MemoryCard_" + i,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(BremenMemoryCardUI)
            );

            cardObject.transform.SetParent(cardRoot, false);
            cardObject.transform.SetAsLastSibling();

            RectTransform rect = cardObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = cardSize;
            rect.anchoredPosition = GetCardPosition(i);
            rect.localScale = Vector3.one;

            Image image = cardObject.GetComponent<Image>();
            image.sprite = backCardSprite;
            image.color = Color.white;
            image.preserveAspect = false;
            image.raycastTarget = true;

            BremenMemoryCardUI card = cardObject.GetComponent<BremenMemoryCardUI>();

            card.Init(
                this,
                setupCards[i].pairId,
                setupCards[i].cardType,
                setupCards[i].frontSprite,
                backCardSprite
            );

            cards.Add(card);
        }

        Debug.Log("Memory Karten erstellt: " + cards.Count + " Karten.");
    }

    private Vector2 GetCardPosition(int index)
    {
        int block = index < cardsPerBlock ? 0 : 1;
        int localIndex = index < cardsPerBlock ? index : index - cardsPerBlock;

        int row = localIndex / columnsPerBlock;
        int col = localIndex % columnsPerBlock;

        float blockWidth = columnsPerBlock * cardSize.x + (columnsPerBlock - 1) * spacingX;
        float totalWidth = blockWidth * 2f + middleGap;
        float totalHeight = rowsPerBlock * cardSize.y + (rowsPerBlock - 1) * spacingY;

        float leftStartX = -totalWidth / 2f + cardSize.x / 2f;
        float rightStartX = leftStartX + blockWidth + middleGap;

        float startY = totalHeight / 2f - cardSize.y / 2f;

        float x = block == 0
            ? leftStartX + col * (cardSize.x + spacingX)
            : rightStartX + col * (cardSize.x + spacingX);

        float y = startY - row * (cardSize.y + spacingY);

        return new Vector2(x, y);
    }

    public void SelectCard(BremenMemoryCardUI card)
    {
        if (card == null)
            return;

        if (inputLocked || puzzleSolved)
            return;

        if (card.IsMatched || card.IsFlipped)
            return;

        card.ShowFront();

        if (firstSelectedCard == null)
        {
            firstSelectedCard = card;
            return;
        }

        secondSelectedCard = card;
        StartCoroutine(CheckPair());
    }

    private IEnumerator CheckPair()
    {
        inputLocked = true;

        yield return new WaitForSecondsRealtime(0.2f);

        if (firstSelectedCard != null && secondSelectedCard != null)
        {
            bool samePair = firstSelectedCard.PairId == secondSelectedCard.PairId;
            bool differentType = firstSelectedCard.CardType != secondSelectedCard.CardType;

            if (samePair && differentType)
            {
                firstSelectedCard.SetMatched();
                secondSelectedCard.SetMatched();

                foundPairs++;

                firstSelectedCard = null;
                secondSelectedCard = null;

                inputLocked = false;

                if (foundPairs >= totalPairs)
                    SolvePuzzle();

                yield break;
            }
        }

        yield return new WaitForSecondsRealtime(wrongPairDelay);

        if (firstSelectedCard != null)
            firstSelectedCard.ShowBack();

        if (secondSelectedCard != null)
            secondSelectedCard.ShowBack();

        firstSelectedCard = null;
        secondSelectedCard = null;

        inputLocked = false;
    }

    private void SolvePuzzle()
    {
        puzzleSolved = true;
        inputLocked = true;

        Debug.Log("Memory gelöst!");

        OnPuzzleSolved?.Invoke();

        Invoke(nameof(CloseAfterSolved), closeDelayAfterSolved);
    }

    private void CloseAfterSolved()
    {
        if (puzzleInteractable != null)
        {
            puzzleInteractable.ClosePuzzleAfterSolved();
        }
        else
        {
            gameObject.SetActive(false);
            Debug.LogWarning("PuzzleInteractable ist nicht eingetragen. MemoryCanvas wurde deaktiviert.");
        }
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
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void Shuffle(List<CardSetupData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            CardSetupData temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private class CardSetupData
    {
        public int pairId;
        public MemoryCardType cardType;
        public Sprite frontSprite;

        public CardSetupData(int newPairId, MemoryCardType newCardType, Sprite newFrontSprite)
        {
            pairId = newPairId;
            cardType = newCardType;
            frontSprite = newFrontSprite;
        }
    }
}