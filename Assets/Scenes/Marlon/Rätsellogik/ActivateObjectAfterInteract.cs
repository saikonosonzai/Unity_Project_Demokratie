using System.Collections;
using UnityEngine;

public class ActivateObjectAfterInteract : MonoBehaviour
{
    [Header("Interaktion")]
    public KeyCode interactKey = KeyCode.E;
    public string playerTag = "Player";

    [Header("Objekt aktivieren")]
    public GameObject objectToActivate;
    public float activationDelay = 2f;

    [Header("Nur einmal benutzen")]
    public bool disableColliderAfterUse = true;

    private bool playerInRange = false;
    private bool hasTriggered = false;
    private Coroutine activationCoroutine;
    private Collider triggerCollider;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();

        if (objectToActivate != null)
            objectToActivate.SetActive(false);
    }

    private void Update()
    {
        if (hasTriggered)
            return;

        if (!playerInRange)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            hasTriggered = true;

            if (disableColliderAfterUse && triggerCollider != null)
                triggerCollider.enabled = false;

            activationCoroutine = StartCoroutine(ActivateAfterDelay());
        }
    }

    private IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        activationCoroutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (!other.CompareTag(playerTag))
            return;

        playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;
    }
}