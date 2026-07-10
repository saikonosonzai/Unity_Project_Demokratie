using System.Collections.Generic;
using UnityEngine;

namespace AdventurePuzzleKit
{
    public class AKPromptManager : MonoBehaviour
    {
        // Static instance for global access (singleton pattern)
        private static AKPromptManager _instance;
        public static AKPromptManager Instance => _instance;

        // Represents a single UI prompt (e.g., "E to Interact")
        [System.Serializable]
        public class Prompt
        {
            [Tooltip("Key or button used in the prompt (e.g., E, LMB, Scroll)")]
            public string Key;

            [Tooltip("Description of the prompt action (e.g., Rotate, Close, Zoom)")]
            public string Label;

            public Prompt(string key, string label)
            {
                Key = key;
                Label = label;
            }
        }

        // Group of prompts tied to a specific subsystem (like Examine or Flashlight)
        [System.Serializable]
        public class SubsystemPrompts
        {
            [Tooltip("Name of the subsystem (e.g., Examine, Flashlight)")]
            public string SubsystemName;

            [Tooltip("List of prompts associated with this subsystem")]
            public List<Prompt> Prompts;
        }

        [Tooltip("Define prompts for each subsystem here")]
        public List<SubsystemPrompts> SubsystemPromptDefinitions = new List<SubsystemPrompts>();

        // Currently active prompts shown on screen
        private List<Prompt> activePrompts = new List<Prompt>();

        private void Awake()
        {
            // Handle singleton setup (only one PromptManager allowed)
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        // Register prompts for a specific subsystem by name
        public void RegisterPromptsForSubsystem(string subsystemIdentifier)
        {
            if (DebugSettings.EnablePromptDebugLogs)
                Debug.Log($"Registering prompts for subsystem: {subsystemIdentifier}");

            // Find the matching subsystem in the list
            var subsystem = SubsystemPromptDefinitions.Find(s => s.SubsystemName == subsystemIdentifier);

            if (subsystem != null)
            {
                if (DebugSettings.EnablePromptDebugLogs)
                    Debug.Log($"Found subsystem {subsystemIdentifier} with {subsystem.Prompts.Count} prompts.");

                // Register those prompts
                RegisterPrompts(subsystem.Prompts);
            }
            else
            {
                // If no match found, clear all prompts
                if (DebugSettings.EnablePromptDebugLogs)
                    Debug.LogWarning($"No prompts found for subsystem: {subsystemIdentifier}. Clearing prompts.");

                ClearPrompts();
            }
        }

        // Directly register a list of prompts
        public void RegisterPrompts(List<Prompt> prompts)
        {
            activePrompts.Clear();                     // Remove any previous prompts
            activePrompts.AddRange(prompts);           // Add new ones

            if (DebugSettings.EnablePromptDebugLogs)
                Debug.Log($"Registered {activePrompts.Count} prompts: {string.Join(", ", activePrompts.ConvertAll(p => $"{p.Key}: {p.Label}"))}");

            UpdateUI();                                // Refresh the UI
        }

        // Clear all active prompts
        public void ClearPrompts()
        {
            if (DebugSettings.EnablePromptDebugLogs)
                Debug.Log("Clearing all prompts.");

            activePrompts.Clear();
            UpdateUI();                                // Refresh the UI
        }

        // Pass the current prompts to the UI Manager
        private void UpdateUI()
        {
            AKUIManager.instance.UpdatePromptsUI(activePrompts);
        }
    }
}
