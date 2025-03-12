using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsField.Editor.AutoRunner;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.ColorPalette
{
    public class ColorPaletteMenu: SaintsEditorWindow
    {
#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/Color Palette")]
#else
        [MenuItem("Window/Saints/Color Palette")]
#endif
        public static void OpenWindow()
        {
            EditorWindow window = GetWindow<ColorPaletteMenu>(false, "SaintsField Color Palette");
            window.Show();
        }

        [Layout("Dropdown", ELayout.Horizontal)]
        [
            Ordered,
            AdvancedDropdown(nameof(ShowDropdown)),
            OnValueChanged(nameof(TargetChanged)),
            RichLabel("Select Target"),
            BelowSeparator,
        ]
        public SaintsField.ColorPalette inspectTarget;

        [Layout("Dropdown")]
        [Ordered, Button("Save"), PlayaShowIf(nameof(EditorInlineInspectNoFile)), PlayaEnableIf(nameof(TargetIsValid))]
        // ReSharper disable once UnusedMember.Local
        private void SaveToProject()
        {
            string defaultPath;
            SaintsField.ColorPalette existSo = GetAllSo().FirstOrDefault();
            if (existSo != null)
            {
                defaultPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(existSo));
            }
            else
            {
                defaultPath = Directory.Exists("Assets/Editor Default Resources")
                    ? "Assets/Editor Default Resources"
                    : "Assets";
            }

            string path = EditorUtility.SaveFilePanelInProject("Save Auto Runner", "AutoRunner", "asset", "Save Auto Runner", defaultPath);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            AssetDatabase.CreateAsset(editorInlineInspect, path);
            AssetDatabase.SaveAssets();
            inspectTarget = editorInlineInspect = AssetDatabase.LoadAssetAtPath<SaintsField.ColorPalette>(path);
            EditorRefreshTarget();
        }

        private bool TargetIsValid()
        {
            if(editorInlineInspect == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(editorInlineInspect.displayName))
            {
                return false;
            }

            if (editorInlineInspect.colors?.Count <= 0)
            {
                return false;
            }

            return true;
        }

        private bool EditorInlineInspectNoFile()
        {
            // Debug.Log(inspectTarget);

            if (editorInlineInspect == null)
            {
                return true;
            }

            // Debug.Log(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(inspectTarget)));

            return string.IsNullOrEmpty(AssetDatabase.GetAssetPath(editorInlineInspect));
        }

        [Ordered, WindowInlineEditor(typeof(SaintsEditor))]
        public SaintsField.ColorPalette editorInlineInspect;

        [Ordered, Button("<color=red><icon=trash.png/></color> Delete"), PlayaHideIf(nameof(EditorInlineInspectNoFile))]
        // ReSharper disable once UnusedMember.Local
        private void Delete()
        {
            string path = AssetDatabase.GetAssetPath(inspectTarget);
            AssetDatabase.DeleteAsset(path);
            TargetChanged(null);
        }

        // private bool TargetIsNotNull()
        // {
        //     return !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(inspectTarget));
        // }

        private AdvancedDropdownList<SaintsField.ColorPalette> ShowDropdown()
        {
            AdvancedDropdownList<SaintsField.ColorPalette> down = new AdvancedDropdownList<SaintsField.ColorPalette>();

            down.Add("New...", null);

            SaintsField.ColorPalette[] allSo = GetAllSo().ToArray();

            if(allSo.Length > 0)
            {
                down.AddSeparator();

                foreach (SaintsField.ColorPalette scriptableObject in allSo)
                {
                    down.Add(scriptableObject.displayName, scriptableObject);
                }
            }

            return down;
        }

        private static IEnumerable<SaintsField.ColorPalette> GetAllSo()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(SaintsField.ColorPalette)}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SaintsField.ColorPalette colorPalette = AssetDatabase.LoadAssetAtPath<SaintsField.ColorPalette>(path);
                if(colorPalette != null)
                {
                    yield return colorPalette;
                }
            }
        }

        private void TargetChanged(SaintsField.ColorPalette so)
        {
            Debug.Log($"changed to {so}");
            editorInlineInspect = so == null? CreateInstance<SaintsField.ColorPalette>(): so;
            titleContent = new GUIContent(so == null? "Pick or Create Color Palette": $"Color Palette: {(string.IsNullOrEmpty(so.displayName) ? so.name : so.displayName)}");
            EditorRefreshTarget();
        }

        public override Type EditorDrawerType => typeof(AutoRunnerEditor);

        public override void OnEditorEnable()
        {
            if (editorInlineInspect != null)
            {
                return;
            }

            SaintsField.ColorPalette first = GetAllSo().FirstOrDefault(each => each != null);
            if (first == null)
            {
                TargetChanged(null);
            }
            else
            {
                inspectTarget = editorInlineInspect = first;
            }

            EditorRefreshTarget();
        }
    }
}
