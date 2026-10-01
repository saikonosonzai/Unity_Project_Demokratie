using UnityEngine;

public class PaintingInteraction : MonoBehaviour
{
    [Header("Referenzen")]
    public ComicManager comicManager;

    [Header("Interaktion")]
    public KeyCode interactionKey = KeyCode.E;
    public string playerTag = "Player";

    [Header("Hinweis")]
    public bool showDebugLogs = true;

    private bool playerInRange = false;

    private void Start()
    {
        if (comicManager == null)
            comicManager = FindFirstObjectByType<ComicManager>();

        if (showDebugLogs)
            Debug.Log("PaintingInteraction gestartet auf: " + gameObject.name);
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Input.GetKeyDown(interactionKey))
        {
            if (comicManager == null)
            {
                Debug.LogWarning("Kein ComicManager eingetragen.");
                return;
            }

            if (showDebugLogs)
                Debug.Log("Bild wurde mit E interagiert: " + gameObject.name);

            comicManager.OpenComic();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = true;

        if (showDebugLogs)
            Debug.Log("Spieler ist im Bild-Trigger: " + gameObject.name);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;

        if (showDebugLogs)
            Debug.Log("Spieler hat den Bild-Trigger verlassen: " + gameObject.name);
    }
}