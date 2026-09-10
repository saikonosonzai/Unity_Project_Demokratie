using System;
using UnityEngine;
using AdventurePuzzleKit;

namespace AdventurePuzzleKit
{
    public class CandleItem : MonoBehaviour, IInteractable
    {
        private const string SUBSYSTEM = "Candle";
        private CandleController _candleController;

        public void Awake()
        {
            _candleController = GetComponent<CandleController>();
        }

        public void StartLooking()
        {
            AKPromptManager.Instance.RegisterPromptsForSubsystem(SUBSYSTEM);
            print("Looking for candle");
        }

        public void StopInteraction()
        {
            AKPromptManager.Instance.ClearPrompts();
        }

        public void HandleInputClick()
        {
            Debug.Log("Clock aktiviert!");
            _candleController.OnTrigger();

        }

        public void HandleInputHold() { }

        public void HandleInputStop() { }
    }
}