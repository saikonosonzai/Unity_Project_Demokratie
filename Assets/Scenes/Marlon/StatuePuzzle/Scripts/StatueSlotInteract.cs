using UnityEngine;

public class StatueSlotInteract : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private StatueAssignmentPuzzle puzzle;
    [SerializeField] private int slotIndex = 0;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";

    [Header("Optional Hinweis")]
    [SerializeField] private GameObject interactHint;

    private bool playerInRange;

    private void Start()
    {
        if (interactHint != null)
            interactHint.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            if (puzzle != null)
            {
                Debug.Log("STATUE TEST: Öffne Statue Slot " + slotIndex);
                puzzle.OpenSlot(slotIndex);
            }
            else
            {
                Debug.LogWarning("STATUE TEST: StatueAssignmentPuzzle fehlt im Inspector.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("STATUE TEST: Trigger betreten von " + other.name + " | Tag: " + other.tag);

        if (!other.CompareTag(playerTag))
            return;

        playerInRange = true;

        if (interactHint != null)
            interactHint.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("STATUE TEST: Trigger verlassen von " + other.name + " | Tag: " + other.tag);

        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;

        if (interactHint != null)
            interactHint.SetActive(false);
    }
}