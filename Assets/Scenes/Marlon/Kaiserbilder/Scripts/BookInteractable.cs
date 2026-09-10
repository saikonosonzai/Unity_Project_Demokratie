using UnityEngine;

public class BookInteractable : MonoBehaviour
{
    [Header("Referenzen")]
    [SerializeField] private BookUI bookUI;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";

    [Header("Hint")]
    [SerializeField] private string openHintText = "[E] Buch öffnen";

    [Header("Player Controller")]
    [SerializeField] private AdventurePuzzleKit.AKFPSController fpsController;

    [Header("Sound")]
    [SerializeField] private AudioClip openBookSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float openSoundVolume = 1f;

    [Header("Sicherheit")]
    [SerializeField] private float inputCooldown = 0.2f;

    private bool playerInRange;
    private bool bookOpen;
    private float lastInputTime = -999f;

    private void Awake()
    {
        if (bookUI == null)
            bookUI = FindFirstObjectByType<BookUI>();

        if (fpsController == null)
            fpsController = FindFirstObjectByType<AdventurePuzzleKit.AKFPSController>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        if (bookUI != null)
            bookUI.bookInteractable = this;
    }

    private void Start()
    {
        Collider col = GetComponent<Collider>();

        if (col == null)
        {
            Debug.LogError("BOOK TEST: Kein Collider auf diesem Objekt.");
        }
        else
        {
            col.isTrigger = true;
            Debug.Log("BOOK TEST: Collider gefunden. Is Trigger = " + col.isTrigger);
        }

        if (bookUI == null)
            Debug.LogError("BOOK TEST: BookUI fehlt.");
        else
            Debug.Log("BOOK TEST: BookUI gefunden: " + bookUI.name);

        if (fpsController == null)
            Debug.LogWarning("BOOK TEST: FPS Controller fehlt.");

        if (openBookSound == null)
            Debug.LogWarning("BOOK TEST: Open Book Sound ist nicht eingetragen.");

        if (bookUI != null)
            bookUI.HideHint();
    }

    private void Update()
    {
        if (bookOpen)
            return;

        if (!playerInRange)
            return;

        if (Input.GetKeyDown(interactKey) && CanUseInput())
        {
            lastInputTime = Time.unscaledTime;
            OpenBook();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("BOOK TEST: Trigger betreten von: " + other.name + " | Tag: " + other.tag);

        if (!IsPlayer(other))
            return;

        playerInRange = true;

        if (!bookOpen && bookUI != null)
            bookUI.ShowHint(openHintText);

        Debug.Log("BOOK TEST: Player ist im Buch-Trigger.");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("BOOK TEST: Trigger verlassen von: " + other.name + " | Tag: " + other.tag);

        if (!IsPlayer(other))
            return;

        playerInRange = false;

        if (!bookOpen && bookUI != null)
            bookUI.HideHint();

        Debug.Log("BOOK TEST: Player ist nicht mehr im Buch-Trigger.");
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        if (other.CompareTag(playerTag))
            return true;

        AdventurePuzzleKit.AKFPSController controller =
            other.GetComponentInParent<AdventurePuzzleKit.AKFPSController>();

        if (controller != null)
            return true;

        if (fpsController != null)
        {
            if (other.transform == fpsController.transform)
                return true;

            if (other.transform.IsChildOf(fpsController.transform))
                return true;
        }

        return false;
    }

    private bool CanUseInput()
    {
        return Time.unscaledTime - lastInputTime >= inputCooldown;
    }

    private void OpenBook()
    {
        if (bookUI == null)
        {
            Debug.LogError("BOOK TEST: Buch kann nicht geöffnet werden, weil BookUI fehlt.");
            return;
        }

        bookOpen = true;

        PlayOpenBookSound();

        bookUI.HideHint();
        bookUI.Open();

        if (fpsController != null)
            fpsController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("BOOK TEST: Buch wurde geöffnet.");
    }

    private void PlayOpenBookSound()
    {
        if (audioSource == null || openBookSound == null)
            return;

        audioSource.PlayOneShot(openBookSound, openSoundVolume);
    }

    public void CloseBook()
    {
        bookOpen = false;

        if (bookUI != null)
        {
            bookUI.Close();

            if (playerInRange)
                bookUI.ShowHint(openHintText);
            else
                bookUI.HideHint();
        }

        if (fpsController != null)
            fpsController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("BOOK TEST: Buch wurde geschlossen.");
    }
}