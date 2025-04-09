using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.PlayaSeparatorSemiRenderer
{
    public partial class PlayaSeparatorRenderer
    {
        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            throw new System.NotImplementedException("PlayaSeparatorRenderer is not supported in IMGUI. Try UI Toolkit instead");
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            throw new System.NotImplementedException("PlayaSeparatorRenderer is not supported in IMGUI. Try UI Toolkit instead");
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            throw new System.NotImplementedException("PlayaSeparatorRenderer is not supported in IMGUI. Try UI Toolkit instead");
        }
    }
}
