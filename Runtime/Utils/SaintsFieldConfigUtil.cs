#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SaintsField.Utils
{
    public static class SaintsFieldConfigUtil
    {
        public const string EditorResourcePath = "SaintsField/SaintsFieldConfig.asset";

        public static SaintsFieldConfig Config;

        public static SaintsFieldConfig GetConfig()
        {
#if UNITY_EDITOR
            if (Config == null)
            {
                Config = (SaintsFieldConfig)EditorGUIUtility.Load(EditorResourcePath);
            }
#endif

            return Config;
        }

        public static EXP GetComponentExp(EXP defaultValue) => GetConfig()?.getComponentExp ?? defaultValue;

        public static EXP GetComponentInChildrenExp(EXP defaultValue) => GetConfig()?.getComponentInChildrenExp ?? defaultValue;
        public static EXP GetComponentInParentExp(EXP defaultValue) => GetConfig()?.getComponentInParentExp ?? defaultValue;
        public static EXP GetComponentInParentsExp(EXP defaultValue) => GetConfig()?.getComponentInParentsExp ?? defaultValue;
        public static EXP GetComponentInSceneExp(EXP defaultValue) => GetConfig()?.getComponentInSceneExp ?? defaultValue;
        public static EXP GetPrefabWithComponentExp(EXP defaultValue) => GetConfig()?.getPrefabWithComponentExp ?? defaultValue;
        public static EXP GetScriptableObjectExp(EXP defaultValue) => GetConfig()?.getScriptableObjectExp ?? defaultValue;
        public static EXP GetByXPathExp(EXP defaultValue) => GetConfig()?.getByXPathExp ?? defaultValue;
        public static EXP GetComponentByPathExp(EXP defaultValue) => GetConfig()?.getComponentByPathExp ?? defaultValue;
        public static EXP FindComponentExp(EXP defaultValue) => GetConfig()?.findComponentExp ?? defaultValue;
    }
}
