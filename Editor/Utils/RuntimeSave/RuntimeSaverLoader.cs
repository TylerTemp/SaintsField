using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

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
            foreach (PathSaver pathSaver in RuntimeSaver.instance.pathSavers)
            {
                RuntimeSaverUtil.RestoreComponent(pathSaver);
            }
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
