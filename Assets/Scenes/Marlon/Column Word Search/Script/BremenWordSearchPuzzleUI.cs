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

        CreateWords();
        CreateFixedLetterGrid();
        CreateGridVisuals();
        RenderFoundWordsText();

        SetInfo("Markiere Wörter waagerecht oder senkrecht.");
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

                cells[x, y] = cell;
            }
        }
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

        foundWordsText.fontSize = 20f;
        foundWordsText.lineSpacing = 6f;
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
            infoText.fontSize = 18f;
            infoText.alignment = TextAlignmentOptions.Center;
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