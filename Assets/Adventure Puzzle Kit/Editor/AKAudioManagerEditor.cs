using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace AdventurePuzzleKit
{
    [CustomEditor(typeof(AKAudioManager))]
    public class AKAudioManagerEditor : Editor
    {
        private SerializedProperty soundsProperty;
        private SerializedProperty persistAcrossScenesProperty;
        private SerializedProperty mixerGroupProperty;

        private string searchFilter = ""; // For search functionality
        private Dictionary<SoundCategory, List<SerializedProperty>> categorizedSounds; // Group sounds by enum category
        private bool[] foldoutStates;
        private bool showDefaultInspector = false; // Toggle state

        private void OnEnable()
        {
            // Get references to serialized properties
            soundsProperty = serializedObject.FindProperty("sounds");
            persistAcrossScenesProperty = serializedObject.FindProperty("persistAcrossScenes");
            mixerGroupProperty = serializedObject.FindProperty("mixerGroup");

            UpdateCategories();
        }

        private void UpdateCategories()
        {
            categorizedSounds = new Dictionary<SoundCategory, List<SerializedProperty>>();

            if (soundsProperty == null || !soundsProperty.isArray)
            {
                Debug.LogError("UpdateCategories: soundsProperty is null or not an array!");
                return;
            }

            for (int i = 0; i < soundsProperty.arraySize; i++)
            {
                SerializedProperty soundProperty = soundsProperty.GetArrayElementAtIndex(i);

                // Get the actual Sound ScriptableObject reference
                Sound sound = soundProperty.objectReferenceValue as Sound;

                if (sound == null)
                {
                    Debug.LogWarning($"UpdateCategories: Sound at index {i} is null or not assigned!");
                    continue;
                }

                SoundCategory category = sound.category;

                if (!categorizedSounds.ContainsKey(category))
                {
                    categorizedSounds[category] = new List<SerializedProperty>();
                }

                categorizedSounds[category].Add(soundProperty);
            }

            foldoutStates = new bool[categorizedSounds.Count];
        }

        public override void OnInspectorGUI()
        {
            // Display the script field
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((AKAudioManager)target), typeof(AKAudioManager), false);
            GUI.enabled = true;

            EditorGUILayout.Space();

            // Refresh Button
            if (GUILayout.Button("Refresh Sound List"))
            {
                RefreshSoundList();
            }

            EditorGUILayout.Space();

            // Display an explanatory message
            EditorGUILayout.HelpBox(
                "If you need to debug or edit the full array of the AKAudioManager, you can use the 'Show Default Inspector' button below.",
                MessageType.Info
            );

            EditorGUILayout.Space();

            // Debug Button
            if (GUILayout.Button(showDefaultInspector ? "Show Custom Inspector" : "Show Default Inspector"))
            {
                showDefaultInspector = !showDefaultInspector; // Toggle the state
            }

            if (showDefaultInspector)
            {
                // Show the default Unity inspector
                DrawDefaultInspector();
                return; // Skip the rest of the custom inspector
            }

            serializedObject.Update();

            // Clean up null entries in the sounds array
            CleanUpNullReferences();

            // Draw basic settings
            EditorGUILayout.PropertyField(persistAcrossScenesProperty, new GUIContent("Persist Across Scenes"));
            EditorGUILayout.PropertyField(mixerGroupProperty, new GUIContent("Sound Mixer Group"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Search & Categories Section", EditorStyles.toolbarTextField);

            // Search box
            EditorGUILayout.LabelField("Search", EditorStyles.boldLabel);
            searchFilter = EditorGUILayout.TextField(searchFilter);

            EditorGUILayout.Space();

            // Display categorized sounds
            if (categorizedSounds != null && categorizedSounds.Count > 0)
            {
                int index = 0;
                foreach (var category in categorizedSounds)
                {
                    // Skip categories that don't match the search filter
                    if (!string.IsNullOrEmpty(searchFilter) &&
                        !category.Key.ToString().ToLower().Contains(searchFilter.ToLower()))
                    {
                        index++;
                        continue;
                    }

                    foldoutStates[index] = EditorGUILayout.Foldout(foldoutStates[index], category.Key.ToString(), true);

                    if (foldoutStates[index])
                    {
                        foreach (var soundProp in category.Value)
                        {
                            EditorGUILayout.PropertyField(soundProp);
                        }
                    }

                    index++;
                }
            }

            EditorGUILayout.Space();

            // Add "Manage Sounds" Section
            DrawManageSoundsSection();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Additional Debugging", EditorStyles.toolbarTextField);

            EditorGUILayout.HelpBox(
            "The 'Fix Missing References' button removes any missing or null entries from the sounds array. Use this if you notice empty slots in the list.",
            MessageType.Info);

            // Add Fix Missing References Button
            if (GUILayout.Button("Fix Missing References"))
            {
                FixMissingReferences();
            }

            // Add the "Edit Custom Editor Script" Button
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
            "The 'Edit AKAudioManagerEditor Script' button opens the custom editor script in your code editor. Use this if you need to customize how the inspector works.",
            MessageType.Info);

            if (GUILayout.Button("Edit AKAudioManagerEditor Script"))
            {
                OpenEditorScript();
            }

            serializedObject.ApplyModifiedProperties();
        }

        //Refreshes the list of sounds to ensure new tracks are detected.
        private void RefreshSoundList()
        {
            serializedObject.Update();
            UpdateCategories(); // Force update
            serializedObject.ApplyModifiedProperties();
            Debug.Log("Sound list refreshed!");
        }

        private void DrawManageSoundsSection()
        {
            EditorGUILayout.LabelField("Click here to create new sound:", EditorStyles.toolbarTextField);
            EditorGUILayout.Space(2);

            EditorGUILayout.HelpBox(
            "The 'Add New Sound' button creates a new Sound ScriptableObject asset and adds it to the sounds array. You will be prompted to save the new asset.",
            MessageType.Info);

            // Add a new Sound ScriptableObject to the array
            if (GUILayout.Button("Add New Sound"))
            {
                // Create a new ScriptableObject instance
                Sound newSound = ScriptableObject.CreateInstance<Sound>();
                newSound.name = "New Sound";

                // Save the asset
                string path = EditorUtility.SaveFilePanelInProject("Save New Sound", "New Sound", "asset", "Choose a location to save the new Sound.");
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.CreateAsset(newSound, path);
                    AssetDatabase.SaveAssets();

                    // Add the new sound to the array
                    soundsProperty.InsertArrayElementAtIndex(soundsProperty.arraySize);
                    soundsProperty.GetArrayElementAtIndex(soundsProperty.arraySize - 1).objectReferenceValue = newSound;

                    Debug.Log($"New Sound created at {path}");
                }
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Drag and Drop Sounds Here:", EditorStyles.toolbarTextField);

            // Drag and drop existing Sound assets
            //EditorGUILayout.LabelField("Drag and Drop Sounds Here:", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
            "Drag and drop existing Sound ScriptableObject assets into the box below to quickly add them to the sounds array.",
            MessageType.Info);

            Rect dropArea = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drop Sound Assets Here");

            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (dropArea.Contains(evt.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object obj in DragAndDrop.objectReferences)
                        {
                            Sound sound = obj as Sound;
                            if (sound != null)
                            {
                                soundsProperty.InsertArrayElementAtIndex(soundsProperty.arraySize);
                                soundsProperty.GetArrayElementAtIndex(soundsProperty.arraySize - 1).objectReferenceValue = sound;
                            }
                        }
                    }

                    Event.current.Use();
                }
            }
        }

        private void CleanUpNullReferences()
        {
            for (int i = soundsProperty.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty soundProperty = soundsProperty.GetArrayElementAtIndex(i);
                if (soundProperty.objectReferenceValue == null)
                {
                    soundsProperty.DeleteArrayElementAtIndex(i);
                }
            }
        }

        private void FixMissingReferences()
        {
            int removedCount = 0;

            for (int i = soundsProperty.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty soundProperty = soundsProperty.GetArrayElementAtIndex(i);
                if (soundProperty.objectReferenceValue == null)
                {
                    soundsProperty.DeleteArrayElementAtIndex(i);
                    removedCount++;
                }
            }

            Debug.Log($"Removed {removedCount} missing references from the sounds array.");
        }

        private void OpenEditorScript()
        {
            // Find the MonoScript for the AKAudioManagerEditor class
            MonoScript script = MonoScript.FromScriptableObject(this);
            if (script != null)
            {
                AssetDatabase.OpenAsset(script);
            }
            else
            {
                Debug.LogWarning("Unable to locate the AKAudioManagerEditor script.");
            }
        }
    }
}
