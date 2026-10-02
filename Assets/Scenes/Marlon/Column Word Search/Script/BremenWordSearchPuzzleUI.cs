using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BremenWordSearchPuzzleUI : MonoBehaviour
{
    [Header("UI")]
    public Transform gridRoot;
    public TMP_Text infoText;
    public TMP_Text foundWordsText;

    [Header("Oberer Text / Titel")]
    public bool showTopText = true;
    public TMP_Text topText;
    [TextArea(1, 3)]
    public string topTextContent = "Finde die Begriffe der Demokratie zwischen den Buchstaben";
    public TMP_FontAsset topTextFont;
    public float topTextFontSize = 34f;
    public Color topTextColor = new Color(0.95f, 0.78f, 0.28f, 1f);
    public Vector2 topTextPosition = new Vector2(0f, 410f);
    public Vector2 topTextSize = new Vector2(900f, 70f);

    [Header("Fonts")]
    public TMP_FontAsset letterFont;
    public TMP_FontAsset wordListFont;
    public TMP_FontAsset infoFont;

    [Header("Schriftgrößen")]
    public float wordListFontSize = 20f;
    public float wordListLineSpacing = 6f;
    public float infoFontSize = 18f;

    [Header("Exit")]
    public PuzzleInteractable puzzleInteractable;
    public float closeDelay = 0.5f;

    [Header("Grid")]
    public int gridSize = 16;
    public Vector2 cellSize = new Vector2(34f, 34f);
    public Vector2 spacing = new Vector2(1f, 1f);
    public float letterFontSize = 19f;

    [Header("Visual Style")]
    public Color normalCellColor = new Color(0.86f, 0.81f, 0.69f, 0.96f);
    public Color selectedCellColor = new Color(0.78f, 0.62f, 0.28f, 1f);
    public Color foundCellColor = new Color(0.40f, 0.70f, 0.45f, 1f);

    public Color cellBorderColor = new Color(0.34f, 0.27f, 0.18f, 0.55f);
    public Color selectedBorderColor = new Color(0.95f, 0.70f, 0.20f, 1f);
    public Color foundBorderColor = new Color(0.65f, 0.95f, 0.55f, 1f);

    public Color textColor = new Color(0.12f, 0.09f, 0.05f, 1f);
    public Color selectedTextColor = new Color(0.08f, 0.05f, 0.02f, 1f);
    public Color foundTextColor = new Color(0.02f, 0.25f, 0.05f, 1f);

    [Header("Word List Style")]
    public Color wordNormalColor = new Color(0.78f, 0.72f, 0.60f, 1f);
    public Color wordFoundColor = new Color(0.55f, 0.95f, 0.55f, 1f);
    public string foundSymbol = "✓ ";

    [Header("Messages")]
    public bool showInfoMessages = true;

    [Header("Solved Event")]
    public UnityEvent OnPuzzleSolved;

    private BremenWordSearchCellUI[,] cells;
    private char[,] letters;

    private readonly List<BremenWordData> words = new List<BremenWordData>();
    private readonly List<BremenWordSearchCellUI> currentSelection = new List<BremenWordSearchCellUI>();

    private bool isSelecting;
    private bool puzzleSolved;

    private void OnEnable()
    {
        StartPuzzle();
    }

    public void StartPuzzle()
    {
        CancelInvoke();

        puzzleSolved = false;
        isSelecting = false;
        currentSelection.Clear();

        CreateTopTextIfNeeded();
        ApplyTextSettings();

        CreateWords();
        CreateFixedLetterGrid();
        CreateGridVisuals();
        RenderFoundWordsText();

        SetInfo("Markiere Wörter waagerecht oder senkrecht.");
    }

    private void CreateTopTextIfNeeded()
    {
        if (!showTopText)
        {
            if (topText != null)
                topText.gameObject.SetActive(false);

            return;
        }

        if (topText != null)
        {
            topText.gameObject.SetActive(true);
            return;
        }

        GameObject textObject = new GameObject("TopText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(transform, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = topTextPosition;
        rect.sizeDelta = topTextSize;

        topText = textObject.GetComponent<TMP_Text>();
        topText.alignment = TextAlignmentOptions.Center;
        topText.raycastTarget = false;
        topText.enableWordWrapping = true;
    }

    private void ApplyTextSettings()
    {
        if (topText != null)
        {
            topText.gameObject.SetActive(showTopText);
            topText.text = topTextContent;
            topText.fontSize = topTextFontSize;
            topText.color = topTextColor;
            topText.alignment = TextAlignmentOptions.Center;

            if (topTextFont != null)
                topText.font = topTextFont;

            RectTransform topRect = topText.GetComponent<RectTransform>();

            if (topRect != null)
            {
                topRect.anchorMin = new Vector2(0.5f, 0.5f);
                topRect.anchorMax = new Vector2(0.5f, 0.5f);
                topRect.pivot = new Vector2(0.5f, 0.5f);
                topRect.anchoredPosition = topTextPosition;
                topRect.sizeDelta = topTextSize;
            }
        }

        if (foundWordsText != null)
        {
            foundWordsText.fontSize = wordListFontSize;
            foundWordsText.lineSpacing = wordListLineSpacing;
            foundWordsText.alignment = TextAlignmentOptions.Left;

            if (wordListFont != null)
                foundWordsText.font = wordListFont;
        }

        if (infoText != null)
        {
            infoText.fontSize = infoFontSize;
            infoText.alignment = TextAlignmentOptions.Center;

            if (infoFont != null)
                infoText.font = infoFont;
        }
    }

    private void CreateWords()
    {
        words.Clear();

        AddWord("FREIHEIT");
        AddWord("AKZEPTANZ");
        AddWord("GEWALTENTEILUNG");
        AddWord("GLEICHHEIT");
        AddWord("MITBESTIMMUNG");
        AddWord("WAHLEN");
        AddWord("GRUNDRECHTE");
        AddWord("AUFKLÄRUNG");
    }

    private void AddWord(string word)
    {
        words.Add(new BremenWordData(word));
    }

    private void CreateFixedLetterGrid()
    {
        gridSize = 16;

        string[] rows =
        {
            "WUDAXIHHEXDVXÜRC",
            "ASNBAAUFKLÄRUNGC",
            "HGHQTARGWUWRNHOS",
            "LGEWALTENTEILUNG",
            "EIZÖAYZFWNKIEGGY",
            "NKDCMDLÖLTIZBFLX",
            "ORDMCRJÄUTÜÖLRES",
            "GWCBVHYJCÖHÖDEIM",
            "IOUÄLFLLGVIWVICU",
            "CTUFAKZEPTANZHHR",
            "XHFOMIUWRHVKÄEHY",
            "YBHÄBZKMICGSÜIEW",
            "KGUPMÜUOEIEHXTIR",
            "GRUNDRECHTERIXTS",
            "NÜSMLHEQPCYBÖDEU",
            "MITBESTIMMUNGFZV"
        };

        letters = new char[gridSize, gridSize];

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                letters[x, y] = rows[y][x];
            }
        }
    }

    private void CreateGridVisuals()
    {
        if (gridRoot == null)
        {
            Debug.LogError("GridRoot fehlt im Inspector.");
            return;
        }

        for (int i = gridRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(gridRoot.GetChild(i).gameObject);
        }

        GridLayoutGroup grid = gridRoot.GetComponent<GridLayoutGroup>();

        if (grid == null)
            grid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();

        grid.cellSize = cellSize;
        grid.spacing = spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = gridSize;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;

        RectTransform gridRect = gridRoot.GetComponent<RectTransform>();

        if (gridRect != null)
        {
            float totalWidth = gridSize * cellSize.x + (gridSize - 1) * spacing.x;
            float totalHeight = gridSize * cellSize.y + (gridSize - 1) * spacing.y;

            gridRect.sizeDelta = new Vector2(totalWidth, totalHeight);
        }

        cells = new BremenWordSearchCellUI[gridSize, gridSize];

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                GameObject cellObject = new GameObject(
                    "Cell_" + x + "_" + y,
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(Outline),
                    typeof(BremenWordSearchCellUI)
                );

                cellObject.transform.SetParent(gridRoot, false);

                BremenWordSearchCellUI cell = cellObject.GetComponent<BremenWordSearchCellUI>();
                cell.Init(this, x, y, letters[x, y]);

                ApplyLetterFontToCell(cellObject);

                cells[x, y] = cell;
            }
        }
    }

    private void ApplyLetterFontToCell(GameObject cellObject)
    {
        if (cellObject == null)
            return;

        TMP_Text letterText = cellObject.GetComponentInChildren<TMP_Text>(true);

        if (letterText == null)
            return;

        letterText.fontSize = letterFontSize;

        if (letterFont != null)
            letterText.font = letterFont;
    }

    public void BeginSelection(BremenWordSearchCellUI cell)
    {
        if (puzzleSolved)
            return;

        isSelecting = true;
        ClearCurrentSelection();

        AddCellToSelection(cell);
    }

    public void ContinueSelection(BremenWordSearchCellUI cell)
    {
        if (!isSelecting || puzzleSolved)
            return;

        if (currentSelection.Count == 0)
        {
            AddCellToSelection(cell);
            return;
        }

        BremenWordSearchCellUI first = currentSelection[0];

        bool sameRow = cell.Y == first.Y;
        bool sameColumn = cell.X == first.X;

        if (!sameRow && !sameColumn)
            return;

        ClearCurrentSelection();

        if (sameRow)
        {
            int minX = Mathf.Min(first.X, cell.X);
            int maxX = Mathf.Max(first.X, cell.X);

            for (int x = minX; x <= maxX; x++)
                AddCellToSelection(cells[x, first.Y]);
        }
        else
        {
            int minY = Mathf.Min(first.Y, cell.Y);
            int maxY = Mathf.Max(first.Y, cell.Y);

            for (int y = minY; y <= maxY; y++)
                AddCellToSelection(cells[first.X, y]);
        }
    }

    public void EndSelection()
    {
        if (!isSelecting || puzzleSolved)
            return;

        isSelecting = false;

        string selectedWord = GetSelectedWord();
        string reversedWord = ReverseString(selectedWord);

        BremenWordData foundWord = null;

        foreach (BremenWordData word in words)
        {
            if (word.found)
                continue;

            if (word.word == selectedWord || word.word == reversedWord)
            {
                foundWord = word;
                break;
            }
        }

        if (foundWord != null)
        {
            foundWord.found = true;
            MarkCurrentSelectionAsFound();
            SetInfo("Gefunden: " + foundWord.word);
            RenderFoundWordsText();
            CheckWinCondition();
        }
        else
        {
            ClearCurrentSelection();

            if (selectedWord.Length > 1)
                SetInfo("Kein gesuchtes Wort.");
            else
                SetInfo("Markiere ein ganzes Wort.");
        }
    }

    private void AddCellToSelection(BremenWordSearchCellUI cell)
    {
        if (cell == null)
            return;

        if (currentSelection.Contains(cell))
            return;

        currentSelection.Add(cell);

        if (!cell.IsFound)
            cell.SetSelected(true);
    }

    private void ClearCurrentSelection()
    {
        foreach (BremenWordSearchCellUI cell in currentSelection)
        {
            if (cell != null && !cell.IsFound)
                cell.SetSelected(false);
        }

        currentSelection.Clear();
    }

    private void MarkCurrentSelectionAsFound()
    {
        foreach (BremenWordSearchCellUI cell in currentSelection)
        {
            if (cell != null)
                cell.SetFound(true);
        }

        currentSelection.Clear();
    }

    private string GetSelectedWord()
    {
        string result = "";

        foreach (BremenWordSearchCellUI cell in currentSelection)
        {
            result += cell.Letter;
        }

        return result;
    }

    private string ReverseString(string input)
    {
        char[] array = input.ToCharArray();
        System.Array.Reverse(array);
        return new string(array);
    }

    private void CheckWinCondition()
    {
        foreach (BremenWordData word in words)
        {
            if (!word.found)
                return;
        }

        puzzleSolved = true;

        SetInfo("Gelöst! Alle Wörter wurden gefunden.");
        Debug.Log("Wortsuchrätsel gelöst.");

        OnPuzzleSolved?.Invoke();

        Invoke(nameof(ClosePuzzleUI), closeDelay);
    }

    private void ClosePuzzleUI()
    {
        if (puzzleInteractable != null)
        {
            puzzleInteractable.ClosePuzzleAfterSolved();
        }
        else
        {
            Debug.LogWarning("PuzzleInteractable ist nicht eingetragen.");
        }
    }

    private void RenderFoundWordsText()
    {
        if (foundWordsText == null)
            return;

        if (wordListFont != null)
            foundWordsText.font = wordListFont;

        foundWordsText.fontSize = wordListFontSize;
        foundWordsText.lineSpacing = wordListLineSpacing;
        foundWordsText.alignment = TextAlignmentOptions.Left;

        string text = "";

        string normalHex = ColorUtility.ToHtmlStringRGB(wordNormalColor);
        string foundHex = ColorUtility.ToHtmlStringRGB(wordFoundColor);

        foreach (BremenWordData word in words)
        {
            if (word.found)
                text += "<color=#" + foundHex + ">" + foundSymbol + word.word + "</color>\n";
            else
                text += "<color=#" + normalHex + ">□ " + word.word + "</color>\n";
        }

        foundWordsText.text = text;
    }

    private void SetInfo(string message)
    {
        if (infoText != null)
        {
            infoText.gameObject.SetActive(showInfoMessages);
            infoText.text = message;
            infoText.fontSize = infoFontSize;
            infoText.alignment = TextAlignmentOptions.Center;

            if (infoFont != null)
                infoText.font = infoFont;
        }

        Debug.Log(message);
    }

    public Color GetNormalCellColor()
    {
        return normalCellColor;
    }

    public Color GetSelectedCellColor()
    {
        return selectedCellColor;
    }

    public Color GetFoundCellColor()
    {
        return foundCellColor;
    }

    public Color GetCellBorderColor()
    {
        return cellBorderColor;
    }

    public Color GetSelectedBorderColor()
    {
        return selectedBorderColor;
    }

    public Color GetFoundBorderColor()
    {
        return foundBorderColor;
    }

    public Color GetTextColor()
    {
        return textColor;
    }

    public Color GetSelectedTextColor()
    {
        return selectedTextColor;
    }

    public Color GetFoundTextColor()
    {
        return foundTextColor;
    }

    public float GetLetterFontSize()
    {
        return letterFontSize;
    }

    public TMP_FontAsset GetLetterFont()
    {
        return letterFont;
    }

    private class BremenWordData
    {
        public string word;
        public bool found;

        public BremenWordData(string newWord)
        {
            word = newWord;
            found = false;
        }
    }
}