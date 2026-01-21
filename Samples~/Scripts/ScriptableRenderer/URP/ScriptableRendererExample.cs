namespace SaintsField.Samples.Scripts.ScriptableRenderer.URP
{
    public class ScriptableRendererExample : UnityEngine.Rendering.Universal.ScriptableRenderer
    {

        public ScriptableRendererExample(UnityEngine.Rendering.Universal.ScriptableRendererData data) : base(data)
        {
        }

#if !UNITY_6000_3_OR_NEWER
        public override void Setup(UnityEngine.Rendering.ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
        }
#endif
    }
}
