using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    private HashSet<ItemType> collectedItems = new HashSet<ItemType>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void CollectItem(ItemType item)
    {
        collectedItems.Add(item);

        Debug.Log("Item gesammelt: " + item);
    }

    public bool HasItem(ItemType item)
    {
        return collectedItems.Contains(item);
    }
}