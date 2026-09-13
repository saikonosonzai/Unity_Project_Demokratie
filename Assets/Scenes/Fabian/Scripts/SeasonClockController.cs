using AdventurePuzzleKit;
using UnityEngine;
using System.Collections;

public class SeasonClockController : MonoBehaviour
{
    public bool isActive = false;
    public GameObject player;
    public Renderer clockRenderer;
    public Transform[] pointer;
    public Camera mainCamera;
    public Camera clockCamera;
    public bool[] axis;
    public GameObject puzzleUI;
    public bool puzzleSolved = false;

    // --- UMGESCHRIEBEN AUF FLOAT FÜR PRÄZISE DREHUNGEN ---
    private float[] startPos;
    private Quaternion[] resetPointerRotation;
    private float[] pointerPos;
    private float[] stepPointer;

    void Start()
    {
        // Arrays initialisieren
        pointerPos = new float[pointer.Length];
        stepPointer = new float[pointer.Length];
        startPos = new float[pointer.Length];
        resetPointerRotation = new Quaternion[pointer.Length];

        for (int i = 0; i < pointer.Length; i++)
        {
            pointerPos[i] = 0f;
            startPos[i] = pointerPos[i];
            resetPointerRotation[i] = pointer[i].localRotation;
        }

        if (puzzleUI != null) puzzleUI.SetActive(false);
    }


    public void SetStepSizes(int totalQuestions)
    {
        // Pivot 1 (Index 0) soll insgesamt 360 Grad schaffen
        if (pointer.Length > 0) 
        {
            stepPointer[0] = 360f / totalQuestions;
        }
        
        // Pivot 2 (Index 1) soll insgesamt 180 Grad schaffen
        if (pointer.Length > 1) 
        {
            stepPointer[1] = 180f / totalQuestions;
        }
    }

    public void StartPuzzle()
    {
        if (puzzleSolved) return;

        isActive = true;

        if (puzzleUI != null) puzzleUI.SetActive(true);

        clockCamera.enabled = true;
        mainCamera.enabled = false;

        if (player.GetComponent<AKFPSController>() != null)
        {
            player.GetComponent<AKFPSController>().canMove = false;
            player.GetComponent<AKFPSController>().canRotate = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!isActive || puzzleSolved) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitPuzzle();
            ResetPointers();
        }
    }

    public void movePointers(bool isForward) 
    {
        for (int index = 0; index < pointer.Length; index++)
        {
            movePointer(index, isForward);
        }
    }

    private void movePointer(int index, bool isForward) 
    {
        float currentY = 0f;
        float currentX = 0f;
        float currentZ = 0f;

        if (axis[0]) currentY = stepPointer[index];
        if (axis[1]) currentX = stepPointer[index];
        if (axis[2]) currentZ = stepPointer[index];

        pointerPos[index] = (pointerPos[index] + stepPointer[index]) % 360f;
        
        if (isForward)
        {
            pointer[index].Rotate(currentY, currentX, currentZ);
        }
        else
        {
            pointer[index].Rotate(-currentY, -currentX, -currentZ);
        }
    }

    public void ResetPointers()
    {
        for (int i = 0; i < pointer.Length; i++)
        {
            pointer[i].localRotation = resetPointerRotation[i];
            pointerPos[i] = startPos[i];
        }
    }

    public void ExitPuzzle()
    {
        mainCamera.enabled = true;
        clockCamera.enabled = false;
        
        if (player.GetComponent<AKFPSController>() != null)
        {
            player.GetComponent<AKFPSController>().canMove = true;
            player.GetComponent<AKFPSController>().canRotate = true;
        }

        isActive = false;

        if (puzzleUI != null) puzzleUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}