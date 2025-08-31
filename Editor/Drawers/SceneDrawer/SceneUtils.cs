using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
    public static class SceneUtils
    {
        public static IEnumerable<string> GetTrimedScenePath(bool fullPath) =>
            GetScenePath().Select(scenePath => TrimScenePath(scenePath, fullPath));

        public static void OpenBuildSettings()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        }

        /// <summary>
        ///  <see href="https://github.com/dbrizov/NaughtyAttributes/blob/a97aa9b3b416e4c9122ea1be1a1b93b1169b0cd3/Assets/NaughtyAttributes/Scripts/Editor/PropertyDrawers/ScenePropertyDrawer.cs#L10" />
        /// </summary>
        private static IEnumerable<string> GetScenePath() =>
            EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                // .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .Select(scene => scene.path) ;

        private static string TrimScenePath(string scenePath, bool fullPath)
        {
            string preTrimScenePath = scenePath;
            if(preTrimScenePath.StartsWith("/Assets/"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                preTrimScenePath = preTrimScenePath.Substring("/Assets/".Length);
            }
            else if(preTrimScenePath.StartsWith("Assets/"))
            {
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                preTrimScenePath = preTrimScenePath.Substring("Assets/".Length);
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            // ReSharper disable once InvertIf
            if (preTrimScenePath.EndsWith(".unity"))
            {
                if(fullPath)
                {
                    // ReSharper disable once ReplaceSubstringWithRangeIndexer
                    return preTrimScenePath.Substring(0, preTrimScenePath.Length - ".unity".Length);
                }
                return Path.GetFileNameWithoutExtension(preTrimScenePath);
            }

            return fullPath? preTrimScenePath : Path.GetFileNameWithoutExtension(preTrimScenePath);
        }


    }
}
