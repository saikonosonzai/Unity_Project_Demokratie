using System;
using UnityEngine;
using AdventurePuzzleKit;

namespace AdventurePuzzleKit
{
    public class CollectableItem : MonoBehaviour, IInteractable
    {
        private const string SUBSYSTEM = "Collectable";
        private CollectableController _collectableController;

        public void Awake()
        {
            _collectableController = GetComponent<CollectableController>();
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
            _collectableController.Collect();
        }

        public void HandleInputHold() { }

        public void HandleInputStop() { }
    }
}