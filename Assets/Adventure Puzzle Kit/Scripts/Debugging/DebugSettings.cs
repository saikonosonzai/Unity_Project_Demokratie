using UnityEditor;

namespace AdventurePuzzleKit
{
    public static class DebugSettings
    {
        private const string EnableDebugLogsKey = "AdventurePuzzleKit_EnableDebugLogs";
        private const string EnablePromptDebugLogsKey = "AdventurePuzzleKit_EnablePromptDebugLogs";
        private const string EnablePostProcessingMessageKey = "AdventurePuzzleKit_EnablePostProcessingMessage";

        public static bool EnablePromptDebugLogs
        {
            get => EditorPrefs.GetBool(EnablePromptDebugLogsKey, true);
            set => EditorPrefs.SetBool(EnablePromptDebugLogsKey, value);
        }

        public static bool EnableDebugLogs
        {
            get => EditorPrefs.GetBool(EnableDebugLogsKey, true);
            set => EditorPrefs.SetBool(EnableDebugLogsKey, value);
        }

        public static bool EnablePostProcessingMessage
        {
            get => EditorPrefs.GetBool(EnablePostProcessingMessageKey, true);
            set => EditorPrefs.SetBool(EnablePostProcessingMessageKey, value);
        }
    }
}



