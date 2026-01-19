namespace SaintsField.Samples.Scripts.ScriptableRenderer
{
    public class ScriptableRendererExample
#if SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL
        : UnityEngine.Rendering.Universal.ScriptableRenderer
#endif
    {

#if SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL
        public ScriptableRendererExample(UnityEngine.Rendering.Universal.ScriptableRendererData data) : base(data)
        {
        }

#if !UNITY_6000_3_OR_NEWER
        public override void Setup(UnityEngine.Rendering.ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
        }
#endif

#endif
    }
}
