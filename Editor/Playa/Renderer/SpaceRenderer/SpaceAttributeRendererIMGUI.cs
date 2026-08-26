using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.SpaceRenderer
{
    public partial class SpaceAttributeRenderer
    {
        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            return _spaceAttribute.height;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
        }

        public override void OnDestroyIMGUI()
        {
        }
    }
}
