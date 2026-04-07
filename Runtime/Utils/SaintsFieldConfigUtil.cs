#if UNITY_EDITOR

using UnityEditor;
using UnityEngine.Events;

using System.IO;
using UnityEngine;


// ReSharper disable once CheckNamespace
namespace SaintsField.Utils
{

    public static class SaintsFieldConfigUtil
    {
        public const string AssetPath = "Assets/Editor Default Resources/SaintsField/SaintsFieldConfig.asset";

        // public static SaintsFieldConfig Config;
        // private static string _configAssetPath = "";
        // public static bool IsConfigLoaded;

        public static readonly UnityEvent<SaintsFieldConfig> OnConfigLoaded = new UnityEvent<SaintsFieldConfig>();

#if UNITY_EDITOR
// #if UNITY_2019_2_OR_NEWER
//         [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
// #endif
        // public static bool ReloadConfig()
        // {
        //     ReloadConfigInternal();
        //     return IsConfigLoaded;
        // }

        [InitializeOnLoadMethod]
        private static void ReloadConfigInternal()
        {
            if (!Directory.Exists("Assets/Editor Default Resources"))
            {
                Debug.Log("Create folder: Assets/Editor Default Resources");
                AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
            }

            if (!Directory.Exists("Assets/Editor Default Resources/SaintsField"))
            {
                Debug.Log("Create folder: Assets/Editor Default Resources/SaintsField");
                AssetDatabase.CreateFolder("Assets/Editor Default Resources", "SaintsField");
            }

            if (!File.Exists(AssetPath))
            {
                AssetDatabase.CreateAsset(SaintsFieldConfig.instance, AssetPath);
                AssetDatabase.SaveAssets();
            }

            OnConfigLoaded.Invoke(SaintsFieldConfig.instance);
        }
#endif

        public static int GetFoldoutSpaceImGui() => SaintsFieldConfig.instance.foldoutSpaceImGui;

        public static EXP GetComponentExp(EXP defaultValue) => SaintsFieldConfig.instance.getComponentExpOverride? SaintsFieldConfig.instance.getComponentExp: defaultValue;

        public static EXP GetComponentInChildrenExp(EXP defaultValue) => SaintsFieldConfig.instance.getComponentInChildrenExpOverride? SaintsFieldConfig.instance.getComponentInChildrenExp: defaultValue;
        public static EXP GetComponentInParentExp(EXP defaultValue) => SaintsFieldConfig.instance.getComponentInParentExpOverride? SaintsFieldConfig.instance.getComponentInParentExp: defaultValue;
        public static EXP GetComponentInParentsExp(EXP defaultValue) => SaintsFieldConfig.instance.getComponentInParentsExpOverride? SaintsFieldConfig.instance.getComponentInParentsExp: defaultValue;
        public static EXP GetComponentInSceneExp(EXP defaultValue) => SaintsFieldConfig.instance.getComponentInSceneExpOverride? SaintsFieldConfig.instance.getComponentInSceneExp: defaultValue;
        public static EXP GetPrefabWithComponentExp(EXP defaultValue) => SaintsFieldConfig.instance.getPrefabWithComponentExpOverride? SaintsFieldConfig.instance.getPrefabWithComponentExp: defaultValue;
        public static EXP GetScriptableObjectExp(EXP defaultValue) => SaintsFieldConfig.instance.getScriptableObjectExpOverride? SaintsFieldConfig.instance.getScriptableObjectExp: defaultValue;
        public static EXP GetByXPathExp(EXP defaultValue) => SaintsFieldConfig.instance.getByXPathExpOverride? SaintsFieldConfig.instance.getByXPathExp: defaultValue;
        // ReSharper disable once UnusedParameter.Global
        public static EXP GetComponentByPathExp(EXP defaultValue) => SaintsFieldConfig.instance.getComponentByPathExp;
        // ReSharper disable once UnusedParameter.Global
        public static EXP FindComponentExp(EXP defaultValue) => SaintsFieldConfig.instance.findComponentExp;

        public static int ResizableTextAreaMinRow() => SaintsFieldConfig.instance.resizableTextAreaMinRowOverride? SaintsFieldConfig.instance.resizableTextAreaMinRow: SaintsFieldConfig.ResizableTextAreaMinRowDefault;
        public static bool DisableOnValueChangedWatchArrayFieldUIToolkit() => SaintsFieldConfig.instance.disableOnValueChangedWatchArrayFieldUIToolkit;

        // public static int GetByXPathDelayMs() => IsConfigLoaded? Config.getByXPathDelayMs: 0;
        // public static int GetByXPathLoopIntervalMs() => IsConfigLoaded? Config.getByXPathLoopIntervalMs: SaintsFieldConfig.GetByXPathLoopIntervalDefaultMs;
        // public static int GetByXPathLoopIntervalMsIMGUI() => IsConfigLoaded? Config.getByXPathLoopIntervalMsIMGUI: SaintsFieldConfig.GetByXPathLoopIntervalDefaultMsIMGUI;
        // public static int GetByXPathFieldPassIMGUI() => IsConfigLoaded? Config.getByXPathFieldPassIMGUI: SaintsFieldConfig.GetByXPathDefaultFieldPassIMGUI;
        // public static int GetByXPathArrayPassIMGUI() => IsConfigLoaded? Config.getByXPathArrayPassIMGUI: SaintsFieldConfig.GetByXPathDefaultArrayPassIMGUI;

        // ReSharper disable once SimplifyConditionalTernaryExpression
        public static bool GetValidateInputLoopCheckUIToolkit() => SaintsFieldConfig.instance.validateInputLoopCheckUIToolkit;
        public static bool GetMonoBehaviorSearchable()
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (SaintsFieldConfig.instance.monoBehaviorSearchableOverride)
            {
                return SaintsFieldConfig.instance.monoBehaviorSearchable;
            }

            return SaintsFieldConfig.MonoBehaviorSearchableDefault;
        }
    }
}
#endif
