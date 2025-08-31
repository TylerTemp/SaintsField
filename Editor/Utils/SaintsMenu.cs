﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsField.Editor.ColorPalette;
using SaintsField.Editor.I2Setup;
using SaintsField.Utils;
using UnityEditor;
#if UNITY_2023_1_OR_NEWER
using UnityEditor.Build;
#endif
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class SaintsMenu
    {

        #region Config

#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/Create or Edit SaintsField Config")]
#else
        [MenuItem("Window/Saints/Create or Edit SaintsField Config")]
#endif
        public static void CreateOrEditSaintsFieldConfig()
        {
            SaintsFieldConfig saintsFieldConfig;
            if (SaintsFieldConfigUtil.ReloadConfig())
            {
                saintsFieldConfig = SaintsFieldConfigUtil.Config;
            }
            else
            {
                if (!Directory.Exists("Assets/Editor Default Resources"))
                {
                    Debug.Log($"Create folder: Assets/Editor Default Resources");
                    AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
                }

                if (!Directory.Exists("Assets/Editor Default Resources/SaintsField"))
                {
                    Debug.Log($"Create folder: Assets/Editor Default Resources/SaintsField");
                    AssetDatabase.CreateFolder("Assets/Editor Default Resources", "SaintsField");
                }

                saintsFieldConfig = ScriptableObject.CreateInstance<SaintsFieldConfig>();
                Debug.Log(
                    $"Create saintsFieldConfig: Assets/Editor Default Resources/{SaintsFieldConfigUtil.EditorResourcePath}");
                AssetDatabase.CreateAsset(saintsFieldConfig,
                    $"Assets/Editor Default Resources/{SaintsFieldConfigUtil.EditorResourcePath}");
                AssetDatabase.SaveAssets();

                SaintsFieldConfigUtil.ReloadConfig();
                saintsFieldConfig = SaintsFieldConfigUtil.Config;
            }

            Selection.activeObject = saintsFieldConfig;
        }

        #endregion


        #region UI Toolkit
#if SAINTSFIELD_UI_TOOLKIT_DISABLE
        [MenuItem("Window/Saints/Enable UI Toolkit Support")]
        public static void UIToolkit() => RemoveCompileDefine("SAINTSFIELD_UI_TOOLKIT_DISABLE");
#else
        [MenuItem("Window/Saints/Disable UI Toolkit Support")]
        public static void UIToolkit() => AddCompileDefine("SAINTSFIELD_UI_TOOLKIT_DISABLE");
#endif

        #region IMGUI decorators

#if SAINTSFIELD_IMGUI_DUPLICATE_DECORATOR_FIX
        [MenuItem("Window/Saints/Disable IMGUI duplicated decorator fix")]
        public static void ImGuiDuplicateDecoratorFix() => RemoveCompileDefine("SAINTSFIELD_IMGUI_DUPLICATE_DECORATOR_FIX");
#else
        [MenuItem("Window/Saints/Enable IMGUI duplicated decorator fix")]
        public static void ImGuiDuplicateDecoratorFix() => AddCompileDefine("SAINTSFIELD_IMGUI_DUPLICATE_DECORATOR_FIX");
#endif

        #endregion
//         #region Label Fix (UI Toolkit)
// #if !SAINTSFIELD_UI_TOOLKIT_DISABLE
// #if !SAINTSFIELD_UI_TOOLKIT_LABEL_FIX_DISABLE
//         [MenuItem("Window/Saints/Disable UI Toolkit Label Fix")]
//         public static void UIToolkitLabelFix() => AddCompileDefine("SAINTSFIELD_UI_TOOLKIT_LABEL_FIX_DISABLE");
// #else
//         [MenuItem("Window/Saints/Enable UI Toolkit Label Fix")]
//         public static void UIToolkitLabelFix() => RemoveCompileDefine("SAINTSFIELD_UI_TOOLKIT_LABEL_FIX_DISABLE");
// #endif
// #endif
//         #endregion

        #endregion

        #region SaintsEditor

#if SAINTSFIELD_SAINTS_EDITOR_APPLY
        [MenuItem("Window/Saints/SaintsEditor/Disable SaintsEditor")]
        public static void SaintsEditorUnapply() => RemoveCompileDefine("SAINTSFIELD_SAINTS_EDITOR_APPLY");

        #region IMGUI Constant Repaint
#if SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE
        [MenuItem("Window/Saints/SaintsEditor/Enable IMGUI Constant Repaint")]
        public static void SaintsEditorIMGUIConstantRepaint() => RemoveCompileDefine("SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE");
#else
        [MenuItem("Window/Saints/SaintsEditor/Disable IMGUI Constant Repaint")]
        public static void SaintsEditorIMGUIConstantRepaint() => AddCompileDefine("SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE");
#endif
        #endregion
#else
        [MenuItem("Window/Saints/Enable SaintsEditor")]
        public static void ApplySaintsEditor() => AddCompileDefine("SAINTSFIELD_SAINTS_EDITOR_APPLY");
#endif

        #endregion

        #region DOTween

#if DOTWEEN

#if SAINTSFIELD_DOTWEEN_DISABLED
        [MenuItem("Window/Saints/Enable DOTween Support")]
        public static void DOTween() => RemoveCompileDefine("SAINTSFIELD_DOTWEEN_DISABLED");
#else
        [MenuItem("Window/Saints/Disable DOTween Support")]
        public static void DOTween() => AddCompileDefine("SAINTSFIELD_DOTWEEN_DISABLED");
#endif  // SAINTSFIELD_DOTWEEN_DISABLED

#else
        [MenuItem("Window/Saints/DOTween Not Installed")]
        public static void DOTweenNotInstalled() { }
        [MenuItem("Window/Saints/DOTween Not Installed", true)]
        public static bool DOTweenNotInstalledEnabled() => false;
#endif  // DOTWEEN

        #endregion

        #region Addressable
#if SAINTSFIELD_ADDRESSABLE

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
        public static bool AddressableNotInstalledEnabled() => false;
#endif
        #endregion

        #region AI Navigation

        // && !SAINTSFIELD_AI_NAVIGATION_DISABLED
#if SAINTSFIELD_AI_NAVIGATION

#if !SAINTSFIELD_AI_NAVIGATION_DISABLED
        [MenuItem("Window/Saints/Disable AI Navigation Support")]
        public static void AiNavigation() => AddCompileDefine("SAINTSFIELD_AI_NAVIGATION_DISABLED");
#endif  // !SAINTSFIELD_AI_NAVIGATION_DISABLED

#if SAINTSFIELD_AI_NAVIGATION_DISABLED
        [MenuItem("Window/Saints/Enable AI Navigation Support")]
        public static void AiNavigation() => RemoveCompileDefine("SAINTSFIELD_AI_NAVIGATION_DISABLED");
#endif  // SAINTSFIELD_AI_NAVIGATION_DISABLED

#else   // SAINTSFIELD_AI_NAVIGATION
        [MenuItem("Window/Saints/AI Navigation Not Installed")]
        public static void AiNavigationNotInstalled() { }
        [MenuItem("Window/Saints/AI Navigation Not Installed", true)]
        public static bool AiNavigationNotInstalledEnabled() => false;
#endif  // SAINTSFIELD_AI_NAVIGATION

        #endregion

        #region I2Loc

#if SAINTSFIELD_I2_LOC
        [MenuItem("Window/Saints/Disable I2 Localization Support")]
        public static void I2Localization() => RemoveCompileDefine("SAINTSFIELD_I2_LOC");
#else
        [MenuItem("Window/Saints/Enable I2 Localization Support")]
        public static void I2Localization() => I2SetupWindow.OpenWindow();
#endif

        #endregion

        #region Header GUI

#if SAINTSFIELD_HEADER_GUI
        [MenuItem("Window/Saints/Disable Stand-Alone Header GUI Support")]
        public static void HeaderGUI() => RemoveCompileDefine("SAINTSFIELD_HEADER_GUI");
#else
        [MenuItem("Window/Saints/Enable Stand-Alone Header GUI Support")]
        public static void HeaderGUI() => AddCompileDefine("SAINTSFIELD_HEADER_GUI");
#endif


        #endregion

        #region Color Palette

#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/Color Palette...")]
#else
        [MenuItem("Window/Saints/Color Palette...")]
#endif
        public static void ColorPaletteMenu()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(ColorPaletteArray).FullName);
            if (guids.Length > 0)
            {
                OpenColorPaletteInstance(
                    AssetDatabase.LoadAssetAtPath<ColorPaletteArray>(AssetDatabase.GUIDToAssetPath(guids[0])));
                return;
            }

            bool result = EditorUtility.DisplayDialog(
                "Create Color Palette",
                "You don't have a Color Palette in your project. Create One?",
                "Create",
                "Cancel"
            );

            if (result)
            {
                // Debug.Log("User clicked OK");
                CreateAndOpenColorPalette();
            }
        }

        private static void CreateAndOpenColorPalette()
        {
            if (!Directory.Exists("Assets/Editor Default Resources"))
            {
                Debug.Log($"Create folder: Assets/Editor Default Resources");
                AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
            }

            if (!Directory.Exists("Assets/Editor Default Resources/SaintsField"))
            {
                Debug.Log($"Create folder: Assets/Editor Default Resources/SaintsField");
                AssetDatabase.CreateFolder("Assets/Editor Default Resources", "SaintsField");
            }

            ColorPaletteArray colorPaletteArray = ScriptableObject.CreateInstance<ColorPaletteArray>();
            colorPaletteArray.colorInfoArray = new[]
            {
                new ColorPaletteArray.ColorInfo
                {
                    color = Color.black,
                    labels = new[] { "Your Label" },
                },
            };

            string path = EditorUtility.SaveFilePanelInProject("Create Color Palette", "ColorPalette", "asset", "Create a Color Palette in this project folder","Assets/Editor Default Resources/SaintsField");
            Debug.Log($"Create ColorPaletteArray: {path}");
            AssetDatabase.CreateAsset(colorPaletteArray, path);

            OpenColorPaletteInstance(AssetDatabase.LoadAssetAtPath<ColorPaletteArray>(path));
            // AssetDatabase.SaveAssets();
        }

        private static void OpenColorPaletteInstance(ColorPaletteArray colorPaletteArray)
        {
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
            ColorPaletteEditorWindow window = ColorPaletteEditorWindow.OpenColorPaletteEditorWindow();
            // window.EditorInspectingTarget = colorPaletteArray;
            window.ColorPaletteArrayChanged(window.colorPaletteArray = colorPaletteArray);
            window.Show();
#else
            Selection.activeObject = colorPaletteArray;
#endif
        }

        #endregion

        // ReSharper disable once UnusedMember.Local
        public static void AddCompileDefine(string newDefineCompileConstant, IEnumerable<BuildTargetGroup> targetGroups = null)
        {
            IEnumerable<BuildTargetGroup> targets = targetGroups ?? Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>();

            foreach (BuildTargetGroup grp in targets.Where(each => each != BuildTargetGroup.Unknown))
            {
                string defines;
                try
                {
#if UNITY_2023_1_OR_NEWER
                    defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(grp));
#else
                    defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
#endif
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
                    try
                    {
#if UNITY_2023_1_OR_NEWER
                    PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(grp), defines);
#else
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, defines);
#endif
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
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
#if UNITY_2023_1_OR_NEWER
                    defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(grp));
#else
                    defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
#endif
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

                try
                {
#if UNITY_2023_1_OR_NEWER
                    PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(grp), result);
#else
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, result);
#endif
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}
