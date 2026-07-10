using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BremenWordSearchCellUI : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public char Letter { get; private set; }
    public bool IsFound { get; private set; }

    private BremenWordSearchPuzzleUI game;
    private Image backgroundImage;
    private TMP_Text letterText;
    private Outline outline;

    public void Init(BremenWordSearchPuzzleUI owner, int x, int y, char letter)
    {
        game = owner;
        X = x;
        Y = y;
        Letter = letter;
        IsFound = false;

        backgroundImage = GetComponent<Image>();

        if (backgroundImage == null)
            backgroundImage = gameObject.AddComponent<Image>();

        backgroundImage.color = game.GetNormalCellColor();
        backgroundImage.raycastTarget = true;

        outline = GetComponent<Outline>();

        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectColor = game.GetCellBorderColor();
        outline.effectDistance = new Vector2(1f, -1f);

        CreateLetterText();

        SetSelected(false);
    }

    private void CreateLetterText()
    {
        GameObject textObject = new GameObject("Letter", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(transform, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        letterText = textObject.GetComponent<TMP_Text>();
        letterText.text = Letter.ToString();
        letterText.alignment = TextAlignmentOptions.Center;
        letterText.fontSize = game.GetLetterFontSize();
        letterText.fontStyle = FontStyles.Bold;
        letterText.color = game.GetTextColor();
        letterText.raycastTarget = false;
        letterText.enableWordWrapping = false;

        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.18f);
        shadow.effectDistance = new Vector2(1f, -1f);
    }

    public void SetSelected(bool selected)
    {
        if (IsFound)
            return;

        if (backgroundImage == null)
            return;

        backgroundImage.color = selected ? game.GetSelectedCellColor() : game.GetNormalCellColor();

        if (letterText != null)
            letterText.color = selected ? game.GetSelectedTextColor() : game.GetTextColor();

        if (outline != null)
            outline.effectColor = selected ? game.GetSelectedBorderColor() : game.GetCellBorderColor();
    }

    public void SetFound(bool found)
    {
        IsFound = found;

        if (backgroundImage != null)
            backgroundImage.color = game.GetFoundCellColor();

        if (letterText != null)
            letterText.color = game.GetFoundTextColor();

        if (outline != null)
            outline.effectColor = game.GetFoundBorderColor();

        StartCoroutine(GlowEffect());
    }

    private System.Collections.IEnumerator GlowEffect()
    {
        Vector3 startScale = transform.localScale;
        Vector3 bigScale = Vector3.one * 1.10f;

        float timer = 0f;
        float duration = 0.12f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(startScale, bigScale, timer / duration);
            yield return null;
        }

        timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(bigScale, Vector3.one, timer / duration);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (game != null)
            game.BeginSelection(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0))
        {
            if (game != null)
                game.ContinueSelection(this);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (game != null)
            game.EndSelection();
    }
}