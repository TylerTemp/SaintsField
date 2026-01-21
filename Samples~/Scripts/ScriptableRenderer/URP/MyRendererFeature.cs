using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SaintsField.Samples.Scripts.ScriptableRenderer.URP
{
    public class MyRendererFeature: SaintsField.ScriptableRenderer.Urp.SaintsScriptableRendererFeature
    {

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
        [LayoutStart("Hor", ELayout.Horizontal | ELayout.FoldoutBox)]

        [CurveRange]
        public AnimationCurve curve1;

        [ResizableTextArea] public string content;
    }
}
