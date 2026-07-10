using UnityEngine;
using UnityEditor;
using AdventurePuzzleKit.GeneratorSystem;
using AdventurePuzzleKit.ValveSystem;

namespace AdventurePuzzleKit
{
    [CustomEditor(typeof(AKItem))]
    public class AKItemEditor : Editor
    {
        SerializedProperty _systemType;

        SerializedProperty showNameHighlight;
        SerializedProperty showEmissionHighlight;
        SerializedProperty highlightName;

        SerializedProperty showInteractPrompt;
        SerializedProperty showPickupPrompt;
        SerializedProperty showExaminePrompt;

        SerializedProperty isEmptyParent;
        SerializedProperty includeChildrenWithParentMesh;

        private void OnEnable()
        {
            _systemType = serializedObject.FindProperty(nameof(_systemType));

            showNameHighlight = serializedObject.FindProperty(nameof(showNameHighlight));
            showEmissionHighlight = serializedObject.FindProperty(nameof(showEmissionHighlight));
            highlightName = serializedObject.FindProperty(nameof(highlightName));

            showInteractPrompt = serializedObject.FindProperty(nameof(showInteractPrompt));
            showPickupPrompt = serializedObject.FindProperty(nameof(showPickupPrompt));
            showExaminePrompt = serializedObject.FindProperty(nameof(showExaminePrompt));

            isEmptyParent = serializedObject.FindProperty(nameof(isEmptyParent));
            includeChildrenWithParentMesh = serializedObject.FindProperty(nameof(includeChildrenWithParentMesh));
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((AKItem)target), typeof(AKItem), false);
            GUI.enabled = true;

            EditorGUILayout.Space(5); // General spacing at the start

            AKItem _AKitem = (AKItem)target;

            DrawSystemTypeSection(_AKitem);
            EditorGUILayout.Space(5); // Space between System Type and Name Highlight sections

            DrawNameHighlightSection(_AKitem);
            EditorGUILayout.Space(5); // Space between Name Highlight and Emission Highlight sections

            DrawEmissionHighlightSection(_AKitem);
            EditorGUILayout.Space(10); // Final spacing before the button

            OpenEditorScript();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawSystemTypeSection(AKItem item)
        {
            EditorGUILayout.LabelField("System Type", EditorStyles.toolbarTextField);
            EditorGUILayout.PropertyField(_systemType);
        }

        void DrawNameHighlightSection(AKItem item)
        {
            EditorGUILayout.LabelField("Name Highlight (Tooltip For More Information)", EditorStyles.toolbarTextField);
            EditorGUILayout.PropertyField(showNameHighlight);

            if (item.ShowNameHighlight)
            {
                EditorGUILayout.PropertyField(highlightName);
            }

            EditorGUILayout.PropertyField(showInteractPrompt);
            EditorGUILayout.PropertyField(showPickupPrompt);
            EditorGUILayout.PropertyField(showExaminePrompt);

            if (item.ShowExaminePrompt)
            {
                EditorGUILayout.Space(5);

                EditorGUILayout.HelpBox(
                    "Ensure the ExaminableItem script is attached to this object for examining functionality.",
                    MessageType.Warning);
            }

        }

        void DrawEmissionHighlightSection(AKItem item)
        {
            EditorGUILayout.LabelField("Emission Highlight Section", EditorStyles.toolbarTextField);
            EditorGUILayout.PropertyField(showEmissionHighlight);

            EditorGUILayout.Space(5);

            if (item.ShowEmissionHighlight)
            {
                EditorGUILayout.PropertyField(isEmptyParent);

                EditorGUILayout.Space(5);

                EditorGUILayout.HelpBox(
                    "• Enable 'Is Empty Parent' if THIS object has no Mesh Renderer but its child objects do. This will automatically find and highlight the child objects.",
                    MessageType.Info);

                EditorGUILayout.PropertyField(includeChildrenWithParentMesh);

                EditorGUILayout.Space(5);

                EditorGUILayout.HelpBox(
                    "• Enable 'Include Children With Parent Mesh' to highlight both THIS parent object (if it has a Mesh Renderer) and its specified child objects.",
                    MessageType.Info);
            }
        }

        void OpenEditorScript()
        {
            if (GUILayout.Button("Open Editor Script"))
            {
                string scriptFilePath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(scriptFilePath));
            }
        }
    }
}
