using System;
using UnityEngine;
using AdventurePuzzleKit;

namespace AdventurePuzzleKit
{
    public class ClockItem : MonoBehaviour, IInteractable
    {
        private const string SUBSYSTEM = "Clock";
        private ClockController _clockController;

        public void Awake()
        {
            _clockController = GetComponent<ClockController>();
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
            Debug.Log("Clock aktiviert!");
            _clockController.isActive = true;
            
        }

        public void HandleInputHold() { }

        public void HandleInputStop() { }
    }
}