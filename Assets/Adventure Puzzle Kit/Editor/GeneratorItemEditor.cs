using UnityEditor;
using UnityEngine;

namespace AdventurePuzzleKit.GeneratorSystem
{
    [CustomEditor(typeof(GeneratorItem))]
    public class GeneratorItemEditor : Editor
    {
        SerializedProperty itemType;

        SerializedProperty _canBurnFuel;
        SerializedProperty burnRate;

        SerializedProperty _canRumble;
        SerializedProperty rumbleSpeed;
        SerializedProperty rumbleIntensity;

        SerializedProperty fillRate;
        SerializedProperty _itemFuelAmount;
        SerializedProperty _itemMaxFuelAmount;

        SerializedProperty infiniteFuelBarrel;

        SerializedProperty _showUI;
        SerializedProperty _popoutUI;

        SerializedProperty interactSound;
        SerializedProperty fuelPourSound;

        SerializedProperty activateGenerator;
        SerializedProperty deactivateGenerator;

        bool generatorSelection, fuelParameters, soundGroup, generatorEvents;

        void OnEnable()
        {
            #region SerializedObject Properties
            itemType = serializedObject.FindProperty(nameof(itemType));

            _canBurnFuel = serializedObject.FindProperty(nameof(_canBurnFuel));
            burnRate = serializedObject.FindProperty(nameof(burnRate));

            _canRumble = serializedObject.FindProperty(nameof(_canRumble));
            rumbleSpeed = serializedObject.FindProperty(nameof(rumbleSpeed));
            rumbleIntensity = serializedObject.FindProperty(nameof(rumbleIntensity));

            fillRate = serializedObject.FindProperty(nameof(fillRate));
            _itemFuelAmount = serializedObject.FindProperty(nameof(_itemFuelAmount));
            _itemMaxFuelAmount = serializedObject.FindProperty(nameof(_itemMaxFuelAmount));

            infiniteFuelBarrel = serializedObject.FindProperty(nameof(infiniteFuelBarrel));

            _showUI = serializedObject.FindProperty(nameof(_showUI));
            _popoutUI = serializedObject.FindProperty(nameof(_popoutUI));

            interactSound = serializedObject.FindProperty(nameof(interactSound));
            fuelPourSound = serializedObject.FindProperty(nameof(fuelPourSound));

            activateGenerator = serializedObject.FindProperty(nameof(activateGenerator));
            deactivateGenerator = serializedObject.FindProperty(nameof(deactivateGenerator));
            #endregion
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((GeneratorItem)target), typeof(GeneratorItem), false);
            GUI.enabled = true;

            //GeneratorItem _generatorItem = (GeneratorItem)target;
            EditorGUILayout.Space(5);

            // Item type selection
            EditorGUILayout.LabelField("Item Type", EditorStyles.toolbarTextField);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(itemType);

            GeneratorItem.GeneratorItemType currentType = (GeneratorItem.GeneratorItemType)itemType.enumValueIndex;

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Fuel Parameters", EditorStyles.toolbarTextField);
            EditorGUILayout.Space(5);

            if (currentType == GeneratorItem.GeneratorItemType.FuelBarrel)
            {
                EditorGUILayout.PropertyField(infiniteFuelBarrel);

                // Check if the infiniteFuelBarrel is not set to true
                if (infiniteFuelBarrel.boolValue == false)
                {
                    EditorGUILayout.PropertyField(_itemFuelAmount);
                    EditorGUILayout.PropertyField(_itemMaxFuelAmount);
                }
            }
            else
            {
                // If the item is not a Fuel Barrel then always show fuel amount and max fuel amount
                EditorGUILayout.PropertyField(_itemFuelAmount);
                EditorGUILayout.PropertyField(_itemMaxFuelAmount);
            }

            if (currentType == GeneratorItem.GeneratorItemType.Generator)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Generator ONLY Parameters", EditorStyles.toolbarTextField);
                EditorGUILayout.Space(5);

                EditorGUILayout.PropertyField(fillRate);

                EditorGUILayout.Space(5);

                EditorGUILayout.PropertyField(_canBurnFuel);
                if (_canBurnFuel.boolValue)
                {
                    EditorGUILayout.PropertyField(burnRate);
                }

                EditorGUILayout.Space(5);

                EditorGUILayout.PropertyField(_canRumble);
                if (_canRumble.boolValue)
                {
                    EditorGUILayout.PropertyField(rumbleSpeed);
                    EditorGUILayout.PropertyField(rumbleIntensity);
                }
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Object Canvas Settings", EditorStyles.toolbarTextField);

            EditorGUILayout.PropertyField(_showUI);
            if (_showUI.boolValue)
            {
                EditorGUILayout.PropertyField(_popoutUI);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Sound Effects", EditorStyles.toolbarTextField);

            EditorGUILayout.Space(5);

            if (currentType == GeneratorItem.GeneratorItemType.Generator)
            {
                EditorGUILayout.PropertyField(fuelPourSound);
            }
            else
            {
                EditorGUILayout.PropertyField(interactSound);
            }
            EditorGUILayout.Space(5);

            if (currentType == GeneratorItem.GeneratorItemType.Generator)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Generator Activate / Deactivate ", EditorStyles.toolbarTextField);

                generatorEvents = EditorGUILayout.BeginFoldoutHeaderGroup(generatorEvents, "Generator Events");
                if (generatorEvents)
                {
                    EditorGUILayout.PropertyField(activateGenerator);
                    EditorGUILayout.PropertyField(deactivateGenerator);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
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
