using System;
using UnityEngine;
using AdventurePuzzleKit;

namespace AdventurePuzzleKit
{
    public class KeyItem : MonoBehaviour, IInteractable
    {
        private const string SUBSYSTEM = "Key";
        private keyController _keyController;

        public void Awake()
        {
            _keyController = GetComponent<keyController>();
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

            _keyController.OnTrigger();
        }

        public void HandleInputHold() { }

        public void HandleInputStop() { }
    }
}