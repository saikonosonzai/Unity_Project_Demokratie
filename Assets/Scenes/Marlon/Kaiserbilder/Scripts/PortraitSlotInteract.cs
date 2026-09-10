using UnityEngine;

public class PortraitSlotInteract : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private PortraitAssignmentPuzzle puzzle;
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
                puzzle.OpenSlot(slotIndex);
            else
                Debug.LogWarning("PortraitAssignmentPuzzle fehlt im Inspector.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = true;

        if (interactHint != null)
            interactHint.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;

        if (interactHint != null)
            interactHint.SetActive(false);
    }
}