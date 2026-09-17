using UnityEngine;

public class CameraController : MonoBehaviour
{
    void Update()
    {
        Debug.Log(
            "Key 1: " + GameStateManager.Instance.HasKey(1) +
            " | Camera: " + ItemManager.Instance.HasItem(ItemType.Cam)
        );

        if (GameStateManager.Instance.HasKey(1))
        {
            gameObject.SetActive(false);
            return;
        }

        if (ItemManager.Instance.HasItem(ItemType.Cam))
        {
            gameObject.SetActive(true);
        }
    }
}