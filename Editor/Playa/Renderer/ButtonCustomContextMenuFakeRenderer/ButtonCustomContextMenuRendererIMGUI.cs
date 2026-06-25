using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.ButtonCustomContextMenuFakeRenderer
{
    public partial class ButtonCustomContextMenuRenderer: AbsRenderer
    {
        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            return 0;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
        }

        public override void OnDestroyIMGUI()
        {
        }
    }
}
