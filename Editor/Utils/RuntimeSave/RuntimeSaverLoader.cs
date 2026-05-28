using System.Collections.Generic;
using System.Linq;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaintsField.Editor.Utils.RuntimeSave
{
    public static class RuntimeSaverLoader
    {
        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
//             if (EditorApplication.isPlayingOrWillChangePlaymode)
//             {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
//                 Debug.Log("isPlayingOrWillChangePlaymode, skip");
// #endif
//                 return;
//             }
//
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
//             Debug.Log("RuntimeSaverLoader start");
// #endif
//             RestoreRuntimeSaver();
//             ClearListAndSave();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
            Debug.Log("RuntimeSaverLoader start");
#endif
            RestoreRuntimeSaver();
            ClearListAndSave();
        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
        [MenuItem(RuntimeUtil.MenuRoot + "/DEBUG Restore RuntimeSaver")]
#endif
        private static void RestoreRuntimeSaver()
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
            Debug.Log($"RestoreRuntimeSaver count {RuntimeSaver.instance.pathSavers.Count}");
#endif
            Scene previouslyActiveScene = SceneManager.GetActiveScene();
            Dictionary<string, Scene> manuallyOpenedScenes = new Dictionary<string, Scene>();
            Dictionary<string, Component> newAddComponents = new Dictionary<string, Component>();
            foreach (IGrouping<string, PathSaver> sceneGroup in RuntimeSaver.instance.pathSavers.GroupBy(each => each.scenePath))
            {
                (bool open, Scene scene) = EnsureTargetSceneLoaded(sceneGroup.Key, manuallyOpenedScenes);
                if (!open)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
                    Debug.Log($"RestoreRuntimeSaver scene {sceneGroup.Key} not loaded, skip");
#endif
                    continue;
                }

                foreach (PathSaver pathSaver in sceneGroup.OrderByDescending(each => each.toDestroy))
                {
                    RuntimeSaverUtil.RestoreComponent(pathSaver, scene, newAddComponents);
                }
            }

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (Scene manuallyOpenedScene in manuallyOpenedScenes.Values)
            {
                if (!manuallyOpenedScene.IsValid() || !manuallyOpenedScene.isLoaded)
                {
                    continue;
                }

                EditorSceneManager.SaveScene(manuallyOpenedScene);
                EditorSceneManager.CloseScene(manuallyOpenedScene, true);
            }

            if (previouslyActiveScene.IsValid() && previouslyActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(previouslyActiveScene);
            }
        }

        private static (bool open, Scene target) EnsureTargetSceneLoaded(string scenePath, IDictionary<string, Scene> manuallyOpenedScenes)
        {
            Debug.Assert(!string.IsNullOrEmpty(scenePath));

            Scene scene = SceneManager.GetSceneByPath(scenePath);
            if (scene.IsValid() && scene.isLoaded)
            {
                return (true, scene);
            }

            if (manuallyOpenedScenes.TryGetValue(scenePath, out Scene manuallyOpenedScene))
            {
                return (true, manuallyOpenedScene);
            }

            Scene openedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            if (!openedScene.IsValid() || !openedScene.isLoaded)
            {
                Debug.LogWarning($"failed to open scene {scenePath} for runtime restore");
                return (false, default);
            }

            manuallyOpenedScenes[scenePath] = openedScene;
            return (true, openedScene);
        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
        [MenuItem(RuntimeUtil.MenuRoot + "/DEBUG Clear RuntimeSaver")]
#endif
        private static void ClearListAndSave()
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_RUNTIME_SAVER
            Debug.Log($"RestoreRuntimeSaver clear {RuntimeSaver.instance.pathSavers.Count}");
#endif
            RuntimeSaver.instance.pathSavers.Clear();
            RuntimeSaver.instance.SaveToDisk();
        }


    }
}
