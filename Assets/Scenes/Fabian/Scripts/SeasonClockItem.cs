using System;
using UnityEngine;
using AdventurePuzzleKit;

namespace AdventurePuzzleKit
{
    public class SeasonClockItem : MonoBehaviour, IInteractable
    {
        private const string SUBSYSTEM = "Clock";
        private SeasonClockController _clockController;

        public void Awake()
        {
            _clockController = GetComponent<SeasonClockController>();
        }

        public void StopInteraction()
        {
            AKPromptManager.Instance.ClearPrompts();
        }

        public void StartLooking()
        {
            if (_clockController != null && !_clockController.puzzleSolved)
            {
                AKPromptManager.Instance.RegisterPromptsForSubsystem(SUBSYSTEM);
            }
        }

        public void HandleInputClick()
        {
            if (_clockController != null && !_clockController.puzzleSolved)
            {
                Debug.Log("Clock aktiviert!");
                _clockController.StartPuzzle();
            }
        }

        public void HandleInputHold() { }

        public void HandleInputStop() { }
    }
}