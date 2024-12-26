#if UNITY_EDITOR
using System.Collections.Generic;
using SaintsField.Editor;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsWindowEditorExample
{
    public class ExampleSo: SaintsEditorWindow
    {

#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/ScriptableEditor")]
#else
        [MenuItem("Window/Saints/Example/ScriptableEditor")]
#endif
        public static void TestOpenWindow()
        {
            EditorWindow window = GetWindow<ExampleSo>(false, "Scriptable Editor");
            window.Show();
        }

        [
            AdvancedDropdown(nameof(ShowDropdown)),
            OnValueChanged(nameof(TargetChanged))
        ]
        public ScriptableObject inspectTarget;

        [WindowInlineEditor]
        public Object editorInlineInspect;

        private IReadOnlyList<ScriptableObject> GetAllSo() => Resources.LoadAll<ScriptableObject>("");

        private AdvancedDropdownList<ScriptableObject> ShowDropdown()
        {
            AdvancedDropdownList<ScriptableObject> down = new AdvancedDropdownList<ScriptableObject>();
            down.Add("[Null]", null);
            foreach (ScriptableObject scriptableObject in GetAllSo())
            {
                down.Add(scriptableObject.name, scriptableObject);
            }

            return down;
        }

        private void TargetChanged(ScriptableObject so)
        {
            Debug.Log($"changed to {so}");
            editorInlineInspect = so;
            titleContent = new GUIContent(so == null? "Pick One": $"Edit {so.name}");
        }

        [LayoutStart("Buttons", ELayout.Horizontal)]

        [Button]
        private void Save() {}

        [Button]
        private void Discard() {}

        // [Button]
        // private void Test() => Debug.Log(editorInlineInspect);
    }
}
#endif
