using System;
using AdventurePuzzleKit;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public GameObject[] Doors;
    public GameObject[] PicturePuzzle;
    public GameObject HallwayChest;
    public GameObject[] statues;
    
    
    public static GameStateManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void finishedClockPuzzle()
    {
        foreach (var puzzle in PicturePuzzle)
        {
            puzzle.GetComponent<PictureSlotInteract>().interactable = true;
        }

    }

    public void finishedBilderPuzzle()
    {
        Doors[0].GetComponent<DoorController>().OpenDoor();

    }
    
    
    public void finishedKameraPuzzle()
    {
       Doors[1].GetComponent<DoorController>().OpenDoor();
        
    }

    public void activateChestCollision()
    {
        HallwayChest.GetComponent<BoxCollider>().enabled = true;
    }
    
    public void finishedHallwayPuzzle()
    {
        Doors[2].GetComponent<DoorController>().OpenDoor();
        Doors[3].GetComponent<DoorController>().OpenDoor();
        
    }
    
    public void startStatuePuzzle()
    {
        foreach (var puzzle in statues)
        {
            puzzle.transform.Find("Trigger").gameObject.SetActive(true);
            puzzle.transform.Find("Symbol").gameObject.SetActive(true);
        }
    }
    
    public void finishedStatuePuzzle()
    {
        Doors[4].GetComponent<DoorController>().OpenDoor();
        Doors[5].GetComponent<DoorController>().OpenDoor();
        
    }
    
    public void finishedSenaatssaalPuzzle()
    {
        //Tür zum Kaminsaal geht auf
        
    }
    
    public void finishedKaminsaalPuzzle()
    {
        //Tür zum Goblinzimmer geht auf
        
    }
    
    public void finishedGoblinzimmer()
    {
        //tür zum Ausgang geht auf
        
    }
}
