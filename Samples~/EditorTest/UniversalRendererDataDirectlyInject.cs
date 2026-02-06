#if UNITY_EDITOR  && SAINTSFIELD_DEBUG && SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL
using SaintsField.Editor.Playa.ScriptableRenderer;
using UnityEditor;
using UnityEngine.Rendering.Universal;

namespace SaintsField.Samples.EditorTest
{
    [CustomEditor(typeof(UniversalRendererData), true)]
    public class UniversalRendererDataDirectlyInject: SaintsUniversalRendererDataEditor
    {

    }
}
#endif
