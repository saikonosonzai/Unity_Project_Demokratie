using UnityEngine;

public class PuzzleInteractable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Puzzle UI")]
    [SerializeField] private GameObject puzzleCanvas;

    [Header("Extra Puzzle UI Objects")]
    [SerializeField] private GameObject[] extraPuzzleObjectsToHideOnClose;

    [Header("Player / Camera Objects wieder aktivieren")]
    [SerializeField] private GameObject[] objectsToEnableOnClose;

    [Header("Scripts deaktivieren, solange Puzzle offen ist")]
    [SerializeField] private MonoBehaviour[] scriptsToDisableWhilePuzzleOpen;

    [Header("Camera Reset")]
    [SerializeField] private GameObject mainCameraObject;
    [SerializeField] private GameObject examineCameraObject;
    [SerializeField] private bool forceMainCameraOnClose = true;

    [Header("Fixed Puzzle Camera View")]
    [SerializeField] private bool useFixedPuzzleCameraView = false;
    [SerializeField] private Transform puzzleCameraPoint;
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private bool restoreCameraTransformOnClose = true;

    [Header("Blur / Overlay Objects")]
    [SerializeField] private GameObject[] blurObjectsToDisableOnClose;

    [Header("Blur / Post Processing Components")]
    [SerializeField] private Behaviour[] blurComponentsToDisableOnClose;

    [Header("Reward / Box Opening")]
    [SerializeField] private Animator boxAnimator;
    [SerializeField] private string openAnimationTrigger = "Open";
    [SerializeField] private GameObject rewardObject;
    [SerializeField] private bool showRewardAfterSolved = true;

    [Header("Lid Opening After Solved")]
    [SerializeField] private Transform lidPivot;
    [SerializeField] private float lidOpenAngle = -90f;
    [SerializeField] private float lidOpenSpeed = 30f;

    [Header("Objects To Enable After Solved")]
    [SerializeField] private GameObject[] objectsToEnableWithKey9;

    [Header("After Solved")]
    [SerializeField] private bool disableInteractionAfterSolved = true;

    private bool playerInRange;
    private bool puzzleOpen;
    private bool puzzleSolved;

    private Quaternion lidClosedRotation;
    private Quaternion lidOpenRotation;
    private Quaternion lidTargetRotation;
    private bool lidSetupDone;

    private Vector3 savedMainCameraPosition;
    private Quaternion savedMainCameraRotation;
    private Transform savedMainCameraParent;
    private bool cameraStateSaved;

    private void Start()
    {
        if (puzzleCanvas != null)
        {
            puzzleCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("PuzzleCanvas ist nicht eingetragen!");
        }

        HideExtraPuzzleObjects();

        if (rewardObject != null)
            rewardObject.SetActive(false);

        DisableSolvedObjects();
        SetupLid();
    }

    private void Update()
    {
        UpdateLidOpening();

        if (puzzleSolved)
            return;

        if (!playerInRange)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            if (puzzleOpen)
                ClosePuzzle();
            else
                OpenPuzzle();
        }
    }

    private void SetupLid()
    {
        if (lidPivot == null)
        {
            Debug.LogWarning("Lid Pivot ist nicht eingetragen.");
            return;
        }

        lidClosedRotation = lidPivot.localRotation;
        lidOpenRotation = lidClosedRotation * Quaternion.Euler(0f, 0f, lidOpenAngle);
        lidTargetRotation = lidClosedRotation;
        lidSetupDone = true;

        Debug.Log("Lid Setup fertig: " + lidPivot.name);
    }

    private void OpenLidAfterSolved()
    {
        if (lidPivot == null)
        {
            Debug.LogWarning("Klappe kann nicht geöffnet werden: Lid Pivot fehlt.");
            return;
        }

        if (!lidSetupDone)
            SetupLid();

        lidTargetRotation = lidOpenRotation;

        Debug.Log("Klappe wird geöffnet.");
    }

    private void UpdateLidOpening()
    {
        if (lidPivot == null || !lidSetupDone)
            return;

        lidPivot.localRotation = Quaternion.RotateTowards(
            lidPivot.localRotation,
            lidTargetRotation,
            lidOpenSpeed * Time.deltaTime
        );
    }

    private void DisableSolvedObjects()
    {
        if (objectsToEnableWithKey9 == null)
            return;

        foreach (GameObject obj in objectsToEnableWithKey9)
        {
            if (obj == null)
                continue;

            obj.SetActive(false);
            Debug.Log("Solved-Objekt am Anfang deaktiviert: " + obj.name);
        }
    }

    private void EnableSolvedObjects()
    {
        if (objectsToEnableWithKey9 == null)
            return;

        foreach (GameObject obj in objectsToEnableWithKey9)
        {
            if (obj == null)
                continue;

            obj.SetActive(true);
            Debug.Log("Solved-Objekt aktiviert: " + obj.name);
        }
    }

    private void OpenPuzzle()
    {
        puzzleOpen = true;

        SaveCameraTransform();
        ApplyFixedPuzzleCameraView();

        if (puzzleCanvas != null)
            puzzleCanvas.SetActive(true);

        ShowExtraPuzzleObjects();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        DisablePlayerAndCameraScripts();

        Debug.Log("Puzzle geöffnet.");
    }

    private void ClosePuzzle()
    {
        puzzleOpen = false;

        HidePuzzleUI();
        ResetAfterPuzzle();

        Debug.Log("Puzzle geschlossen.");
    }

    public void ClosePuzzleAfterSolved()
    {
        puzzleSolved = true;
        puzzleOpen = false;
        playerInRange = false;

        HidePuzzleUI();
        ResetAfterPuzzle();

        OpenLidAfterSolved();
        EnableSolvedObjects();

        OpenBoxAndShowReward();

        Debug.Log("Puzzle gelöst, UI geschlossen, Klappe geöffnet und Objekte aktiviert.");

        if (disableInteractionAfterSolved)
        {
            Collider triggerCollider = GetComponent<Collider>();

            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
                Debug.Log("Interaction Collider deaktiviert.");
            }
        }
    }

    private void HidePuzzleUI()
    {
        if (puzzleCanvas != null)
        {
            puzzleCanvas.SetActive(false);
            Debug.Log("PuzzleCanvas deaktiviert: " + puzzleCanvas.name);
        }

        HideExtraPuzzleObjects();
    }

    private void ResetAfterPuzzle()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        EnableImportantObjects();
        ResetCameraState();
        RestoreCameraTransform();

        EnablePlayerAndCameraScripts();

        DisableBlurObjects();
        DisableBlurComponents();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("Puzzle Reset ausgeführt. Player/Kamera sollten wieder aktiv sein.");
    }

    private void SaveCameraTransform()
    {
        if (!useFixedPuzzleCameraView)
            return;

        if (mainCameraObject == null)
        {
            Debug.LogWarning("Fixed Puzzle Camera View ist aktiv, aber Main Camera Object ist nicht eingetragen.");
            return;
        }

        savedMainCameraParent = mainCameraObject.transform.parent;
        savedMainCameraPosition = mainCameraObject.transform.position;
        savedMainCameraRotation = mainCameraObject.transform.rotation;
        cameraStateSaved = true;
    }

    private void ApplyFixedPuzzleCameraView()
    {
        if (!useFixedPuzzleCameraView)
            return;

        if (mainCameraObject == null)
        {
            Debug.LogWarning("Main Camera Object fehlt. Feste Puzzle-Kamera kann nicht gesetzt werden.");
            return;
        }

        if (puzzleCameraPoint == null)
        {
            Debug.LogWarning("Puzzle Camera Point fehlt. Feste Puzzle-Kamera kann nicht gesetzt werden.");
            return;
        }

        if (forceMainCameraOnClose)
        {
            mainCameraObject.SetActive(true);

            if (examineCameraObject != null)
                examineCameraObject.SetActive(false);
        }

        mainCameraObject.transform.SetParent(null);
        mainCameraObject.transform.position = puzzleCameraPoint.position;
        mainCameraObject.transform.rotation = puzzleCameraPoint.rotation;

        if (lookAtTarget != null)
            mainCameraObject.transform.LookAt(lookAtTarget.position);

        Debug.Log("Feste Puzzle-Kameraansicht aktiviert.");
    }

    private void RestoreCameraTransform()
    {
        if (!useFixedPuzzleCameraView)
            return;

        if (!restoreCameraTransformOnClose)
            return;

        if (!cameraStateSaved)
            return;

        if (mainCameraObject == null)
            return;

        mainCameraObject.transform.SetParent(savedMainCameraParent);
        mainCameraObject.transform.position = savedMainCameraPosition;
        mainCameraObject.transform.rotation = savedMainCameraRotation;

        cameraStateSaved = false;

        Debug.Log("Kamera-Transform wiederhergestellt.");
    }

    private void DisablePlayerAndCameraScripts()
    {
        if (scriptsToDisableWhilePuzzleOpen == null)
            return;

        foreach (MonoBehaviour script in scriptsToDisableWhilePuzzleOpen)
        {
            if (script == null)
                continue;

            script.enabled = false;
            Debug.Log("Deaktiviert während Puzzle: " + script.GetType().Name + " auf " + script.gameObject.name);
        }
    }

    private void EnablePlayerAndCameraScripts()
    {
        if (scriptsToDisableWhilePuzzleOpen == null)
            return;

        foreach (MonoBehaviour script in scriptsToDisableWhilePuzzleOpen)
        {
            if (script == null)
                continue;

            script.enabled = true;
            Debug.Log("Wieder aktiviert nach Puzzle: " + script.GetType().Name + " auf " + script.gameObject.name);
        }
    }

    private void EnableImportantObjects()
    {
        if (objectsToEnableOnClose == null)
            return;

        foreach (GameObject obj in objectsToEnableOnClose)
        {
            if (obj == null)
                continue;

            obj.SetActive(true);
            Debug.Log("Wieder aktiviert: " + obj.name);
        }
    }

    private void ResetCameraState()
    {
        if (!forceMainCameraOnClose)
            return;

        if (mainCameraObject != null)
        {
            mainCameraObject.SetActive(true);
            Debug.Log("Main Camera aktiviert: " + mainCameraObject.name);
        }

        if (examineCameraObject != null)
        {
            examineCameraObject.SetActive(false);
            Debug.Log("Examine Camera deaktiviert: " + examineCameraObject.name);
        }
    }

    private void ShowExtraPuzzleObjects()
    {
        if (extraPuzzleObjectsToHideOnClose == null)
            return;

        foreach (GameObject obj in extraPuzzleObjectsToHideOnClose)
        {
            if (obj == null)
                continue;

            obj.SetActive(true);
        }
    }

    private void HideExtraPuzzleObjects()
    {
        if (extraPuzzleObjectsToHideOnClose == null)
            return;

        foreach (GameObject obj in extraPuzzleObjectsToHideOnClose)
        {
            if (obj == null)
                continue;

            obj.SetActive(false);
            Debug.Log("Extra Puzzle Objekt deaktiviert: " + obj.name);
        }
    }

    private void DisableBlurObjects()
    {
        if (blurObjectsToDisableOnClose == null)
            return;

        foreach (GameObject obj in blurObjectsToDisableOnClose)
        {
            if (obj == null)
                continue;

            obj.SetActive(false);
            Debug.Log("Blur Objekt deaktiviert: " + obj.name);
        }
    }

    private void DisableBlurComponents()
    {
        if (blurComponentsToDisableOnClose == null)
            return;

        foreach (Behaviour component in blurComponentsToDisableOnClose)
        {
            if (component == null)
                continue;

            component.enabled = false;
            Debug.Log("Blur Komponente deaktiviert: " + component.name);
        }
    }

    private void OpenBoxAndShowReward()
    {
        if (boxAnimator != null)
        {
            boxAnimator.SetTrigger(openAnimationTrigger);
            Debug.Log("Box Animation gestartet: " + openAnimationTrigger);
        }

        if (showRewardAfterSolved && rewardObject != null)
        {
            rewardObject.SetActive(true);
            Debug.Log("Reward Object aktiviert: " + rewardObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (puzzleSolved)
            return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Drücke E, um das Puzzle zu öffnen.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (puzzleOpen)
                ClosePuzzle();
        }
    }
}