using UnityEditor;
using UnityEngine;
using AdventurePuzzleKit.ChessSystem;
using AdventurePuzzleKit.ThemedKey;
using AdventurePuzzleKit.ValveSystem;

namespace AdventurePuzzleKit
{
    public static class ScriptableObjectMenu
    {
        [MenuItem("APK/Create/ScriptableObject/Chess Piece")]
        public static void CreateChessPiece()
        {
            CreateAsset<ChessPiece>("NewChessPiece");
        }

        [MenuItem("APK/Create/ScriptableObject/Key")]
        public static void CreateKey()
        {
            CreateAsset<Key>("NewKey");
        }

        [MenuItem("APK/Create/ScriptableObject/Sound")]
        public static void CreateSound()
        {
            CreateAsset<Sound>("NewSound");
        }

        [MenuItem("APK/Create/ScriptableObject/Valve")]
        public static void CreateValve()
        {
            CreateAsset<Valve>("NewValve");
        }

        private static void CreateAsset<T>(string defaultName) where T : ScriptableObject
        {
            string path = EditorUtility.SaveFilePanelInProject(
                $"Save {typeof(T).Name}",
                defaultName,
                "asset",
                "Please enter a file name to save the ScriptableObject."
            );

            if (string.IsNullOrEmpty(path)) return;

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}

