using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.EmptyFakeRenderer
{
    public partial class EmptyRenderer
    {
        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container)
        {
            return (null, false);
        }
    }
}
