using UnityEditor;
using UnityEngine;

namespace AdventurePuzzleKit
{
    [CustomEditor(typeof(AKMasterTrigger))]
    public class AKMasterTriggerEditor : Editor
    {
        // Serialized Properties
        SerializedProperty triggerType;
        SerializedProperty disableAfterUse;
        SerializedProperty linkedObject;
        SerializedProperty flashlightItemType;
        SerializedProperty batteryNumber;
        SerializedProperty playerTag;

        private void OnEnable()
        {
            // Link serialized properties
            triggerType = serializedObject.FindProperty("triggerType");
            disableAfterUse = serializedObject.FindProperty("disableAfterUse");
            linkedObject = serializedObject.FindProperty("linkedObject");
            flashlightItemType = serializedObject.FindProperty("flashlightItemType");
            batteryNumber = serializedObject.FindProperty("batteryNumber");
            playerTag = serializedObject.FindProperty("playerTag");
        }

        public override void OnInspectorGUI()
        {
            // Display MonoScript at the top
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((AKMasterTrigger)target), typeof(AKMasterTrigger), false);
            GUI.enabled = true;

            // Display a help box
            EditorGUILayout.HelpBox(
                "This script manages triggers for various interaction types in the Adventure Puzzle Kit. Use the Trigger Type dropdown to specify the interaction type.",
                MessageType.Info
            );

            // Update serialized object
            serializedObject.Update();

            // Display Trigger Type
            EditorGUILayout.PropertyField(triggerType);
            EditorGUILayout.PropertyField(disableAfterUse);

            EditorGUILayout.Space();

            // Show/hide fields based on Trigger Type
            AKMasterTrigger.TriggerType selectedType = (AKMasterTrigger.TriggerType)triggerType.enumValueIndex;

            switch (selectedType)
            {
                case AKMasterTrigger.TriggerType.None:
                    EditorGUILayout.HelpBox(
                        "No specific action is set for this trigger type.",
                        MessageType.Info
                    );
                    break;

                case AKMasterTrigger.TriggerType.Padlock:
                    EditorGUILayout.HelpBox(
                        "Add the Padlock Controller GameObject here, usually the padlock controller GameObject",
                        MessageType.Info
                    );
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(linkedObject);
                    break;

                case AKMasterTrigger.TriggerType.Note:
                    EditorGUILayout.HelpBox(
                        "Add the Note GameObject here, usually the Trigger Controller (Which has a NoteSelector script on).",
                        MessageType.Info
                    );
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(linkedObject);
                    break;

                case AKMasterTrigger.TriggerType.Safe:
                    EditorGUILayout.HelpBox(
                        "Add the SafeItem GameObject here, usually the 'Safe - Model' object.",
                        MessageType.Info
                    );
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(linkedObject);
                    break;

                case AKMasterTrigger.TriggerType.Keypad:
                    EditorGUILayout.HelpBox(
                        "Add the Keypad GameObject here, usually the interactive keypad model or script.",
                        MessageType.Info
                    );
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(linkedObject);
                    break;

                case AKMasterTrigger.TriggerType.Flashlight:
                    EditorGUILayout.HelpBox(
                        "This updates the FlashlightController, no need to add additional objects here",
                        MessageType.Info
                    );
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Flashlight Settings", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(flashlightItemType);
                    EditorGUILayout.PropertyField(batteryNumber);
                    break;

                case AKMasterTrigger.TriggerType.Phone:
                    EditorGUILayout.HelpBox(
                        "Add the PhoneItem GameObject here, usually the 'Phone - Model' object",
                        MessageType.Info
                    );
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(linkedObject);
                    break;
            }

            // General settings
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(playerTag);

            // Add space before additional options
            EditorGUILayout.Space();

            // Add a help box and a button to open this editor script
            EditorGUILayout.HelpBox(
                "If you need to customize this editor, you can open the AKMasterTriggerEditor script directly.",
                MessageType.Info
            );

            if (GUILayout.Button("Open AKMasterTriggerEditor Script"))
            {
                OpenEditorScript();
            }

            // Apply modified properties
            serializedObject.ApplyModifiedProperties();
        }

        private void OpenEditorScript()
        {
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            if (!string.IsNullOrEmpty(scriptPath))
            {
                AssetDatabase.OpenAsset(MonoScript.FromScriptableObject(this));
            }
            else
            {
                Debug.LogWarning("Unable to locate the AKMasterTriggerEditor script.");
            }
        }
    }
}
