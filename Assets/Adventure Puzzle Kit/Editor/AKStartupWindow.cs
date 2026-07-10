using UnityEditor;
using UnityEngine;

namespace AdventurePuzzleKit
{
    public class AKStartupWindow : EditorWindow
    {
        // Foldout toggles for existing questions
        private bool postProcessingFoldout;
        private bool itemTextFoldout;
        private bool layersTextFoldout;
        private bool quickSetupTextFoldout;
        private bool buttonPromptsFoldout;
        // Foldout toggles for new questions
        private bool faqRenderPipelineFoldout;
        private bool faqPinkMaterialsFoldout;
        private bool faqExistingProjectFoldout;
        private bool faqDemoSceneFoldout;
        private bool faqCustomizationFoldout;
        private bool faqSwitchingPipelinesFoldout;
        private bool faqMobileVRFoldout;
        private bool faqUnityVersionFoldout;

        private Texture banner;
        private const string DontOpenKey = "AdventureKitDontOpenAutomatically";

        // Menu item to manually open the support hub window
        [MenuItem("APK/Tools/Adventure Kit Support Hub")]
        private static void Open()
        {
            EditorApplication.delayCall += () =>
            {
                GetWindow<AKStartupWindow>().Show(); // Open the window after a delay
            };
        }

        // Automatically open the window on editor load
        [InitializeOnLoadMethod]
        private static void OpenOnStart()
        {
            EditorApplication.update += CheckToOpenWindow; // Register the check method
        }

        // Checks if the window should open automatically
        private static void CheckToOpenWindow()
        {
            // Open only if not disabled and not already opened in this session
            if (!EditorPrefs.GetBool(DontOpenKey, false) && !SessionState.GetBool("AdventureKitSupportOpened", false))
            {
                Open();
                SessionState.SetBool("AdventureKitSupportOpened", true);
                EditorApplication.update -= CheckToOpenWindow; // Unregister after opening
            }
        }

        // Load the banner texture when the window is enabled
        private void OnEnable()
        {
            banner = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Adventure Puzzle Kit/Additional Packages/AKBanner.png", typeof(Texture));
        }

        // Render the window's GUI
        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // Display the banner centered horizontally
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(banner, GUILayout.Width(480), GUILayout.Height(100));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            StarterMessage();

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Common Questions", EditorStyles.toolbarTextField);

            EditorGUILayout.Space(5);

            QuickSetupMessage();

            EditorGUILayout.Space(5);

            LayersMessage();

            EditorGUILayout.Space(5);

            PostProcessingMessage();

            EditorGUILayout.Space(5);

            ButtonPromptsMessage();

            EditorGUILayout.Space(5);

            ItemNameMessage();

            // New FAQ sections
            EditorGUILayout.Space(5);

            FAQRenderPipelineMessage();

            EditorGUILayout.Space(5);

            FAQPinkMaterialsMessage();

            EditorGUILayout.Space(5);

            FAQExistingProjectMessage();

            EditorGUILayout.Space(5);

            FAQDemoSceneMessage();

            EditorGUILayout.Space(5);

            FAQCustomizationMessage();

            EditorGUILayout.Space(5);

            FAQSwitchingPipelinesMessage();

            EditorGUILayout.Space(5);

            FAQMobileVRMessage();

            EditorGUILayout.Space(5);

            FAQUnityVersionMessage();

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Support Links", EditorStyles.toolbarTextField);

            SupportLinks();

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Useful Links", EditorStyles.toolbarTextField);

            UsefulLinks();

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Debug Settings", EditorStyles.toolbarTextField);

            AddDebugSettingsButton();

            EditorGUILayout.Space(5);

            DontOpenAutomaticallyToggle();
        }

        // Display the introductory message
        void StarterMessage()
        {
            string starterText = "Hey there! Welcome to the Adventure Puzzle Kit.\n" +
            "If you have any issues or suggestions please send me an email and I'll get back to you as soon as possible (That's the best way to get hold of me)\n\n" +
            " You can dock this window AND resize it (There might be missing buttons at the bottom of this window)";
            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.TextArea(starterText);
        }

        // Display the quick setup instructions
        void QuickSetupMessage()
        {
            quickSetupTextFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(quickSetupTextFoldout, "Fastest way to get started?");
            EditorGUILayout.Space(5);
            if (quickSetupTextFoldout)
            {
                string quickSetupText = "Drag the 'APK - Entire Demo Scene Prefab' into your scene and you'll have everything to go without any setup at all, then you can remove puzzles or assets" +
                    "as required!";

                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(quickSetupText);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display the required tags and layers
        void LayersMessage()
        {
            layersTextFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(layersTextFoldout, "What Tags & Layers do I need?");
            EditorGUILayout.Space(5);
            if (layersTextFoldout)
            {
                string layersText = "Add Layers: 'ExamineLayer', 'InspectPointLayer', 'PadlockSpinner' & 'PostProcess'";
                string tagText = "Add Tags: 'InteractiveObject', 'ExaminePoint', 'InspectPoint'";

                EditorGUILayout.TextArea(tagText);
                EditorGUILayout.TextArea(layersText);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display the button prompts guide
        void ButtonPromptsMessage()
        {
            buttonPromptsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(buttonPromptsFoldout, "Button Prompts Guide");
            EditorGUILayout.Space(5);

            if (buttonPromptsFoldout)
            {
                string buttonPromptsText =
                    "Adventure Puzzle Kit uses the following default button prompts:\n\n" +
                    "• **[E]** - Interact / Pick Up\n" +
                    "• **[Q]** - Examine / Close Examine\n" +
                    "• **[Left Click]** - Rotate Examined Object\n" +
                    "• **[Right Click]** - Drop Examined or interacted Object\n" +
                    "• **[Mouse Scroll]** - Zoom Examined Object\n" +
                    "• **[G]** - Equip Gas Mask\n" +
                    "• **[T]** - Replace Gas Mask Filter\n" +
                    "• **[F]** - Toggle Flashlight\n" +
                    "• **[V]** - Reload Flashlight\n" +
                    "• **[TAB]** - Open Inventory\n" +
                    "• **[ESC]** - Close Inventory\n\n" +
                    "You can modify these controls inside the **AKInputManager** script or in Unity's Input settings.";

                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(buttonPromptsText);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display instructions for fixing post-processing errors
        void PostProcessingMessage()
        {
            postProcessingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(postProcessingFoldout, "Post Processing Error");
            EditorGUILayout.Space(5);
            if (postProcessingFoldout)
            {
                string postProcessingText = "Make sure to import post processing if you're using the gas mask system, this can be found in 'Window' > 'Package Manager' > 'Unity Registry " +
                    "Dropdown' (Top left) > Type in 'Post Processing' > Install";

                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(postProcessingText);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display instructions for fixing missing item text
        void ItemNameMessage()
        {
            itemTextFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(itemTextFoldout, "Item Text Missing?");
            EditorGUILayout.Space(5);
            if (itemTextFoldout)
            {
                string itemTextExamine = "When highlighting an item, examining or other places text appear, if text isn't visible make sure the item in question has an 'Item Name' and/or 'Font' added. " +
                    "you can find the TMP backup font in 'Additonal Packages'";

                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(itemTextExamine);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display instructions for setting up the render pipeline
        void FAQRenderPipelineMessage()
        {
            faqRenderPipelineFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(faqRenderPipelineFoldout, "How do I set this up for my render pipeline?");
            EditorGUILayout.Space(5);
            if (faqRenderPipelineFoldout)
            {
                string renderPipelineText = "Follow the online documentation (Link below) or contact me for the URP / HDRP package! :)\n\n";

                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(renderPipelineText);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display troubleshooting for pink materials or post-processing issues
        void FAQPinkMaterialsMessage()
        {
            faqPinkMaterialsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(faqPinkMaterialsFoldout, "Why do materials or post-processing look wrong or pink?");
            EditorGUILayout.Space(5);
            if (faqPinkMaterialsFoldout)
            {
                string pinkMaterialsText = "That usually means the wrong render pipeline assets were imported or your project hasn't been set up for URP or HDRP properly. Make sure your Render Pipeline Asset is assigned under\n" +
                    "Edit > Project Settings > Graphics.";

                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(pinkMaterialsText);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display guidance for using the kit in existing projects
        void FAQExistingProjectMessage()
        {
            faqExistingProjectFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(faqExistingProjectFoldout, "Can I use this in my existing project?");
            EditorGUILayout.Space(5);
            if (faqExistingProjectFoldout)
            {
                string existingProjectText = "Yep! You can import the package into any Unity project (2020.3+ recommended). Just be sure you’re using a supported render pipeline, and everything should integrate smoothly.";

                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(existingProjectText);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display information about the demo scene location
        void FAQDemoSceneMessage()
        {
            faqDemoSceneFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(faqDemoSceneFoldout, "Where’s the demo scene?");
            EditorGUILayout.Space(5);
            if (faqDemoSceneFoldout)
            {
                string demoSceneText = "You’ll find it under:\n" +
                    "Assets/Adventure Puzzle Kit/Scenes/\n\n" +
                    "It’s optional, but a great way to see the kit in action.";

                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(demoSceneText);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display information about customizing puzzle systems
        void FAQCustomizationMessage()
        {
            faqCustomizationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(faqCustomizationFoldout, "Can I customize or extend the puzzle systems?");
            EditorGUILayout.Space(5);
            if (faqCustomizationFoldout)
            {
                string customizationText = "Absolutely — the systems are modular and designed to be extended. Check the Scripts/ folder and hover over components for tooltips and comments. Documentation is included too!";

                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(customizationText);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display instructions for switching render pipelines
        void FAQSwitchingPipelinesMessage()
        {
            faqSwitchingPipelinesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(faqSwitchingPipelinesFoldout, "How do I switch pipelines after import?");
            EditorGUILayout.Space(5);
            if (faqSwitchingPipelinesFoldout)
            {
                string switchingPipelinesText = "If you've already imported a support package and want to switch:\n\n" +
                    "Re-import the AdventurePuzzleKit core package.\n" +
                    "Import the new support .unitypackage (URP, HDRP, etc.).\n\n" +
                    "Unity will replace any pipeline-specific files as needed.";

                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(switchingPipelinesText);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display guidance for mobile or VR projects
        void FAQMobileVRMessage()
        {
            faqMobileVRFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(faqMobileVRFoldout, "Can I use this in a mobile or VR project?");
            EditorGUILayout.Space(5);
            if (faqMobileVRFoldout)
            {
                string mobileVRText = "While the core systems are lightweight, performance depends on your render pipeline and scene complexity. You might need to tweak visuals or lighting setups for mobile/VR.";

                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(mobileVRText);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display compatibility information for Unity versions
        void FAQUnityVersionMessage()
        {
            faqUnityVersionFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(faqUnityVersionFoldout, "Will this work with Unity 2022 or 2023?");
            EditorGUILayout.Space(5);
            if (faqUnityVersionFoldout)
            {
                string unityVersionText = "Yes! The kit is tested with Unity 2020.3 LTS and newer. Just make sure your project is set up with a compatible render pipeline and input system.";

                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(unityVersionText);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // Display toggle for disabling automatic window opening
        void DontOpenAutomaticallyToggle()
        {
            bool dontOpenAutomatically = EditorPrefs.GetBool(DontOpenKey, false);
            dontOpenAutomatically = EditorGUILayout.Toggle("Don't open automatically", dontOpenAutomatically);
            if (GUI.changed)
            {
                EditorPrefs.SetBool(DontOpenKey, dontOpenAutomatically); // Save toggle state
            }
        }

        // Display support-related links
        void SupportLinks()
        {
            EditorGUILayout.Space(5);

            if (GUILayout.Button("Online Documentation"))
            {
                Application.OpenURL("https://speedtutoruk.gitbook.io/apk-doc-v1.7/"); // Open documentation website
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("Contact Me (Website)"))
            {
                Application.OpenURL("https://www.speed-tutor.com/pages/contact"); // Open contact page
            }
        }

        // Display useful external links
        void UsefulLinks()
        {
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Asset Store"))
            {
                Application.OpenURL("https://assetstore.unity.com/lists/speedtutor-puzzle-assets-5773131546630?aid=1101l9Bhe&utm_campaign=unity_affiliate&utm_medium=affiliate&utm_source=partnerize-linkmaker"); // Open Asset Store page
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("YouTube"))
            {
                Application.OpenURL("https://www.youtube.com/user/speedtutor"); // Open YouTube channel
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("Discord"))
            {
                Application.OpenURL("https://discord.com/invite/Dh3Kb7Z"); // Open Discord server
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("Write Review"))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/adventure-puzzle-kit-175376#reviews"); // Open review page
            }
        }

        // Display button to open debug settings
        void AddDebugSettingsButton()
        {
            EditorGUILayout.HelpBox("Use this button to adjust global debug settings like toggling debug logs.", MessageType.Info);

            if (GUILayout.Button("Open Debug Settings"))
            {
                DebugSettingsWindow.ShowWindow(); // Open debug settings window
            }
        }
    }
}