using AdventurePuzzleKit;
using UnityEngine;
using System.Collections;



public class ClockController : MonoBehaviour
{
    public bool isActive = false;
    public GameObject player;
    public Renderer clockRenderer;
    public Transform[] pointer;
    public Transform door;
    public int[] correctPos;
    public Camera mainCamera;
    public Camera clockCamera;
    
    private int activePointer = 0;
    private int[] pointerPos;
    private bool puzzleSolved = false;
    
    [SerializeField] private AudioSource doorSource;
    [SerializeField] private AudioSource ambientSource;
    

    void Start()
    {
        
        pointerPos = new int[pointer.Length];

        for (int i = 0; i < pointer.Length; i++)
        {
            pointerPos[i] = 0;
        }
    }

    void Update()
    {
        if (!isActive || puzzleSolved) return;

        var akItem = GetComponent<AKItem>();
        akItem.ShowNameHighlight = false;
        akItem.ToggleHighlight(false); // <- das hier sofort ausblenden

        clockCamera.enabled = true;
        mainCamera.enabled = false;
        player.GetComponent<AKFPSController>().canMove = false;
        player.GetComponent<AKFPSController>().canRotate = false;

        int step = 360 / 12; // 30 Grad pro Schritt

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            pointerPos[activePointer] = (pointerPos[activePointer] + step) % 360;
            pointer[activePointer].Rotate(step, 0, 0);
            CheckSolution();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            pointerPos[activePointer] = (pointerPos[activePointer] - step + 360) % 360;
            pointer[activePointer].Rotate(-step, 0, 0);
            CheckSolution();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            activePointer = (activePointer - 1 + pointer.Length) % pointer.Length;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            activePointer = (activePointer + 1) % pointer.Length;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitPuzzle();
        }
    }

    void CheckSolution()
    {
        for (int i = 0; i < pointer.Length; i++)
        {
            Debug.Log($"Zeiger {i}: Position = {pointerPos[i]}, Ziel = {correctPos[i]}");

            if (pointerPos[i] != correctPos[i])
            {
                return;
            }
        }

        // Alle Zeiger korrekt
        Debug.Log("Rätsel gelöst!");
        puzzleSolved = true;
        StartCoroutine(OpenDoor());
        ExitPuzzle();
    }

    void ExitPuzzle()
    {
        var akItem = GetComponent<AKItem>();
        akItem.ShowNameHighlight = true;
        akItem.ToggleHighlight(true); // <- das hier sofort ausblenden
        
        mainCamera.enabled = true;
        clockCamera.enabled = false;
        player.GetComponent<AKFPSController>().canMove = true;
        player.GetComponent<AKFPSController>().canRotate = true;
        isActive = false;
    }

    IEnumerator OpenDoor()
    {
        doorSource.Play();
        ambientSource.Stop();
        Quaternion startRot = door.localRotation;
        Quaternion endRot = Quaternion.Euler(0, 145, 0);

        float t = 0f;
        float duration = 1.5f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            door.localRotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }
    }
}
