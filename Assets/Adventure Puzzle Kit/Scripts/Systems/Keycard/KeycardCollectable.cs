using UnityEngine;

namespace AdventurePuzzleKit.KeycardSystem
{
    public class KeycardCollectable : MonoBehaviour
    {
        [Space(10)][SerializeField] private Keycard keycardScriptable = null;

        [Space(10)][SerializeField] private Sound pickupSound = null;

        public void KeyPickup()
        {
            KeycardInventory.instance.AddKey(keycardScriptable);

            if(pickupSound != null )
            {
                AKAudioManager.instance.Play(pickupSound);
            }
            gameObject.SetActive(false);
        }
    }
}
