
using System;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace SaintsField.Utils
{

#if UNITY_EDITOR
    [InitializeOnLoad]
    public class SaintsFieldConfigDeleteWatcher : UnityEditor.AssetModificationProcessor
    {
        // ReSharper disable once EmptyConstructor
        static SaintsFieldConfigDeleteWatcher()
        {
        }

        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions rao)
        {
            // Debug.Log("deleted - unity callback: " + assetPath);
            // Debug.Log(rao);
            if (SaintsFieldConfigUtil.IsConfigLoaded && SaintsFieldConfigUtil.ConfigAssetPath == assetPath)
            {
                SaintsFieldConfigUtil.IsConfigLoaded = false;
                SaintsFieldConfigUtil.Config = null;
                Debug.Log($"SaintsField config deleted: {assetPath}");
            }

            return AssetDeleteResult.DidNotDelete;
        }

    }
#endif

    public static class SaintsFieldConfigUtil
    {
        public const string EditorResourcePath = "SaintsField/SaintsFieldConfig.asset";

        public static SaintsFieldConfig Config;
        public static string ConfigAssetPath = "";
        public static bool IsConfigLoaded;


#if UNITY_EDITOR
// #if UNITY_2019_2_OR_NEWER
//         [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
// #endif
        public static bool ReloadConfig()
        {
            ReloadConfigInternal();
            return IsConfigLoaded;
        }

        [InitializeOnLoadMethod]
        private static void ReloadConfigInternal()
        {
            // if (Config != null)
            // {
            //     return;
            // }

            // Debug.Log("Reload");
#if SAINTSFIELD_DEBUG
            Debug.Log("Load SaintsFieldConfig");
#endif
            try
            {
                Config = (SaintsFieldConfig)EditorGUIUtility.Load(EditorResourcePath);
            }
            catch (Exception e)
            {
                // do nothing
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(e);
#endif
            }

            IsConfigLoaded = Config != null;
            if (IsConfigLoaded)
            {
                ConfigAssetPath = AssetDatabase.GetAssetPath(Config);
            }
            else
            {
                Config = null;
                ConfigAssetPath = "";
            }
#if SAINTSFIELD_DEBUG
            Debug.Log($"SaintsField config load: {IsConfigLoaded}, {ConfigAssetPath}");
#endif
        }
#endif


        public static EXP GetComponentExp(EXP defaultValue) => IsConfigLoaded? Config.getComponentExp: defaultValue;

        public static EXP GetComponentInChildrenExp(EXP defaultValue) => IsConfigLoaded? Config.getComponentInChildrenExp: defaultValue;
        public static EXP GetComponentInParentExp(EXP defaultValue) => IsConfigLoaded? Config.getComponentInParentExp: defaultValue;
        public static EXP GetComponentInParentsExp(EXP defaultValue) => IsConfigLoaded? Config.getComponentInParentsExp: defaultValue;
        public static EXP GetComponentInSceneExp(EXP defaultValue) => IsConfigLoaded? Config.getComponentInSceneExp: defaultValue;
        public static EXP GetPrefabWithComponentExp(EXP defaultValue) => IsConfigLoaded? Config.getPrefabWithComponentExp: defaultValue;
        public static EXP GetScriptableObjectExp(EXP defaultValue) => IsConfigLoaded? Config.getScriptableObjectExp: defaultValue;
        public static EXP GetByXPathExp(EXP defaultValue) => IsConfigLoaded? Config.getByXPathExp: defaultValue;
        public static EXP GetComponentByPathExp(EXP defaultValue) => IsConfigLoaded? Config.getComponentByPathExp: defaultValue;
        public static EXP FindComponentExp(EXP defaultValue) => IsConfigLoaded? Config.findComponentExp: defaultValue;

        public static int ResizableTextAreaMinRow() => IsConfigLoaded? Config.resizableTextAreaMinRow: 3;
        public static bool DisableOnValueChangedWatchArrayFieldUIToolkit() => IsConfigLoaded && Config.disableOnValueChangedWatchArrayFieldUIToolkit;

        public static int GetByXPathDelayMs() => IsConfigLoaded? Config.getByXPathDelayMs: 0;
        public static int GetByXPathLoopIntervalMs() => IsConfigLoaded? Config.getByXPathLoopIntervalMs: SaintsFieldConfig.GetByXPathLoopIntervalDefaultMs;
        public static int GetByXPathLoopIntervalMsIMGUI() => IsConfigLoaded? Config.getByXPathLoopIntervalMsIMGUI: SaintsFieldConfig.GetByXPathLoopIntervalDefaultMsIMGUI;
        public static int GetByXPathFieldPassIMGUI() => IsConfigLoaded? Config.getByXPathFieldPassIMGUI: SaintsFieldConfig.GetByXPathDefaultFieldPassIMGUI;
        public static int GetByXPathArrayPassIMGUI() => IsConfigLoaded? Config.getByXPathArrayPassIMGUI: SaintsFieldConfig.GetByXPathDefaultArrayPassIMGUI;
    }
}
