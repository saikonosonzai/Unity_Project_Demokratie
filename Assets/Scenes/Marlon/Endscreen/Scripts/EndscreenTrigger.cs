using UnityEngine;
using UnityEngine.SceneManagement;

public class EndscreenTrigger : MonoBehaviour
{
    [Header("Endscreen Scene")]
    [SerializeField] private string endscreenSceneName = "Endscreen";

    [Header("Trigger Settings")]
    [SerializeField] private bool onlyPlayerCanTrigger = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Optional")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered;

    private void Reset()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider>();

        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnlyOnce && hasTriggered)
            return;

        if (onlyPlayerCanTrigger && !other.CompareTag(playerTag))
            return;

        hasTriggered = true;

        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(endscreenSceneName);
    }
}