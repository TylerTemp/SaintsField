using SaintsField.ScriptableRenderer;
using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.ScriptableRenderer
{
    [CustomEditor(typeof(SaintsScriptableRendererData), true)]
    public class SaintsScriptableRendererDataEditor:
        ScriptableRendererDataEditor
        // SaintsEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return new ScriptableRendererDataCore(this).CreateInspectorGUI();
        }
    }
}
