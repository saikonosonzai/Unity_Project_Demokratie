using UnityEngine;

namespace AdventurePuzzleKit.PadlockSystem
{
    public class PadlockItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private PadlockController _padlockController = null;

        public void StartLooking() { } //Started looking at the Padlock object

        public void StopInteraction() { } //Stopped interacting with the Padlock object

        public void HandleInputClick() //Started interaction with the Padlock object
        {
            _padlockController.ShowPadlock();
        }

        public void HandleInputHold() { } //Holding interaction with the Padlock object

        public void HandleInputStop() { } //Stopped interaction with the Padlock object

        public void ObjectInteract()
        {
            _padlockController.ShowPadlock();
        }
    }
}
