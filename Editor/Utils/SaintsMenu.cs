using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class SaintsMenu
    {
        // private readonly static BuildTargetGroup[] _buildingGroups = new BuildTargetGroup[] { BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.Standalone, Bu };
#if !SAINTSFIELD_UI_TOOLKIT_DISABLE
        [MenuItem("Window/Saints/Disable UI Toolkit")]
        public static void UIToolkit() => AddCompileDefine("SAINTSFIELD_UI_TOOLKIT_DISABLE");
#endif

#if SAINTSFIELD_UI_TOOLKIT_DISABLE
        [MenuItem("Window/Saints/Enable UI Toolkit Support")]
        public static void UIToolkit() => RemoveCompileDefine("SAINTSFIELD_UI_TOOLKIT_DISABLE");
#endif

#if !SAINTSFIELD_DOTWEEN
        [MenuItem("Window/Saints/Enable DOTween Support")]
        public static void DOTween() => AddCompileDefine("SAINTSFIELD_DOTWEEN");
#endif

#if SAINTSFIELD_ADDRESSABLE
#if SAINTSFIELD_DOTWEEN
        [MenuItem("Window/Saints/Disable DOTween Support")]
        public static void DOTween() => RemoveCompileDefine("SAINTSFIELD_DOTWEEN");
#endif

#if !SAINTSFIELD_ADDRESSABLE_DISABLE
        [MenuItem("Window/Saints/Disable Addressable Support")]
        public static void Addressable() => AddCompileDefine("SAINTSFIELD_ADDRESSABLE_DISABLE");
#endif

#if SAINTSFIELD_ADDRESSABLE_DISABLE
        [MenuItem("Window/Saints/Enable Addressable Support")]
        public static void Addressable() => RemoveCompileDefine("SAINTSFIELD_ADDRESSABLE_DISABLE");
#endif

#else
        [MenuItem("Window/Saints/Addressable Not Installed")]
        public static void AddressableNotInstalled() { }
        [MenuItem("Window/Saints/Addressable Not Installed", true)]
        public static bool AddressableNotInstalledDisabled() => false;
#endif

        // ReSharper disable once UnusedMember.Local
        private static void AddCompileDefine(string newDefineCompileConstant, IEnumerable<BuildTargetGroup> targetGroups = null)
        {
            IEnumerable<BuildTargetGroup> targets = targetGroups ?? Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>();

            foreach (BuildTargetGroup grp in targets.Where(each => each != BuildTargetGroup.Unknown))
            {
                string defines;
                try
                {
                    defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
                }
                catch (ArgumentException)
                {
                    continue;
                }
                if (!defines.Contains(newDefineCompileConstant))
                {
                    if (defines.Length > 0)
                        defines += ";";

                    defines += newDefineCompileConstant;
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, defines);
                }
            }
        }

        // ReSharper disable once UnusedMember.Local
        private static void RemoveCompileDefine(string defineCompileConstant, IEnumerable<BuildTargetGroup> targetGroups = null)
        {
            IEnumerable<BuildTargetGroup> targets = targetGroups ?? Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>();

            foreach (BuildTargetGroup grp in targets.Where(each => each != BuildTargetGroup.Unknown))
            {
                string defines;
                try
                {
                    defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                string result = string.Join(";", defines
                    .Split(';')
                    .Select(each => each.Trim())
                    .Where(each => each != defineCompileConstant));

                // Debug.Log(result);

                PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, result);
            }
        }
    }
}
