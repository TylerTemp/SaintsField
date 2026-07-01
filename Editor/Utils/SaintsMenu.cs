#if (WWISE_2030_OR_LATER || WWISE_2029_OR_LATER || WWISE_2028_OR_LATER || WWISE_2027_OR_LATER || WWISE_2026_OR_LATER || WWISE_2025_OR_LATER || WWISE_2024_OR_LATER || WWISE_2023_OR_LATER || WWISE_2022_OR_LATER || WWISE_2021_OR_LATER || WWISE_2020_OR_LATER || WWISE_2019_OR_LATER || WWISE_2018_OR_LATER || WWISE_2017_OR_LATER || WWISE_2016_OR_LATER || SAINTSFIELD_WWISE)
#define WWISE_INSTALLED
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsField.Editor.ColorPalette;
using SaintsField.Editor.TroubleshootEditor;
#if !SAINTSFIELD_I2_LOC
using SaintsField.Editor.I2Setup;
#endif
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.Compilation;
#if UNITY_2023_1_OR_NEWER
using UnityEditor.Build;
#endif
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public static class SaintsMenu
    {

        #region SaintsEditor

        // ReSharper disable once InconsistentNaming
        public const string SAINTSFIELD_SAINTS_EDITOR_APPLY = "SAINTSFIELD_SAINTS_EDITOR_APPLY";
        private const string SaintsEditorApplyPath = RuntimeUtil.MenuRoot + "Enable SaintsEditor";
        [MenuItem(SaintsEditorApplyPath, priority = 1)]
        public static void SaintsEditorApply()
        {
#if SAINTSFIELD_SAINTS_EDITOR_APPLY
            RemoveCompileDefine("SAINTSFIELD_SAINTS_EDITOR_APPLY");
#else
            SaintsFieldSetupWindow.Open();
#endif
        }

        #region IMGUI Constant Repaint
        private const string DisableIMGUIConstantRepaintPath = RuntimeUtil.MenuRoot + "Disable IMGUI Constant Repaint";
        [MenuItem(DisableIMGUIConstantRepaintPath, priority = 2)]
        public static void DisableIMGUIConstantRepaint()
        {
#if SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE
            RemoveCompileDefine
#else

            AddCompileDefine
#endif
                ("SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE");
        }

        [MenuItem(DisableIMGUIConstantRepaintPath, true)]
        public static bool DisableIMGUIConstantRepaintValidate() =>
#if SAINTSFIELD_SAINTS_EDITOR_APPLY
            true
#else

            false
#endif
        ;

        #endregion

// #endif

        #endregion

        #region Config

        [MenuItem(RuntimeUtil.MenuRoot + "Edit Config...", priority = 3)]
        private static void CreateOrEditSaintsFieldConfig()
        {
            Selection.activeObject = EnsureCreateSaintsFieldConfig();
        }

        public static SaintsFieldConfig EnsureCreateSaintsFieldConfig()
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

            return saintsFieldConfig;
        }

        #endregion

        [MenuItem(RuntimeUtil.MenuRoot + "Troubleshoot...", priority = 4)]
        private static void Troubleshoot()
        {
            TroubleshootEditorWindow.Open();
        }

        #region DOTween

        // ReSharper disable once InconsistentNaming
        private const string EnableDOTweenPath = RuntimeUtil.MenuRoot + "Enable DOTween Support";
        // ReSharper disable once InconsistentNaming
        [MenuItem(EnableDOTweenPath, priority = 100)]
        public static void EnableDOTween()
        {
            const string enableMarco = "SAINTSFIELD_DOTWEEN_ENABLE";
#if SAINTSFIELD_DOTWEEN_ENABLE && false
            RemoveCompileDefine(enableMarco);
#else
            // DOTween/Modules/DOTween.Modules.asmdef
            if(IsDoTweenSetup())
            {
                AddCompileDefine(enableMarco);
            }
            else
            {
                // ReSharper disable once InvertIf
                if (EditorUtility.DisplayDialog(
                        "DOTween Modules not enabled",
                        "You need to setup a DOTween ASMDEF to use this function.\nPlease go Tools/Demigiant/DOTween Utility Panel, click \"Create ASMDEF...\"",
                        "Open",
                        "Cancel"))
                {
                    if (EditorApplication.ExecuteMenuItem("Tools/Demigiant/DOTween Utility Panel"))
                    {
                        return;
                    }

                    EditorUtility.DisplayDialog(
                        "DOTween Utility Failed",
                        "DOTween Utility Panel failed to open. Please manually \"Create ASMDEF...\" for DOTween",
                        "Close");
                }
            }
#endif
        }

        private static bool IsDoTweenSetup() => CompilationPipeline
            .GetAssemblies()
            .Any(a => a.name == "DOTween.Modules");

        // ReSharper disable once InconsistentNaming
        [MenuItem(EnableDOTweenPath, true)]
        public static bool DisableDOTweenValidate() =>
#if DOTWEEN
            true
#else
            false
#endif
        ;

        #endregion

        #region Addressable

        private const string DisableAddressablePath = RuntimeUtil.MenuRoot + "Disable Addressable Support";

        [MenuItem(DisableAddressablePath, priority = 101)]
        public static void DisableAddressable()
        {
#if SAINTSFIELD_ADDRESSABLE_DISABLE
            RemoveCompileDefine
#else
            AddCompileDefine
#endif
                ("SAINTSFIELD_ADDRESSABLE_DISABLE");
        }

        [MenuItem(DisableAddressablePath, true)]
        public static bool DisableAddressableValidate() =>
#if SAINTSFIELD_ADDRESSABLE
            true
#else
            false
#endif
        ;
        #endregion

        #region AI Navigation
        private const string DisableAINavigationPath = RuntimeUtil.MenuRoot + "Disable AI Navigation Support";
        [MenuItem(DisableAINavigationPath, priority = 102)]
        public static void DisableDisableAINavigation()
        {
#if SAINTSFIELD_AI_NAVIGATION_DISABLE
            RemoveCompileDefine
#else
            AddCompileDefine
#endif
                ("SAINTSFIELD_AI_NAVIGATION_DISABLE");
        }
        [MenuItem(DisableAINavigationPath, true)]
        public static bool DisableAINavigationValidate() =>
#if SAINTSFIELD_AI_NAVIGATION
            true
#else
            false
#endif
        ;
        #endregion

        #region I2Loc

        private const string EnableI2LocalizationSupportPath = RuntimeUtil.MenuRoot + "Enable I2 Localization Support";
        public const string EnableI2LocalizationSupportMarco = "SAINTSFIELD_I2_LOC";
        [MenuItem(EnableI2LocalizationSupportPath, priority = 103)]
        public static void EnableI2LocalizationSupport()
        {
#if SAINTSFIELD_I2_LOC
            RemoveCompileDefine(EnableI2LocalizationSupportMarco);
#else
            I2SetupWindow.OpenWindow();
#endif
        }
        #endregion

        #region UniTask

        private const string DisableUniTaskSupportPath = RuntimeUtil.MenuRoot + "Disable UniTask Support";
        [MenuItem(DisableUniTaskSupportPath, priority = 104)]
        public static void DisableUniTaskSupport()
        {
#if SAINTSFIELD_UNITASK_DISABLE
            RemoveCompileDefine
#else
            AddCompileDefine
#endif
                ("SAINTSFIELD_UNITASK_DISABLE");
        }

        [MenuItem(DisableUniTaskSupportPath, true)]
        public static bool DisableUniTaskSupportValidate() =>
#if SAINTSFIELD_UNITASK
            true
#else
            false
#endif
        ;

        #endregion

        #region Wwise
        private const string DisableWwiseSupportPath = RuntimeUtil.MenuRoot + "Disable Wwise Support";
        [MenuItem(DisableWwiseSupportPath, priority = 104)]
        public static void DisableWwiseSupport()
        {
#if SAINTSFIELD_WWISE_DISABLE
            RemoveCompileDefine
#else
            AddCompileDefine
#endif
                ("SAINTSFIELD_WWISE_DISABLE");
        }
        [MenuItem(DisableWwiseSupportPath, true)]
        public static bool DisableWwiseSupportValidate() =>
#if WWISE_INSTALLED
            true
#else
            false
#endif
        ;
        #endregion

        #region render-pipelines.universal
        private const string DisableScriptableRenderPipelinePath = RuntimeUtil.MenuRoot + "Disable Scriptable Render Pipeline Support";
        [MenuItem(DisableScriptableRenderPipelinePath, priority = 105)]
        public static void DisableScriptableRenderPipeline()
        {
#if SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL_DISABLE
            RemoveCompileDefine
#else
            AddCompileDefine
#endif
                ("SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL_DISABLE");
        }
        [MenuItem(DisableScriptableRenderPipelinePath, true)]
        public static bool DisableScriptableRenderPipelineValidate() =>
#if SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL
            true
#else
            false
#endif
        ;
        #endregion

        #region Netcode GameObjects
        private const string DisableNetcodeGameObjectsPath = RuntimeUtil.MenuRoot + "Disable Netcode GameObjects Support";
        [MenuItem(DisableNetcodeGameObjectsPath, priority = 106)]
        public static void DisableNetcodeGameObjects()
        {
#if SAINTSFIELD_NETCODE_GAMEOBJECTS_DISABLED
            RemoveCompileDefine
#else
            AddCompileDefine
#endif
                ("SAINTSFIELD_NETCODE_GAMEOBJECTS_DISABLED");
        }
        [MenuItem(DisableNetcodeGameObjectsPath, true)]
        public static bool DisableNetcodeGameObjectsValidate() =>
#if SAINTSFIELD_NETCODE_GAMEOBJECTS
            true
#else
            false
#endif
        ;
        #endregion

        #region Header GUI

        private const string EnableStandAloneHeaderGUISupportPath = RuntimeUtil.MenuRoot + "Enable Stand-Alone Header GUI Support";
        [MenuItem(EnableStandAloneHeaderGUISupportPath)]
        public static void EnableStandAloneHeaderGUISupport()
        {
#if SAINTSFIELD_HEADER_GUI
            RemoveCompileDefine
#else
            AddCompileDefine
#endif
                ("SAINTSFIELD_HEADER_GUI");
        }

        #endregion

        #region Color Palette

        [MenuItem(RuntimeUtil.MenuRoot + "Color Palette...")]
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

#if SAINTSFIELD_DEBUG
        [MenuItem(RuntimeUtil.MenuRoot + "IMGUI Debugger...")]
        public static void OpenIMGUIDebugger()
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.GUIViewDebuggerWindow,UnityEditor")).Show();
        }
#endif

        [MenuItem(RuntimeUtil.MenuRoot + "EColor Preview...")]
        public static void OpenEColorPreview()
        {
            EColorPreviewWindow.Open();
        }

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
        public static void RemoveCompileDefine(string defineCompileConstant, IEnumerable<BuildTargetGroup> targetGroups = null)
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

        [InitializeOnLoadMethod]
        private static void Checkmark()
        {
            Menu.SetChecked(SaintsEditorApplyPath,
#if SAINTSFIELD_SAINTS_EDITOR_APPLY
                true
#else
                false
#endif
            );

            Menu.SetChecked(DisableIMGUIConstantRepaintPath,
#if SAINTSFIELD_SAINTS_EDITOR_APPLY && SAINTSFIELD_SAINTS_EDITOR_IMGUI_CONSTANT_REPAINT_DISABLE
                 true
#else
                false
#endif
            );

            Menu.SetChecked(EnableDOTweenPath,
#if DOTWEEN && SAINTSFIELD_DOTWEEN_ENABLE
                true
#else
                false
#endif
            );

            Menu.SetChecked(DisableAddressablePath,
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
                false
#else
                true
#endif
            );

            Menu.SetChecked(DisableAINavigationPath,
#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLE
                false
#else
                true
#endif
            );

            Menu.SetChecked(EnableI2LocalizationSupportPath,
#if SAINTSFIELD_I2_LOC
                true
#else
                false
#endif
            );

            Menu.SetChecked(DisableUniTaskSupportPath,
#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
                false
#else
                true
#endif
            );

            Menu.SetChecked(DisableWwiseSupportPath,
#if WWISE_INSTALLED && !SAINTSFIELD_WWISE_DISABLE
                false
#else
                true
#endif
            );

            Menu.SetChecked(DisableScriptableRenderPipelinePath,
#if SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL && SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL_DISABLE
                true
#else
                false
#endif
            );

            Menu.SetChecked(DisableNetcodeGameObjectsPath,
#if SAINTSFIELD_NETCODE_GAMEOBJECTS && SAINTSFIELD_NETCODE_GAMEOBJECTS_DISABLED
                true
#else
                false
#endif
            );

            Menu.SetChecked(EnableStandAloneHeaderGUISupportPath,
#if SAINTSFIELD_HEADER_GUI
                true
#else
                false
#endif
            );


        }
    }
}
