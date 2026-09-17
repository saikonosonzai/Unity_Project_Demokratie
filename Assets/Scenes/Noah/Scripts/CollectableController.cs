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

        gameObject.SetActive(false);
    }
    
}
