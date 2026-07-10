using UnityEngine;

namespace AdventurePuzzleKit.PhoneSystem
{
    public class PhoneItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private PhoneController _phoneController = null;

        public void StartLooking() { } //Started looking at the Keypad object

        public void StopInteraction() { } //Stopped interacting with the Keypad object

        public void HandleInputClick() //Started interaction with the Keypad object
        {
            _phoneController.ShowKeypad();
        }

        public void HandleInputHold() { } //Holding interaction with the Keypad object

        public void HandleInputStop() { } //Stopped interaction with the Keypad object

        public void ShowKeypad()
        {
            _phoneController.ShowKeypad();
        }
    }
}
