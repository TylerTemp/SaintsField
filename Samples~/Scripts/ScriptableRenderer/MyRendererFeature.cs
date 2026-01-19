using UnityEngine;
#if SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

namespace SaintsField.Samples.Scripts.ScriptableRenderer
{
    public class MyRendererFeature:
#if SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL
        ScriptableRendererFeature
#else
        UnityEngine.ScriptableObject
#endif
    {
#if SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL

        class MyPass : ScriptableRenderPass
        {
            public override void Execute(
                ScriptableRenderContext context,
                ref RenderingData renderingData)
            {
                // Custom rendering logic
            }
        }

        MyPass _pass;

        public override void Create()
        {
            _pass = new MyPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };
        }

        public override void AddRenderPasses(UnityEngine.Rendering.Universal.ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_pass);
        }
#endif

        [CurveRange(EColor.Orange)]
        public AnimationCurve curve1;
    }
}
