using UnityEngine;

public class CollectableController : MonoBehaviour

{
    
    [SerializeField] private ItemType itemType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Collect()
    {
        ItemManager.Instance.CollectItem(itemType);

        if (itemType == ItemType.Bilder)
        {
           GameStateManager.Instance.finishedClockPuzzle();
        }

        if (itemType == ItemType.BremenKey)
        {
            GameStateManager.Instance.activateChestCollision();
        }
        
        if (itemType == ItemType.Compass)
        {
            GameStateManager.Instance.finishedHallwayPuzzle();
        }

        gameObject.SetActive(false);
    }
    
}
