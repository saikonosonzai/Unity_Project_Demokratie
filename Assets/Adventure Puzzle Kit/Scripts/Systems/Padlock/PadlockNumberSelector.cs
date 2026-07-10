using UnityEngine;
using UnityEngine.EventSystems;

namespace AdventurePuzzleKit.PadlockSystem
{
    // Handles the interaction and logic for a single number selector (spinner) in the padlock puzzle
    public class PadlockNumberSelector : MonoBehaviour, IPointerClickHandler
    {
        [Header("Padlock Row")]
        [SerializeField] private PadlockRow selectedRow = PadlockRow.row1; // Specifies which row (digit) of the padlock combination this selector controls
        private enum PadlockRow { row1, row2, row3, row4 }  // Enum to define the possible rows in the padlock combination

        private int spinnerNumber; // Current number displayed by the spinner (1-9)
        private int spinnerLimit; // Maximum value for the spinner (default is 9)
        private PadlockController _padlockController; // Reference to the PadlockController managing the overall padlock logic


        private void Awake()
        {
            spinnerNumber = 1; // Initialize spinner to 1
            spinnerLimit = 9; // Set maximum spinner value to 9
        }

        // Updates the reference to the PadlockController
        public void UpdatePadlockController(PadlockController newController)
        {
            _padlockController = newController; // Assign the provided controller
        }

        // Handles pointer click events (e.g., mouse clicks) on the spinner
        public void OnPointerClick(PointerEventData eventData)
        {
            RotateSpinner(); // Increment and rotate the spinner
            UpdatePadlockController(); // Update the controller with the new value
            _padlockController.CheckCombination(); // Check if the combination is correct
        }

        // Increments the spinner number and rotates the visual representation
        void RotateSpinner()
        {
            spinnerNumber = (spinnerNumber % spinnerLimit) + 1; // Increment number, looping from 9 to 1
            transform.Rotate(0, 0, transform.rotation.z + 40); // Rotate the spinner visually (40 degrees)
            _padlockController.SpinSound(); // Play the spin sound
        }

        // Updates the corresponding row in the PadlockController with the current spinner number
        void UpdatePadlockController()
        {
            int updatedRowValue = spinnerNumber; // Get the current spinner value
            switch (selectedRow)
            {
                case PadlockRow.row1:
                    _padlockController.combinationRow1 = updatedRowValue; // Update row 1
                    break;
                case PadlockRow.row2:
                    _padlockController.combinationRow2 = updatedRowValue; // Update row 2
                    break;
                case PadlockRow.row3:
                    _padlockController.combinationRow3 = updatedRowValue; // Update row 3
                    break;
                case PadlockRow.row4:
                    _padlockController.combinationRow4 = updatedRowValue; // Update row 4
                    break;
            }
        }
    }
}