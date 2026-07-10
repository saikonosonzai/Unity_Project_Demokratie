// Location: Assets/Scripts/Interfaces/IInteractable.cs

namespace AdventurePuzzleKit
{
    public interface IInteractable
    {
        void StartLooking();       // Called when the player looks at the object
        void StopInteraction();    // Called when the player stops looking or interacting
        void HandleInputClick();   // Called when the interaction starts (key down)
        void HandleInputHold();    // Called while the interaction is held (key held)
        void HandleInputStop();    // Called when the interaction ends (key up)
    }
}

