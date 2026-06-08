#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine.UIElements;

namespace SaintsField.Samples.EditorTest.TextColorImporterExample
{
    [CustomEditor(typeof(TextColorImporter))]
    public class TextColorImporterEditor : ScriptedImporterEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.Add(new SaintsEditorCore(this, false).CreateInspectorGUI());  // Fill with SaintsEditor

            // Importer editors still need Apply/Revert.
            // This draws Unity's standard importer Apply/Revert buttons.
            root.Add(new IMGUIContainer(ApplyRevertGUI));

            return root;
        }
    }
}
#endif
