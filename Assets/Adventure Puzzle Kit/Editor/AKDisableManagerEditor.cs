using UnityEditor;
using UnityEngine;

namespace AdventurePuzzleKit
{
    [CustomEditor(typeof(AKDisableManager))]
    public class AKDisableManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Add a HelpBox at the top
            EditorGUILayout.HelpBox(
                "The Disable Manager controls cursor visibility, first-person player control, and camera effects during interaction. It can also persist across scenes if enabled.",
                MessageType.Info
            );

            // Draw the default inspector (handles serialized fields automatically)
            if (DrawDefaultInspector())
            {
                // Optional: Respond to changes if needed
            }

            // Add spacing
            EditorGUILayout.Space();

            // Add a HelpBox at the bottom
            EditorGUILayout.HelpBox(
                "You may need to edit the 'FPSController' field to accommodate your own controller, as per the online documentation.",
                MessageType.Warning
            );

            // Add spacing
            EditorGUILayout.Space();

            // Add a button to open the custom editor script
            if (GUILayout.Button("Open AKDisableManagerEditor Script"))
            {
                OpenEditorScript();
            }
        }

        private void OpenEditorScript()
        {
            // Find and open this editor script
            string scriptFilePath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            if (!string.IsNullOrEmpty(scriptFilePath))
            {
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(scriptFilePath));
            }
            else
            {
                Debug.LogWarning("Unable to locate the AKDisableManagerEditor script.");
            }
        }
    }
}
