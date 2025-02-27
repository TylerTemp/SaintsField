using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SceneAttribute), true)]
    public partial class SceneAttributeDrawer : SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        /// <summary>
        ///  <see href="https://github.com/dbrizov/NaughtyAttributes/blob/a97aa9b3b416e4c9122ea1be1a1b93b1169b0cd3/Assets/NaughtyAttributes/Scripts/Editor/PropertyDrawers/ScenePropertyDrawer.cs#L10" />
        /// </summary>
        private static string[] GetScenePath() =>
            EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                // .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .Select(scene => scene.path)
                .ToArray();

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

        private static string[] GetTrimedScenePath(bool fullPath) =>
            GetScenePath().Select(scenePath => TrimScenePath(scenePath, fullPath)).ToArray();

        private static void OpenBuildSettings()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {

            string[] scenes = GetTrimedScenePath(((SceneAttribute)propertyAttribute).FullPath);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                {
                    if(Array.IndexOf(scenes, property.stringValue) == -1)
                    {
                        return new AutoRunnerFixerResult
                        {
                            ExecError = "",
                            Error = $"{property.name} must be a scene in build settings, get {property.stringValue}",
                        };
                    }

                    return null;
                }
                case SerializedPropertyType.Integer:
                {
                    if (property.intValue < 0 || property.intValue >= scenes.Length)
                    {
                        return new AutoRunnerFixerResult
                        {
                            ExecError = "",
                            Error = $"{property.name} must be a scene in build settings (${scenes.Length}), get {property.intValue}",
                        };
                    }

                    return null;
                }
                default:
                    return new AutoRunnerFixerResult
                    {
                        ExecError = "",
                        Error = $"{property.name} must be an int or a string, get {property.propertyType}",
                    };
            }
        }
    }
}
