using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private bool[] collectedKeys = new bool[4];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void CollectKey(int keyNumber)
    {
        if (keyNumber < 1 || keyNumber > 4)
        {
            Debug.LogError("Ungültige Key-Nummer: " + keyNumber);
            return;
        }

        collectedKeys[keyNumber - 1] = true;

        Debug.Log("Key " + keyNumber + " gesammelt.");
    }

    public bool HasKey(int keyNumber)
    {
        if (keyNumber < 1 || keyNumber > 4)
            return false;

        return collectedKeys[keyNumber - 1];
    }
}