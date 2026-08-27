using UnityEngine;

public class BremenDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private BremenDialogueData dialogue;

    [Header("Trigger Settings")]
    [SerializeField] private bool startOnTriggerEnter = true;
    [SerializeField] private bool requireKeyPress = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool onlyOnce = true;
    [SerializeField] private string playerTag = "Player";

    private bool playerInRange;
    private bool alreadyStarted;

    private void Update()
    {
        if (!requireKeyPress)
            return;

        if (!playerInRange)
            return;

        if (alreadyStarted && onlyOnce)
            return;

        if (Input.GetKeyDown(interactKey))
            StartDialogue();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = true;

        if (startOnTriggerEnter)
            StartDialogue();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;
    }

    public void StartDialogue()
    {
        if (alreadyStarted && onlyOnce)
            return;

        if (BremenDialogueManager.Instance == null)
        {
            Debug.LogWarning("Kein BremenDialogueManager in der Szene gefunden.");
            return;
        }

        if (dialogue == null)
        {
            Debug.LogWarning("DialogueData fehlt im Trigger.");
            return;
        }

        alreadyStarted = true;
        BremenDialogueManager.Instance.StartDialogue(dialogue);
    }
}