using System;
using UnityEngine;
#if UNITY_EDITOR
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

        // Implicit conversion operator: Converts SaintsArray<T> to T[]
        public static implicit operator string(SceneReference sceneReference) => sceneReference.path;
        // public static implicit operator int(SceneReference sceneReference) => sceneReference.index;

#if UNITY_EDITOR

        private bool _editorChecked;
        public void OnBeforeSerialize()
        {
            if (_editorChecked)
            {
                return;
            }

            if (string.IsNullOrEmpty(guid))
            {
                return;
            }


            if (!GUID.TryParse(guid, out GUID resultGuid))
            {
                Debug.LogWarning($"guid {guid} is not valid");
                return;
            }

            SceneAsset sceneAsset;
            try
            {
#if UNITY_6000_2_OR_NEWER
                sceneAsset = AssetDatabase.LoadAssetByGUID<SceneAsset>(resultGuid);
#else
                sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(resultGuid));
#endif
            }
            catch (UnityException)
            {
                return;
            }

            if (sceneAsset == null)
            {
                Debug.Log($"guid {guid} asset is null");
                return;
            }

            _editorChecked = true;

            int searchIndex = 0;
            foreach (EditorBuildSettingsScene editorScene in EditorBuildSettings.scenes)
            {
                if (!editorScene.enabled)
                {
                    continue;
                }

                if (editorScene.guid == resultGuid)
                {
                    index = searchIndex;
                    path = RuntimeUtil.TrimScenePath(editorScene.path, true);
                    return;
                }

                searchIndex += 1;
            }

            Debug.LogWarning($"scene {AssetDatabase.GetAssetPath(sceneAsset)} not in build list or not enabled");
        }

        public void OnAfterDeserialize()
        {
        }
#endif
    }
}
