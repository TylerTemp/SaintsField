using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer
{
    // ReSharper disable once InconsistentNaming
    public class DOTweenPlayRenderer: AbsRenderer
    {
        public DOTweenPlayRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
        }

        protected override bool AllowGuiColor => true;
        public override void OnDestroy()
        {

        }

        public override void OnSearchField(string searchString)
        {
        }

        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            return 0f;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container)
        {
            return (null, false);
        }
    }
}
