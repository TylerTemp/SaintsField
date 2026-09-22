using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.ChipsRenderer
{
    public partial class ChipsAttributeRenderer
    {
        public override void OnDestroyIMGUI()
        {
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            return 0;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
        }
    }
}
