using UnityEditor;
using UnityEngine;

namespace AdventurePuzzleKit
{
    public class DebugSettingsWindow : EditorWindow
    {
        [MenuItem("APK/Tools/Debug Settings")]
        public static void ShowWindow()
        {
            GetWindow<DebugSettingsWindow>("Debug Settings");
        }

        private void OnGUI()
        {
            GUILayout.Label("Global Debug Settings", EditorStyles.boldLabel);

            GUILayout.Space(5);

            // Toggle for enabling/disabling debug logs
            DebugSettings.EnablePromptDebugLogs = EditorGUILayout.Toggle("Enable Prompt System Logs", DebugSettings.EnablePromptDebugLogs);

            GUILayout.Space(5);

            // Toggle for enabling/disabling debug logs
            DebugSettings.EnableDebugLogs = EditorGUILayout.Toggle("Enable System Item Logs", DebugSettings.EnableDebugLogs);

            GUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Toggling this setting will enable or disable debug logs for all SystemItem scripts (e.g., FuseItem, ValveItem, etc.) globally. " +
                "This is useful for reducing log clutter during playtesting or in production builds. " +
                "More systems may use this setting in the future as needed.",
                MessageType.Info);

            // Temporarily increase label width
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 220; // Adjust the width as needed

            // Toggle for enabling/disabling Post Processing message
            DebugSettings.EnablePostProcessingMessage = EditorGUILayout.Toggle("Enable Post Processing Debug Log", DebugSettings.EnablePostProcessingMessage);

            GUILayout.Space(5);
            EditorGUILayout.HelpBox("This option enables or disables the post processing message prompt in the console when you play your game", MessageType.Info);

            // Reset the label width to the original value
            EditorGUIUtility.labelWidth = originalLabelWidth;
        }
    }
}



