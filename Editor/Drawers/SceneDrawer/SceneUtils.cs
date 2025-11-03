using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsField.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
    public static class SceneUtils
    {
        public static IEnumerable<string> GetTrimedScenePath(bool fullPath) =>
            GetScenePath().Select(scenePath => RuntimeUtil.TrimScenePath(scenePath, fullPath));

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
    }
}
