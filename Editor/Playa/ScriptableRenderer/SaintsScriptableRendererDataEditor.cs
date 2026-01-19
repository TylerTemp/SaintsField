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
            VisualElement root = new VisualElement();

            // var renderFeatures = ScriptableRendererDataEditor.Styles.RenderFeatures;
            Label mainTitle = new Label("Renderer Features")
            {
                tooltip = "A Renderer Feature is an asset that lets you add extra Render passes to a URP Renderer and configure their behavior.",
            };
            root.Add(mainTitle);

            return root;
        }
    }
}
