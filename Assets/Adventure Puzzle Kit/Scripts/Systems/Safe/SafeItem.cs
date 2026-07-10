using UnityEngine;

namespace AdventurePuzzleKit.SafeSystem
{
    public class SafeItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private SafeController _safeController = null;

        public void StartLooking() { } //Started looking at the Safe object

        public void StopInteraction() { } //Stopped interacting with the Safe object

        public void HandleInputClick() //Started interaction with the Safe object
        {
            _safeController.ShowSafeUI();
        }

        public void HandleInputHold() { } //Holding interaction with the Safe object

        public void HandleInputStop() { } //Stopped interaction with the Safe object

        public void ShowSafeLock()
        {
            _safeController.ShowSafeUI();
        }
    }
}
