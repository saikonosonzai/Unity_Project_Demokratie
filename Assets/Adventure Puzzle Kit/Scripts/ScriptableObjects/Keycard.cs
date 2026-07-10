using UnityEngine;

namespace AdventurePuzzleKit.KeycardSystem
{
    [CreateAssetMenu(menuName = "Keycard")]
    public class Keycard : ScriptableObject
    {
        [SerializeField] private string keycardName = "Key";
        [SerializeField] private Sprite keycardSprite = null;
        [SerializeField] private string keycardID = null;

        public Sprite KeycardSprite
        {
            get { return keycardSprite; }
        }

        public string KeycardName
        {
            get { return keycardName; }
        }

        public string KeycardID
        {
            get { return keycardID; }
        }
    }
}

