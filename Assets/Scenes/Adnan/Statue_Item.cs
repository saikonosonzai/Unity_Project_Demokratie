using System;
using UnityEngine;
using AdventurePuzzleKit;

namespace AdventurePuzzleKit
{
    public class Statue_Item : MonoBehaviour, IInteractable
    {
        private const string SUBSYSTEM = "Statue";
        private Statue_Controller _statueController;

        public void Awake()
        {
            _statueController = GetComponent<Statue_Controller>();
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
            Debug.Log("Statue wurde aktiviert!");
            if (_statueController != null)
            {
                _statueController.ActivatePuzzle(); // Verwende die Methode statt direkter Variable
            }
        }

        public void HandleInputHold() { }

        public void HandleInputStop() { }
    }
}