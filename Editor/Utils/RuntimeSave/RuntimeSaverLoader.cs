using System.Collections.Generic;
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
            foreach (PathSaver pathSaver in RuntimeSaver.instance.pathSavers)
            {
                if (!EnsureTargetSceneLoaded(pathSaver, manuallyOpenedScenes))
                {
                    continue;
                }

                RuntimeSaverUtil.RestoreComponent(pathSaver);
            }

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

        private static bool EnsureTargetSceneLoaded(PathSaver pathSaver, IDictionary<string, Scene> manuallyOpenedScenes)
        {
            if (string.IsNullOrEmpty(pathSaver.scenePath))
            {
                return true;
            }

            Scene scene = SceneManager.GetSceneByPath(pathSaver.scenePath);
            if (scene.IsValid() && scene.isLoaded)
            {
                return true;
            }

            if (manuallyOpenedScenes.ContainsKey(pathSaver.scenePath))
            {
                return true;
            }

            Scene openedScene = EditorSceneManager.OpenScene(pathSaver.scenePath, OpenSceneMode.Additive);
            if (!openedScene.IsValid() || !openedScene.isLoaded)
            {
                Debug.LogWarning($"failed to open scene {pathSaver.scenePath} for runtime restore");
                return false;
            }

            manuallyOpenedScenes[pathSaver.scenePath] = openedScene;
            return true;
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
