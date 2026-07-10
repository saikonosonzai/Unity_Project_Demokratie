using UnityEditor;
using UnityEngine;

namespace AdventurePuzzleKit.ExamineSystem
{
    [CustomEditor(typeof(ExaminableItem))]
    public class ExaminableItemEditor : Editor
    {
        #region SerializedProperty Fields
        SerializedProperty _isCollectable;
        SerializedProperty _systemType;
        SerializedProperty hasChildObjects;
        SerializedProperty removeInteractionCollider;

        SerializedProperty initialRotationOffset;
        SerializedProperty horizontalOffset;
        SerializedProperty verticalOffset;

        SerializedProperty smoothExamineSpeed;
        SerializedProperty initialZoom;
        SerializedProperty zoomRange;
        SerializedProperty zoomSensitivity;

        SerializedProperty rotationSpeed;
        SerializedProperty invertRotation;

        SerializedProperty _hasInspectPoints;
        SerializedProperty inspectPoints;

        SerializedProperty pickupSound;
        SerializedProperty dropSound;

        SerializedProperty _UIType;

        SerializedProperty itemName;

        SerializedProperty textSize;
        SerializedProperty fontType;
        SerializedProperty fontColor;

        SerializedProperty itemDescription;

        SerializedProperty textSizeDesc;
        SerializedProperty fontTypeDesc;
        SerializedProperty fontColorDesc;
        #endregion

        #region Item / Description Groups

        bool itemNameGroup
        {
            get { return EditorPrefs.GetBool("itemNameGroup", false); }
            set { EditorPrefs.SetBool("itemNameGroup", value); }
        }

        bool itemDescriptionGroup
        {
            get { return EditorPrefs.GetBool("itemDescriptionGroup", false); }
            set { EditorPrefs.SetBool("itemDescriptionGroup", value); }
        }
        #endregion
        void OnEnable()
        {
            #region SerializedObject Properties
            _isCollectable = serializedObject.FindProperty(nameof(_isCollectable));
            _systemType = serializedObject.FindProperty(nameof(_systemType));
            hasChildObjects = serializedObject.FindProperty(nameof(hasChildObjects));
            removeInteractionCollider = serializedObject.FindProperty(nameof(removeInteractionCollider));

            initialRotationOffset = serializedObject.FindProperty(nameof(initialRotationOffset));
            horizontalOffset = serializedObject.FindProperty(nameof(horizontalOffset));
            verticalOffset = serializedObject.FindProperty(nameof(verticalOffset));

            smoothExamineSpeed = serializedObject.FindProperty(nameof(smoothExamineSpeed));
            initialZoom = serializedObject.FindProperty(nameof(initialZoom));
            zoomRange = serializedObject.FindProperty(nameof(zoomRange));
            zoomSensitivity = serializedObject.FindProperty(nameof(zoomSensitivity));

            rotationSpeed = serializedObject.FindProperty(nameof(rotationSpeed));
            invertRotation = serializedObject.FindProperty(nameof(invertRotation));

            _hasInspectPoints = serializedObject.FindProperty(nameof(_hasInspectPoints));
            inspectPoints = serializedObject.FindProperty(nameof(inspectPoints));

            pickupSound = serializedObject.FindProperty(nameof(pickupSound));
            dropSound = serializedObject.FindProperty(nameof(dropSound));

            _UIType = serializedObject.FindProperty(nameof(_UIType));

            itemName = serializedObject.FindProperty(nameof(itemName));

            textSize = serializedObject.FindProperty(nameof(textSize));
            fontType = serializedObject.FindProperty(nameof(fontType));
            fontColor = serializedObject.FindProperty(nameof(fontColor));

            itemDescription = serializedObject.FindProperty(nameof(itemDescription));

            textSizeDesc = serializedObject.FindProperty(nameof(textSizeDesc));
            fontTypeDesc = serializedObject.FindProperty(nameof(fontTypeDesc));
            fontColorDesc = serializedObject.FindProperty(nameof(fontColorDesc));
            #endregion
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((ExaminableItem)target), typeof(ExaminableItem), false);
            GUI.enabled = true;
            EditorGUILayout.Space(5);

            ExaminableItem _examineItemController = (ExaminableItem)target;

            EditorGUILayout.LabelField("General Settings", EditorStyles.toolbarTextField);

            EditorGUILayout.PropertyField(hasChildObjects);
            if (!hasChildObjects.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Enable 'Has Child Objects' if THIS object is an empty parent or has child objects. This will automatically find and set the child objects to the ExamineLayer",
                    MessageType.Info);
            }

            EditorGUILayout.PropertyField(_isCollectable);
            if (_examineItemController.isCollectable)
            {
                EditorGUILayout.PropertyField(_systemType);
            }

            EditorGUILayout.PropertyField(removeInteractionCollider);
            if (!removeInteractionCollider.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Enable 'Remove Interaction Collider' if you want to remove the collider on the parent examined object (Good if you want to access an item inside that this collider might be blocking",
                    MessageType.Info);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Inspect Point Settings", EditorStyles.toolbarTextField);

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(_hasInspectPoints);

            if (_examineItemController.hasInspectPoints)
            {
                EditorGUILayout.PropertyField(inspectPoints);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Rotation Settings", EditorStyles.toolbarTextField);

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(rotationSpeed);
            EditorGUILayout.PropertyField(invertRotation);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Offset Settings", EditorStyles.toolbarTextField);

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(initialRotationOffset);
            EditorGUILayout.PropertyField(horizontalOffset);
            EditorGUILayout.PropertyField(verticalOffset);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Zoom Settings", EditorStyles.toolbarTextField);

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(smoothExamineSpeed);
            EditorGUILayout.PropertyField(initialZoom);
            EditorGUILayout.PropertyField(zoomRange);
            EditorGUILayout.PropertyField(zoomSensitivity);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Text Settings", EditorStyles.toolbarTextField);

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(_UIType);

            itemNameGroup = EditorGUILayout.BeginFoldoutHeaderGroup(itemNameGroup, "Item Name Settings");
            if (itemNameGroup)
            {
                EditorGUILayout.PropertyField(itemName);
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(textSize);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(fontType);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(fontColor);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            itemDescriptionGroup = EditorGUILayout.BeginFoldoutHeaderGroup(itemDescriptionGroup, "Item Description Settings");
            if (itemDescriptionGroup)
            {
                EditorGUILayout.PropertyField(itemDescription);
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(textSizeDesc);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(fontTypeDesc);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(fontColorDesc);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Audio Settings", EditorStyles.toolbarTextField);

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(pickupSound);
            EditorGUILayout.PropertyField(dropSound);

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

