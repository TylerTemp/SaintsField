#if UNITY_EDITOR
using UnityEditor;

namespace SaintsField.Utils
{
    public static class SaintsFieldConfigUtil
    {
        public const string EditorResourcePath = "SaintsField/SaintsFieldConfig.asset";

        public static SaintsFieldConfig Config;

        public static SaintsFieldConfig GetConfig()
        {
            if (Config == null)
            {
                Config = (SaintsFieldConfig)EditorGUIUtility.Load(EditorResourcePath);
            }

            return Config;
        }

        public static EXP GetComponentExp(EXP defaultValue) => GetConfig()?.getComponentExp ?? defaultValue;
    }
}
#endif
