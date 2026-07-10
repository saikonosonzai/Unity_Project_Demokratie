using System.Collections;
using AdventurePuzzleKit;
using UnityEngine;

public class Hatch_Controller : MonoBehaviour
{

    public bool isActive = false;
    public Camera mainCam;
    public Camera puzzleBoxCam;
    public GameObject player;
    public Renderer hatchRenderer;
    public Transform drawer;
    public Transform disc1;
    public Transform disc2;
    public Transform disc3;
    public Transform disc4;
    public int[] correctDisc1Pos = new int[4];
    public GameObject button;

    private int[] discPositions = {0,0,0,0};
    private int selectedDisc = 0;
    private Transform selectedDiscTransform;
    private Transform[] discs;
    private int[] toMove;
    private int[][] moveMap;

    void Start()
    {
        discs = new[] { disc1, disc2, disc3, disc4 };

        moveMap = new int[][]
        {
            new[] {0},        // 1 → nur 1
            new[] {1},        // 2 → nur 2
            new[] {0,1,2},    // 3 → 1,2,3
            new[] {1,3}       // 4 → 2,4
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            int step = 30;
            
            if (mainCam.enabled)
            {
                mainCam.enabled = false;
                puzzleBoxCam.enabled = true;
            }
            player.GetComponent<AKFPSController>().canMove = false;
            player.GetComponent<AKFPSController>().canRotate = false;
            
            toMove = moveMap[selectedDisc];
            
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (selectedDisc < 3)
                {
                    selectedDisc++;
                }
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (selectedDisc > 0)
                {
                    selectedDisc--;
                }
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                foreach (var i in toMove)
                {
                    discPositions[i] = (discPositions[i] + step) % 360;
                    discs[i].localEulerAngles = new Vector3(0, 0, discPositions[i]);
                }
                
                CorrectTransform();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                foreach (var i in toMove)
                {
                    discPositions[i] = (discPositions[i] - step + 360) % 360;
                    discs[i].localEulerAngles = new Vector3(0, 0, discPositions[i]);
                }

                CorrectTransform();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                ExitCameraView();
            }
        }
    }

    void ExitCameraView()
    {
        puzzleBoxCam.enabled = false;
        mainCam.enabled = true;
        player.GetComponent<AKFPSController>().canMove = true;
        player.GetComponent<AKFPSController>().canRotate = true;
        isActive = false;
    }

    IEnumerator OpenHatch()
    {
        Quaternion startRot = transform.localRotation;
        Quaternion endRot = Quaternion.Euler(90, 0,0);

        float t = 0f;
        float duration = 1.5f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        transform.localRotation = endRot;
    }
    
    IEnumerator OpenDrawer()
    {
        Vector3 startPos = drawer.localPosition;
        Vector3 endPos = new Vector3(0, drawer.localPosition.y, -0.204f);

        float t = 0f;
        float duration = 1.5f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            drawer.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        drawer.localPosition = endPos;
    }
    
    void CorrectTransform()
    {
        for (int i = 0; i < discPositions.Length; i++)
        {
            if (discPositions[i] != correctDisc1Pos[i])
            {
                Debug.Log($"Disc {i}: {discPositions[i]} vs {correctDisc1Pos[i]}");
                return;
            }
        }
        print("True");
        StartCoroutine(OpenDrawer());
    }
}
