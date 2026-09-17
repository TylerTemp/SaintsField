using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.OnInspectorDisposeAttributeRenderer
{
    public partial class OnInspectorDisposeRenderer
    {
        private bool _usedIMGUI;

        public override void OnDestroyIMGUI()
        {
            if (_usedIMGUI)
            {
                InvokeDispose();
            }
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            _usedIMGUI = true;
            return 0;
        }


        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
        }
    }
}
