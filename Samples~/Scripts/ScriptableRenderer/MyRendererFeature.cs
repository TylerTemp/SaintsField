using SaintsField.Playa;
using SaintsField.ScriptableRenderer;
using UnityEngine;
// ReSharper disable once RedundantUsingDirective
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SaintsField.Samples.Scripts.ScriptableRenderer
{
    public class MyRendererFeature: SaintsScriptableRendererFeature
    {
        private class MyPass : ScriptableRenderPass
        {
#if !SAINTSFIELD_RENDER_PIPELINE_UNIVERSAL_17_1_0_OR_NEWER
            public override void Execute(
                ScriptableRenderContext context,
                ref RenderingData renderingData)
            {
                // Custom rendering logic
            }
#endif
        }

        private MyPass _pass;

        public override void Create()
        {
            _pass = new MyPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques,
            };
        }

        public override void AddRenderPasses(UnityEngine.Rendering.Universal.ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_pass);
        }
        [LayoutStart("Hor", ELayout.Horizontal | ELayout.FoldoutBox)]

        [CurveRange]
        public AnimationCurve curve1;

        // [LabelText("<color=Chartreuse>Hi! <label/>")]
        [InfoBox("SaintsField rendered!")]
        [ResizableTextArea] public string content;
    }
}
