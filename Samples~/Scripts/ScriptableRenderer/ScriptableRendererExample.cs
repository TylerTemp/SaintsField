namespace SaintsField.Samples.Scripts.ScriptableRendererDataExample
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
#endif

    }
}
