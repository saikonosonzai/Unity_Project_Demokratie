using UnityEngine;
using UnityEditor;

namespace AdventurePuzzleKit.FuseSystem
{
    [CustomEditor(typeof(FuseItem), true)] // Add "true" to make the custom editor apply to derived classes.
    public class FuseItemEditor : Editor
    {
        SerializedProperty _itemType;
        SerializedProperty pickupSound;

        private void OnEnable()
        {
            _itemType = serializedObject.FindProperty(nameof(_itemType));
            pickupSound = serializedObject.FindProperty(nameof(pickupSound));
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((FuseItem)target), typeof(FuseItem), false);
            GUI.enabled = true;

            // Draw the serialized properties
            EditorGUILayout.PropertyField(_itemType);

            // Cast the selected enum value for conditional UI display
            FuseItem.ItemType selectedType = (FuseItem.ItemType)_itemType.enumValueIndex;

            if (selectedType == FuseItem.ItemType.Fuse)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(pickupSound);
            }

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

