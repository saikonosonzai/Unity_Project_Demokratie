using UnityEngine;

public class LockedInventoryObjectInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Unlock")]
    [SerializeField] private bool unlocked = false;

    [Header("Inventory Check")]
    [Tooltip("Das Objekt, das der Spieler im Inventar haben muss. Kann z.B. ein UI-Icon oder ein GameObject im Inventory sein.")]
    [SerializeField] private GameObject requiredInventoryObject;

    [Header("Result")]
    [Tooltip("Dieses Objekt erscheint, wenn der Spieler E drückt und das benötigte Inventar-Objekt besitzt.")]
    [SerializeField] private GameObject objectToEnableAfterUse;

    [Tooltip("Soll das benötigte Inventar-Objekt verschwinden?")]
    [SerializeField] private bool removeRequiredObjectAfterUse = true;

    [Tooltip("Soll dieses Interaktionsobjekt danach deaktiviert werden?")]
    [SerializeField] private bool disableThisAfterUse = true;

    [Header("Optional Message")]
    [SerializeField] private bool showDebugMessages = true;

    private bool playerInRange;
    private bool alreadyUsed;

    private void Start()
    {
        if (objectToEnableAfterUse != null)
            objectToEnableAfterUse.SetActive(false);
    }

    private void Update()
    {
        if (alreadyUsed)
            return;

        if (!playerInRange)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            TryUseObject();
        }
    }

    private void TryUseObject()
    {
        if (!unlocked)
        {
            if (showDebugMessages)
                Debug.Log("Diese Funktion ist noch gesperrt. Erst das andere Spiel abschließen.");

            return;
        }

        if (requiredInventoryObject == null)
        {
            Debug.LogWarning("Required Inventory Object ist nicht eingetragen.");
            return;
        }

        if (!requiredInventoryObject.activeInHierarchy)
        {
            if (showDebugMessages)
                Debug.Log("Du hast das benötigte Objekt nicht im Inventar.");

            return;
        }

        UseObject();
    }

    private void UseObject()
    {
        alreadyUsed = true;

        if (removeRequiredObjectAfterUse && requiredInventoryObject != null)
            requiredInventoryObject.SetActive(false);

        if (objectToEnableAfterUse != null)
            objectToEnableAfterUse.SetActive(true);

        if (showDebugMessages)
            Debug.Log("Inventar-Objekt benutzt. Neues Objekt wurde aktiviert.");

        if (disableThisAfterUse)
        {
            Collider col = GetComponent<Collider>();

            if (col != null)
                col.enabled = false;
        }
    }

    public void UnlockInteraction()
    {
        unlocked = true;

        if (showDebugMessages)
            Debug.Log("Interaktion wurde freigeschaltet.");
    }

    public void LockInteraction()
    {
        unlocked = false;

        if (showDebugMessages)
            Debug.Log("Interaktion wurde gesperrt.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (showDebugMessages)
            {
                if (unlocked)
                    Debug.Log("Drücke E, um das Objekt zu benutzen.");
                else
                    Debug.Log("Dieses Objekt ist noch gesperrt.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}