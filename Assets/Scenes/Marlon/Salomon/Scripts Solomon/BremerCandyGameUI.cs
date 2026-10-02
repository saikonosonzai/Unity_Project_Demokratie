using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BremerCandyGameUI : MonoBehaviour
{
    [Header("Comic")]
    public ComicManager comicManager;

    [Header("UI")]
    public Transform boardRoot;
    public TMP_Text infoText;
    public Button restartButton;
    public Button skipButton;

    [Header("Sprites")]
    public Sprite swordSprite;

    [Tooltip("Reihenfolge: Kerze, Thron, Fisch, Welle, Stadtmusikanten")]
    public Sprite[] normalSprites = new Sprite[5];

    [Header("Spielfeld")]
    public int width = 8;
    public int height = 8;

    [Header("Schwierigkeit")]
    public int startMoves = 30;
    public int difficultyLevel = 1;
    public bool restartOnFail = true;

    [Header("Timing")]
    public float stepDelay = 0.15f;
    public float finishDelay = 1.2f;

    private const int Empty = -1;
    private const int Sword = 99;

    private int[,] board;
    private BremerCandyTileUI[,] tiles;

    private Vector2Int? selectedPosition = null;

    private bool isBusy = false;
    private bool gameWon = false;
    private int movesLeft;

    private void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipGameForTesting);
        }
    }

    public void StartGame()
    {
        StopAllCoroutines();

        selectedPosition = null;
        isBusy = false;
        gameWon = false;

        movesLeft = Mathf.Max(12, startMoves - ((difficultyLevel - 1) * 3));

        CreateTileObjects();
        GenerateBoard();
        RenderAll();

        SetInfo("Bringe das Schwert nach unten! Züge: " + movesLeft);
    }

    public void RestartGame()
    {
        if (isBusy)
            return;

        StartGame();
    }

    public void SkipGameForTesting()
    {
        if (isBusy)
            return;

        gameWon = true;
        selectedPosition = null;

        SetInfo("Minispiel übersprungen.");

        if (comicManager != null)
            comicManager.OnCandyGameFinished();
    }

    private void CreateTileObjects()
    {
        if (boardRoot == null)
        {
            Debug.LogError("BoardRoot fehlt im Inspector.");
            return;
        }

        for (int i = boardRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(boardRoot.GetChild(i).gameObject);
        }

        board = new int[width, height];
        tiles = new BremerCandyTileUI[width, height];

        for (int visualY = height - 1; visualY >= 0; visualY--)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject tileObject = new GameObject("Tile_" + x + "_" + visualY);
                tileObject.transform.SetParent(boardRoot, false);

                Image image = tileObject.AddComponent<Image>();
                image.color = Color.white;
                image.preserveAspect = true;

                Button button = tileObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;

                Outline outline = tileObject.AddComponent<Outline>();
                outline.enabled = false;

                BremerCandyTileUI tile = tileObject.AddComponent<BremerCandyTileUI>();
                tile.Init(this, x, visualY);

                tiles[x, visualY] = tile;
            }
        }
    }

    private void GenerateBoard()
    {
        int swordX = GetMiddleSwordColumn();
        int swordY = height - 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                board[x, y] = GetRandomNormalSymbolWithoutStartMatch(x, y);
            }
        }

        board[swordX, swordY] = Sword;
    }

    private int GetMiddleSwordColumn()
    {
        int middleLeft = Mathf.Max(0, (width / 2) - 1);
        int middleRight = Mathf.Min(width - 1, width / 2);

        return Random.Range(middleLeft, middleRight + 1);
    }

    private int GetRandomNormalSymbolWithoutStartMatch(int x, int y)
    {
        int symbol;
        int safety = 0;

        do
        {
            symbol = Random.Range(0, normalSprites.Length);
            safety++;
        }
        while (WouldCreateStartMatch(x, y, symbol) && safety < 100);

        return symbol;
    }

    private bool WouldCreateStartMatch(int x, int y, int symbol)
    {
        if (x >= 2)
        {
            if (board[x - 1, y] == symbol && board[x - 2, y] == symbol)
                return true;
        }

        if (y >= 2)
        {
            if (board[x, y - 1] == symbol && board[x, y - 2] == symbol)
                return true;
        }

        return false;
    }

    public void SelectTile(int x, int y)
    {
        if (isBusy || gameWon)
            return;

        if (!IsInside(x, y))
            return;

        if (board[x, y] == Sword)
        {
            SetInfo("Das Schwert kann nicht direkt bewegt werden.");
            return;
        }

        Vector2Int clicked = new Vector2Int(x, y);

        if (selectedPosition == null)
        {
            selectedPosition = clicked;
            RenderAll();
            return;
        }

        if (selectedPosition.Value == clicked)
        {
            selectedPosition = null;
            RenderAll();
            return;
        }

        if (AreNeighbors(selectedPosition.Value, clicked))
        {
            StartCoroutine(TrySwap(selectedPosition.Value, clicked));
        }
        else
        {
            selectedPosition = clicked;
            RenderAll();
        }
    }

    private IEnumerator TrySwap(Vector2Int a, Vector2Int b)
    {
        isBusy = true;
        selectedPosition = null;

        if (board[a.x, a.y] == Sword || board[b.x, b.y] == Sword)
        {
            SetInfo("Das Schwert kann nicht direkt bewegt werden.");
            isBusy = false;
            yield break;
        }

        SwapValues(a, b);
        RenderAll();

        yield return new WaitForSeconds(stepDelay);

        List<Vector2Int> matches = FindMatches();

        if (matches.Count == 0)
        {
            SwapValues(a, b);
            RenderAll();

            SetInfo("Kein Treffer. Versuch es nochmal. Züge: " + movesLeft);

            isBusy = false;
            yield break;
        }

        movesLeft--;

        SetInfo("Treffer! Züge: " + movesLeft);

        yield return StartCoroutine(ProcessBoard());

        if (!gameWon)
        {
            if (movesLeft <= 0)
            {
                SetInfo("Keine Züge mehr!");

                if (restartOnFail)
                {
                    yield return new WaitForSeconds(1.2f);
                    StartGame();
                }
            }
            else
            {
                SetInfo("Bringe das Schwert nach unten! Züge: " + movesLeft);
            }
        }

        isBusy = false;
    }

    private IEnumerator ProcessBoard()
    {
        while (true)
        {
            List<Vector2Int> matches = FindMatches();

            if (matches.Count == 0)
                break;

            foreach (Vector2Int pos in matches)
            {
                if (board[pos.x, pos.y] != Sword)
                    board[pos.x, pos.y] = Empty;
            }

            RenderAll();
            yield return new WaitForSeconds(stepDelay);

            DropTiles();

            RenderAll();
            yield return new WaitForSeconds(stepDelay);

            CheckWinCondition();

            if (gameWon)
                yield break;

            FillEmptySpaces();

            RenderAll();
            yield return new WaitForSeconds(stepDelay);
        }

        CheckWinCondition();
    }

    private void DropTiles()
    {
        for (int x = 0; x < width; x++)
        {
            int targetY = 0;

            for (int y = 0; y < height; y++)
            {
                if (board[x, y] != Empty)
                {
                    if (y != targetY)
                    {
                        board[x, targetY] = board[x, y];
                        board[x, y] = Empty;
                    }

                    targetY++;
                }
            }
        }
    }

    private void FillEmptySpaces()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (board[x, y] == Empty)
                {
                    board[x, y] = GetSafeRandomSymbol(x, y);
                }
            }
        }
    }

    private int GetSafeRandomSymbol(int x, int y)
    {
        int symbol;
        int safety = 0;

        do
        {
            symbol = Random.Range(0, normalSprites.Length);
            safety++;
        }
        while (WouldCreateInstantMatch(x, y, symbol) && safety < 100);

        return symbol;
    }

    private bool WouldCreateInstantMatch(int x, int y, int symbol)
    {
        if (x >= 2)
        {
            if (board[x - 1, y] == symbol && board[x - 2, y] == symbol)
                return true;
        }

        if (y >= 2)
        {
            if (board[x, y - 1] == symbol && board[x, y - 2] == symbol)
                return true;
        }

        return false;
    }

    private List<Vector2Int> FindMatches()
    {
        HashSet<Vector2Int> result = new HashSet<Vector2Int>();

        for (int y = 0; y < height; y++)
        {
            int count = 1;

            for (int x = 1; x < width; x++)
            {
                if (IsMatchable(board[x, y]) && board[x, y] == board[x - 1, y])
                {
                    count++;
                }
                else
                {
                    if (count >= 3)
                    {
                        for (int i = 1; i <= count; i++)
                            result.Add(new Vector2Int(x - i, y));
                    }

                    count = 1;
                }
            }

            if (count >= 3)
            {
                for (int i = 1; i <= count; i++)
                    result.Add(new Vector2Int(width - i, y));
            }
        }

        for (int x = 0; x < width; x++)
        {
            int count = 1;

            for (int y = 1; y < height; y++)
            {
                if (IsMatchable(board[x, y]) && board[x, y] == board[x, y - 1])
                {
                    count++;
                }
                else
                {
                    if (count >= 3)
                    {
                        for (int i = 1; i <= count; i++)
                            result.Add(new Vector2Int(x, y - i));
                    }

                    count = 1;
                }
            }

            if (count >= 3)
            {
                for (int i = 1; i <= count; i++)
                    result.Add(new Vector2Int(x, height - i));
            }
        }

        return new List<Vector2Int>(result);
    }

    private bool IsMatchable(int value)
    {
        return value != Empty && value != Sword;
    }

    private void CheckWinCondition()
    {
        for (int x = 0; x < width; x++)
        {
            if (board[x, 0] == Sword)
            {
                gameWon = true;
                SetInfo("Geschafft! Das Schwert ist unten angekommen.");
                StartCoroutine(FinishAfterDelay());
                return;
            }
        }
    }

    private IEnumerator FinishAfterDelay()
    {
        yield return new WaitForSeconds(finishDelay);

        if (comicManager != null)
            comicManager.OnCandyGameFinished();
    }

    private void SwapValues(Vector2Int a, Vector2Int b)
    {
        int temp = board[a.x, a.y];
        board[a.x, a.y] = board[b.x, b.y];
        board[b.x, b.y] = temp;
    }

    private bool AreNeighbors(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        return dx + dy == 1;
    }

    private bool IsInside(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private void RenderAll()
    {
        if (tiles == null || board == null)
            return;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                BremerCandyTileUI tile = tiles[x, y];

                if (tile == null)
                    continue;

                bool selected =
                    selectedPosition.HasValue &&
                    selectedPosition.Value.x == x &&
                    selectedPosition.Value.y == y;

                Sprite sprite = GetSprite(board[x, y]);
                tile.Render(sprite, selected);
            }
        }
    }

    private Sprite GetSprite(int value)
    {
        if (value == Sword)
            return swordSprite;

        if (value >= 0 && value < normalSprites.Length)
            return normalSprites[value];

        return null;
    }

    private void SetInfo(string message)
    {
        if (infoText != null)
            infoText.text = message;
    }
}