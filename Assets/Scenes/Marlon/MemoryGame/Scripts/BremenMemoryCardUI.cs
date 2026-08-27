using UnityEngine;
using UnityEngine.UI;

public enum MemoryCardType
{
    Begriff,
    Erklaerung
}

public class BremenMemoryCardUI : MonoBehaviour
{
    public int PairId { get; private set; }
    public MemoryCardType CardType { get; private set; }
    public bool IsFlipped { get; private set; }
    public bool IsMatched { get; private set; }

    private BremenMemoryGameUI game;
    private Image cardImage;
    private Button button;

    private Sprite frontSprite;
    private Sprite backSprite;

    public void Init(
        BremenMemoryGameUI owner,
        int pairId,
        MemoryCardType cardType,
        Sprite front,
        Sprite back)
    {
        game = owner;
        PairId = pairId;
        CardType = cardType;
        frontSprite = front;
        backSprite = back;

        IsFlipped = false;
        IsMatched = false;

        cardImage = GetComponent<Image>();
        if (cardImage == null)
            cardImage = gameObject.AddComponent<Image>();

        button = GetComponent<Button>();
        if (button == null)
            button = gameObject.AddComponent<Button>();

        cardImage.sprite = backSprite;
        cardImage.color = Color.white;
        cardImage.preserveAspect = false;
        cardImage.raycastTarget = true;

        button.transition = Selectable.Transition.None;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);

        ShowBack();
    }

    private void OnClicked()
    {
        if (game != null)
            game.SelectCard(this);
    }

    public void ShowFront()
    {
        if (IsMatched)
            return;

        IsFlipped = true;

        if (cardImage != null)
            cardImage.sprite = frontSprite;

        StartCoroutine(PopAnimation());
    }

    public void ShowBack()
    {
        if (IsMatched)
            return;

        IsFlipped = false;

        if (cardImage != null)
            cardImage.sprite = backSprite;
    }

    public void SetMatched()
    {
        IsMatched = true;
        IsFlipped = true;

        if (cardImage != null)
            cardImage.sprite = frontSprite;

        if (button != null)
            button.interactable = false;

        StartCoroutine(MatchedAnimation());
    }

    private System.Collections.IEnumerator PopAnimation()
    {
        Vector3 startScale = Vector3.one;
        Vector3 smallScale = Vector3.one * 0.92f;

        float timer = 0f;
        float duration = 0.08f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(startScale, smallScale, timer / duration);
            yield return null;
        }

        timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(smallScale, Vector3.one, timer / duration);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    private System.Collections.IEnumerator MatchedAnimation()
    {
        Vector3 startScale = Vector3.one;
        Vector3 bigScale = Vector3.one * 1.06f;

        float timer = 0f;
        float duration = 0.1f;

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
}