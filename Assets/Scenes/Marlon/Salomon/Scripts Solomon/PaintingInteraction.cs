using UnityEngine;

public class PaintingInteraction : MonoBehaviour
{
    [Header("Referenzen")]
    public Camera playerCamera;
    public ComicManager comicManager;

    [Header("Interaktion")]
    public float interactionDistance = 4f;
    public KeyCode interactionKey = KeyCode.E;
    public bool allowMouseClick = true;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        bool pressedInteract = Input.GetKeyDown(interactionKey);
        bool pressedMouse = allowMouseClick && Input.GetMouseButtonDown(0);

        if (!pressedInteract && !pressedMouse)
            return;

        TryInteract();
    }

    private void TryInteract()
    {
        if (playerCamera == null || comicManager == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                comicManager.OpenComic();
            }
        }
    }
}