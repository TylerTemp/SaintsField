using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using SaintsField.Utils;
using UnityEditor;
#endif

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Serializable]
    public struct SceneReference
#if UNITY_EDITOR
        : ISerializationCallbackReceiver
#endif
    {
        public string guid;
        public int index;
        public string path;

        public static implicit operator string(SceneReference sceneReference) => sceneReference.path;
        // public static implicit operator int(SceneReference sceneReference) => sceneReference.index;

#if UNITY_EDITOR

        private readonly struct EditorSceneCache
        {
            public readonly bool Enabled;
            public readonly int Index;
            public readonly string Path;

            public EditorSceneCache(bool enabled, int index, string path)
            {
                Enabled = enabled;
                Index = index;
                Path = path;
            }
        }

        private static readonly Dictionary<string, EditorSceneCache> EditorScenes = new Dictionary<string, EditorSceneCache>();

        [InitializeOnLoadMethod]
        private static void EditorInitialize()
        {
            EditorRefreshScenes();

            EditorBuildSettings.sceneListChanged -= EditorRefreshScenes;
            EditorBuildSettings.sceneListChanged += EditorRefreshScenes;
        }

        private static void EditorRefreshScenes()
        {
            int buildIndex = 0;

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                EditorScenes[scene.guid.ToString()] = new EditorSceneCache(
                    scene.enabled,
                    buildIndex,
                    RuntimeUtil.TrimScenePath(scene.path, true));

                if(scene.enabled)
                {
                    buildIndex += 1;
                }
            }
        }

        // OnBeforeSerialize should not call anything requires Unity main thread
        // thus we can only use callback + cache... the heck...
        public void OnBeforeSerialize()
        {
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            if (!GUID.TryParse(guid, out GUID _))
            {
                Debug.LogWarning($"scene guid {guid} is not valid");
                return;
            }

            if (!EditorScenes.TryGetValue(guid, out EditorSceneCache cached))  // maybe just early data?
            {
                return;
            }

            if (!cached.Enabled)
            {
                Debug.LogWarning($"scene is not enabled: {cached.Path}");
                // For debug reason, we dont clean the index & path data
                return;
            }

            // ReSharper disable once InvertIf
            if (index != cached.Index || path != cached.Path)
            {
                index = cached.Index;
                path = cached.Path;
            }
        }

        public void OnAfterDeserialize()
        {
        }
#endif
    }
}
