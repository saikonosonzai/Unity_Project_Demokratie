using System;
using UnityEngine;
using AdventurePuzzleKit;

namespace AdventurePuzzleKit
{
    public class HatchItem : MonoBehaviour, IInteractable
    {
        private const string SUBSYSTEM = "Hatch";
        private Hatch_Controller _hatchController;

        public void Awake()
        {
            _hatchController = GetComponent<Hatch_Controller>();
        }

        public void StartLooking()
        {
            AKPromptManager.Instance.RegisterPromptsForSubsystem(SUBSYSTEM);
        }

        public void StopInteraction()
        {
            AKPromptManager.Instance.ClearPrompts();
        }

        public void HandleInputClick()
        {
            Debug.Log("hatch aktiviert!");
            _hatchController.isActive = true;
            
        }

        public void HandleInputHold() { }

        public void HandleInputStop() { }
    }
}