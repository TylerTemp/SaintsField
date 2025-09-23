using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.EmptyFakeRenderer
{
    public partial class EmptyRenderer
    {
        protected override void RenderTargetIMGUI(float width, AbsRenderer.PreCheckResult preCheckResult)
        {
        }

        protected override float GetFieldHeightIMGUI(float width, AbsRenderer.PreCheckResult preCheckResult)
        {
            return 0;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, AbsRenderer.PreCheckResult preCheckResult)
        {
        }
    }
}
