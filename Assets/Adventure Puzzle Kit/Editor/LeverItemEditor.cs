using UnityEngine;
using UnityEditor;

namespace AdventurePuzzleKit.LeverSystem
{
    [CustomEditor(typeof(LeverItem), true)] // Add "true" to make the custom editor apply to derived classes.
    public class LeverItemEditor : Editor
    {
        SerializedProperty _itemType;
        SerializedProperty leverNumber;
        SerializedProperty animationName;
        SerializedProperty _leverSystemController;

        private void OnEnable()
        {
            _itemType = serializedObject.FindProperty(nameof(_itemType));
            leverNumber = serializedObject.FindProperty(nameof(leverNumber));
            animationName = serializedObject.FindProperty(nameof(animationName));
            _leverSystemController = serializedObject.FindProperty(nameof(_leverSystemController));
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((LeverItem)target), typeof(LeverItem), false);
            GUI.enabled = true;

            // Draw the serialized properties
            EditorGUILayout.PropertyField(_itemType);

            // Cast the selected enum value for conditional UI display
            LeverItem.ItemType selectedType = (LeverItem.ItemType)_itemType.enumValueIndex;

            if (selectedType == LeverItem.ItemType.Lever)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(leverNumber);
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(animationName);

            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(_leverSystemController);

            EditorGUILayout.Space(5);

            OpenEditorScript();

            serializedObject.ApplyModifiedProperties();
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
