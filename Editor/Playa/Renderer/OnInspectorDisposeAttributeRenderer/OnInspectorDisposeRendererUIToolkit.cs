using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.OnInspectorDisposeAttributeRenderer
{
    public partial class OnInspectorDisposeRenderer
    {
        private bool _usedUIToolkit;

        protected override bool AllowGuiColor => false;


        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot, VisualElement container)
        {
            _usedUIToolkit = true;
            return (new VisualElement  // dummy so destroy will work (this renderer can be recorded)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = "SaintsField-OnInspectorDisposeRenderer",
            }, false);
        }


        public override void OnDestroyUIToolkit()
        {
            if (_usedUIToolkit)
            {
                InvokeDispose();
            }
        }

    }
}
