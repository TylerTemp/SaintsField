#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SceneReferenceTypeDrawer
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public partial class SceneReferenceDrawer: SaintsPropertyDrawer
    {
        private const string PropNameGuid = nameof(SceneReference.guid);
        private const string PropNamePath = nameof(SceneReference.path);
        private const string PropNameIndex = nameof(SceneReference.index);

        internal enum SceneReferenceHelpAction
        {
            None,
            Enable,
            Add,
        }

        internal readonly struct SceneReferenceContext
        {
            public readonly string Error;
            public readonly SerializedProperty GuidProp;
            public readonly SerializedProperty PathProp;
            public readonly SerializedProperty IndexProp;

            public SceneReferenceContext(string error, SerializedProperty guidProp, SerializedProperty pathProp,
                SerializedProperty indexProp)
            {
                Error = error;
                GuidProp = guidProp;
                PathProp = pathProp;
                IndexProp = indexProp;
            }
        }

        internal readonly struct SceneReferenceState
        {
            public readonly string Error;
            public readonly SceneReferenceHelpAction HelpAction;
            public readonly string ActionPath;
            public readonly string ActionGuid;
            public readonly SceneAsset SceneAsset;
            public readonly string ScenePath;
            public readonly string TrimmedScenePath;
            public readonly int SceneIndex;
            public readonly bool IsValidEnabledScene;

            public SceneReferenceState(string error, SceneReferenceHelpAction helpAction, string actionPath,
                string actionGuid, SceneAsset sceneAsset, string scenePath, string trimmedScenePath, int sceneIndex,
                bool isValidEnabledScene)
            {
                Error = error;
                HelpAction = helpAction;
                ActionPath = actionPath;
                ActionGuid = actionGuid;
                SceneAsset = sceneAsset;
                ScenePath = scenePath;
                TrimmedScenePath = trimmedScenePath;
                SceneIndex = sceneIndex;
                IsValidEnabledScene = isValidEnabledScene;
            }
        }

        internal readonly struct SceneReferencePayload
        {
            public readonly string Guid;
            public readonly string Name;
            public readonly int Index;
            public readonly bool IsAction;

            public SceneReferencePayload(string guid, string name, int index, bool isAction = false)
            {
                Guid = guid;
                Name = name;
                Index = index;
                IsAction = isAction;
            }
        }

        internal static (string error, SceneReferenceContext context) GetSceneReferenceContext(
            SerializedProperty property)
        {
            SerializedProperty sceneGuidProp = property.FindPropertyRelative(PropNameGuid);
            if (sceneGuidProp == null)
            {
                string error = $"{PropNameGuid} not found in {property.propertyPath}";
                return (error, new SceneReferenceContext(error, null, null, null));
            }

            SerializedProperty scenePathProp = property.FindPropertyRelative(PropNamePath);
            if (scenePathProp == null)
            {
                string error = $"{PropNamePath} not found in {property.propertyPath}";
                return (error, new SceneReferenceContext(error, sceneGuidProp, null, null));
            }

            SerializedProperty sceneIndexProp = property.FindPropertyRelative(PropNameIndex);
            if (sceneIndexProp == null)
            {
                string error = $"{PropNameIndex} not found in {property.propertyPath}";
                return (error, new SceneReferenceContext(error, sceneGuidProp, scenePathProp, null));
            }

            return ("", new SceneReferenceContext("", sceneGuidProp, scenePathProp, sceneIndexProp));
        }

        internal static SceneReferenceState GetSceneReferenceState(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return new SceneReferenceState("Guid is empty", SceneReferenceHelpAction.None, "", "", null, "", "",
                    -1, false);
            }

            if (!GUID.TryParse(guid, out GUID guidResult))
            {
                return new SceneReferenceState($"Invalid guid {guid}", SceneReferenceHelpAction.None, "", "", null,
                    "", "", -1, false);
            }

            SceneAsset sceneAsset = LoadSceneAsset(guidResult);
            if (sceneAsset == null)
            {
                return new SceneReferenceState($"Guid {guidResult} does not exists or is not SceneAsset",
                    SceneReferenceHelpAction.None, "", guid, null, "", "", -1, false);
            }

            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            string trimmedScenePath = RuntimeUtil.TrimScenePath(scenePath, true);
            int enabledIndex = 0;
            foreach (EditorBuildSettingsScene inBuild in EditorBuildSettings.scenes)
            {
                if (inBuild.path == scenePath)
                {
                    if (!inBuild.enabled)
                    {
                        return new SceneReferenceState($"{inBuild.path} not enabled",
                            SceneReferenceHelpAction.Enable, inBuild.path, guid, sceneAsset, scenePath,
                            trimmedScenePath, -1, false);
                    }

                    return new SceneReferenceState("", SceneReferenceHelpAction.None, "", guid, sceneAsset, scenePath,
                        trimmedScenePath, enabledIndex, true);
                }

                if (inBuild.enabled)
                {
                    enabledIndex++;
                }
            }

            return new SceneReferenceState($"{trimmedScenePath} not in build list", SceneReferenceHelpAction.Add,
                scenePath, guid, sceneAsset, scenePath, trimmedScenePath, -1, false);
        }

        internal static SceneAsset LoadSceneAsset(GUID guid)
        {
#if UNITY_6000_2_OR_NEWER
            return AssetDatabase.LoadAssetByGUID<SceneAsset>(guid);
#else
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(guid));
#endif
        }

        internal static string GetGuidFromSceneAsset(SceneAsset sceneAsset) =>
            sceneAsset == null ? "" : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sceneAsset));

        internal static IReadOnlyList<SceneReferencePayload> GetSceneReferencePayloads()
        {
            List<SceneReferencePayload> result = new List<SceneReferencePayload>();
            foreach ((EditorBuildSettingsScene editorScene, int index) in EditorBuildSettings.scenes.Where(each => each.enabled).WithIndex())
            {
                result.Add(new SceneReferencePayload(
                    AssetDatabase.GUIDFromAssetPath(editorScene.path).ToString(),
                    RuntimeUtil.TrimScenePath(editorScene.path, true),
                    index));
            }

            return result;
        }

        internal static bool RefreshGuid(SceneReferenceContext context)
        {
            if (context.Error != "")
            {
                return false;
            }

            SceneReferenceState state = GetSceneReferenceState(context.GuidProp.stringValue);
            if (!state.IsValidEnabledScene)
            {
                return false;
            }

            bool changed = false;
            if (context.PathProp.stringValue != state.TrimmedScenePath)
            {
                context.PathProp.stringValue = state.TrimmedScenePath;
                changed = true;
            }

            if (context.IndexProp.intValue != state.SceneIndex)
            {
                context.IndexProp.intValue = state.SceneIndex;
                changed = true;
            }

            if (changed)
            {
                context.GuidProp.serializedObject.ApplyModifiedProperties();
            }

            return changed;
        }

        internal static SceneReference ApplySceneReferenceGuid(SceneReferenceContext context, string guid)
        {
            context.GuidProp.stringValue = guid;

            SceneReferenceState state = GetSceneReferenceState(guid);
            if (state.IsValidEnabledScene)
            {
                context.PathProp.stringValue = state.TrimmedScenePath;
                context.IndexProp.intValue = state.SceneIndex;
            }

            context.GuidProp.serializedObject.ApplyModifiedProperties();
            return GetCurrentValue(context);
        }

        internal static SceneReference GetCurrentValue(SceneReferenceContext context) => new SceneReference
        {
            guid = context.GuidProp.stringValue,
            path = context.PathProp.stringValue,
            index = context.IndexProp.intValue,
        };

        internal static bool EnableScenePath(string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            int index = Array.FindIndex(scenes, each => each.path == scenePath);
            if (index == -1 || scenes[index].enabled)
            {
                return false;
            }

            scenes[index].enabled = true;
            EditorBuildSettings.scenes = scenes;
            return true;
        }

        internal static bool AddScenePath(string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (Array.FindIndex(scenes, each => each.path == scenePath) != -1)
            {
                return false;
            }

            EditorBuildSettings.scenes = scenes.Append(new EditorBuildSettingsScene(scenePath, true)).ToArray();
            return true;
        }
    }
}
#endif
