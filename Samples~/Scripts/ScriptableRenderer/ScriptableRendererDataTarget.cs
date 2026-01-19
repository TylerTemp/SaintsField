using SaintsField.ScriptableRenderer;
using UnityEngine;

namespace SaintsField.Samples.Scripts.ScriptableRenderer
{
#if SAINTSFIELD_DEBUG
    [CreateAssetMenu(
        fileName = "ScriptableRendererDataTarget",
        menuName = "SAINTSFIELD_DEBUG/ScriptableRendererDataTarget")]
#endif
    public class ScriptableRendererDataTarget:
#if SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL
        SaintsScriptableRendererData
#else
        ScriptableObject
#endif
    {
#if SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL
        protected override UnityEngine.Rendering.Universal.ScriptableRenderer Create()
        {
            return new ScriptableRendererExample(this);
        }
#endif

        [CurveRange]
        public AnimationCurve curve1;
    }
}
