using UnityEngine;
using System.Collections.Generic;

namespace AdventurePuzzleKit.ThemedKey
{
    public class TKInventory : MonoBehaviour
    {
        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true;

        [Space(5)]
        public List<Key> _keyList = new List<Key>();

        public static TKInventory instance;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
        }

        public void AddKey(Key key)
        {
            if (!_keyList.Contains(key))
            {
                _keyList.Add(key);
                AKUIManager.instance.ThemedKeyCollected();
                AKUIManager.instance.FillTKInventorySlot();
            }
        }

        public void RemoveKey(Key key)
        {
            if (_keyList.Contains(key))
            {
                int currentCount = _keyList.Count;
                _keyList.Remove(key);
                AKUIManager.instance.ResetTKInventorySlot(currentCount);
            }
        }
    }
}
