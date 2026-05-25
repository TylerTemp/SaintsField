using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.ButtonCustomContextMenuFakeRenderer
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class ButtonCustomContextMenuRenderer: AbsRenderer
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly CustomContextMenuAttribute _customContextMenuAttribute;

        public ButtonCustomContextMenuRenderer(CustomContextMenuAttribute customContextMenuAttribute, SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _customContextMenuAttribute = customContextMenuAttribute;
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
            return 0;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
        }
    }
}
