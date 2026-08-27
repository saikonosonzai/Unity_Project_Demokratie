using UnityEngine;
using UnityEngine.Events;

public class BremenSimpleInteractablePrompt : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string promptText = "E  Interagieren";

    [Header("Optional")]
    [SerializeField] private bool hidePromptAfterInteract = true;
    [SerializeField] private bool onlyUseOnce = false;

    [Header("Event bei E")]
    public UnityEvent OnInteract;

    private bool playerInRange;
    private bool alreadyUsed;

    private void Update()
    {
        if (alreadyUsed && onlyUseOnce)
            return;

        if (!playerInRange)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void Interact()
    {
        OnInteract?.Invoke();

        if (hidePromptAfterInteract && BremenInteractionPromptUI.Instance != null)
            BremenInteractionPromptUI.Instance.HidePrompt();

        if (onlyUseOnce)
        {
            alreadyUsed = true;

            Collider col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
        }

        Debug.Log("Interaktion ausgeführt: " + gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyUsed && onlyUseOnce)
            return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (BremenInteractionPromptUI.Instance != null)
                BremenInteractionPromptUI.Instance.ShowPrompt(promptText);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (BremenInteractionPromptUI.Instance != null)
                BremenInteractionPromptUI.Instance.HidePrompt();
        }
    }
}