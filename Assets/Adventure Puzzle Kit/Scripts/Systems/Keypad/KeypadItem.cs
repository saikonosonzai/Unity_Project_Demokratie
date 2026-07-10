using UnityEngine;

namespace AdventurePuzzleKit.KeypadSystem
{
    public class KeypadItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private KeypadController _keypadController = null;

        public void StartLooking() { } //Started looking at the Keypad object

        public void StopInteraction() { } //Stopped interacting with the Keypad object

        public void HandleInputClick() //Started interaction with the Keypad object
        {
            _keypadController.ShowKeypad();
        }

        public void HandleInputHold() { } //Holding interaction with the Keypad object

        public void HandleInputStop() { } //Stopped interaction with the Keypad object

        public void ShowKeypad()
        {
            _keypadController.ShowKeypad();
        }
    }
}
