using UnityEngine;

public class PuzzleboxLidOpener : MonoBehaviour
{
    public Transform lidPivot;

    public float openAngle = -90f;
    public float openSpeed = 30f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;

    private bool isOpen = false;

    void Start()
    {
        closedRotation = lidPivot.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, 0f, openAngle);

        targetRotation = closedRotation;
    }

    void Update()
    {
        lidPivot.localRotation = Quaternion.RotateTowards(
            lidPivot.localRotation,
            targetRotation,
            openSpeed * Time.deltaTime
        );
    }

    public void OpenLid()
    {
        isOpen = true;
        targetRotation = openRotation;
    }
}