using UnityEngine;
using UnityEngine.UI;

public class BremerCandyTileUI : MonoBehaviour
{
    public int x;
    public int y;

    private BremerCandyGameUI game;
    private Image image;
    private Button button;
    private Outline outline;

    public void Init(BremerCandyGameUI owner, int gridX, int gridY)
    {
        game = owner;
        x = gridX;
        y = gridY;

        image = GetComponent<Image>();
        button = GetComponent<Button>();
        outline = GetComponent<Outline>();

        if (image == null)
            image = gameObject.AddComponent<Image>();

        if (button == null)
            button = gameObject.AddComponent<Button>();

        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(4f, -4f);
        outline.enabled = false;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (game != null)
            game.SelectTile(x, y);
    }

    public void Render(Sprite sprite, bool selected)
    {
        if (image == null)
            image = GetComponent<Image>();

        if (button == null)
            button = GetComponent<Button>();

        if (outline == null)
            outline = GetComponent<Outline>();

        if (sprite == null)
        {
            image.sprite = null;
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = false;

            if (button != null)
                button.interactable = false;

            if (outline != null)
                outline.enabled = false;

            transform.localScale = Vector3.one;
            return;
        }

        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;

        if (button != null)
            button.interactable = true;

        if (outline != null)
            outline.enabled = selected;

        transform.localScale = selected ? Vector3.one * 1.08f : Vector3.one;
    }
}