using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.AutoRunner
{
    public class AutoRunnerMenu: SaintsEditorWindow
    {
#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/Auto Runner")]
#else
        [MenuItem("Window/Saints/Auto Runner")]
#endif
        public static void OpenWindow()
        {
            EditorWindow window = GetWindow<AutoRunnerMenu>(false, "SaintsField Auto Runner");
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
        public AutoRunnerWindow inspectTarget;

        [Layout("Dropdown")]
        [Ordered, Button("Save"), PlayaShowIf(nameof(EditorInlineInspectNoFile))]
        private void SaveToProject()
        {
            string defaultPath;
            var existSo = GetAllSo().FirstOrDefault();
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
            inspectTarget = editorInlineInspect = AssetDatabase.LoadAssetAtPath<AutoRunnerWindow>(path);
            EditorRefreshTarget();
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

        [Ordered, WindowInlineEditor(typeof(AutoRunnerEditor))]
        public AutoRunnerWindow editorInlineInspect;

        private AdvancedDropdownList<AutoRunnerWindow> ShowDropdown()
        {
            AdvancedDropdownList<AutoRunnerWindow> down = new AdvancedDropdownList<AutoRunnerWindow>();

            down.Add("New...", null);

            AutoRunnerWindow[] allSo = GetAllSo().ToArray();

            if(allSo.Length > 0)
            {
                down.AddSeparator();

                foreach (AutoRunnerWindow scriptableObject in allSo)
                {
                    down.Add(scriptableObject.name, scriptableObject);
                }
            }

            return down;
        }

        private static IEnumerable<AutoRunnerWindow> GetAllSo()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(AutoRunnerWindow)}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AutoRunnerWindow autoRunner = AssetDatabase.LoadAssetAtPath<AutoRunnerWindow>(path);
                yield return autoRunner;
            }
        }

        private void TargetChanged(AutoRunnerWindow so)
        {
            Debug.Log($"changed to {so}");
            editorInlineInspect = so == null? CreateInstance<AutoRunnerWindow>(): so;
            titleContent = new GUIContent(so == null? "Pick or Create Auto Runner": $"Auto Runner: {so.name}");
        }

        public override Type EditorDrawerType => typeof(AutoRunnerEditor);

        public override void OnEditorEnable()
        {
            if (editorInlineInspect != null)
            {
                return;
            }

            AutoRunnerWindow first = GetAllSo().FirstOrDefault(each => each != null);
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
