#if UNITY_EDITOR
using System.Collections.Generic;
using SaintsField.Editor;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsWindowEditorExample
{
    public class ExampleSo: SaintsEditorWindow
    {

#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/ScriptableEditor")]
#else
        [MenuItem("Windows/Saints/Example/ScriptableEditor")]
#endif
        public static void TestOpenWindow()
        {
            EditorWindow window = GetWindow<ExampleSo>(false, "Scriptable Editor");
            window.Show();
        }

        // public override Object EditorGetInitTarget(Object oldTarget)
        // {
        //     return GetAllSo()[0];
        // }

        [AdvancedDropdown(nameof(ShowDropdown)), OnValueChanged(nameof(TargetChanged))]
        public ScriptableObject inspectTarget;

        [WindowInlineEditor]
        public UnityEngine.Object editorInlineInspect;

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

        private void TargetChanged(ScriptableObject so) => editorInlineInspect = so;

        [Playa.Button]
        private void Save(){}

        [Playa.Button]
        private void Discard(){}
    }
}
#endif
