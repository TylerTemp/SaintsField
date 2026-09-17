using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.OnInspectorInitAttributeRenderer
{
    public partial class OnInspectorInitRenderer
    {
        protected override bool AllowGuiColor => false;

        public override void OnDestroyUIToolkit()
        {
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot, VisualElement container)
        {
            VisualElement dummyMounter = new VisualElement
            {
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            return (dummyMounter, false);
        }
    }
}
