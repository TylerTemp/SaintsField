#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.EmptyFakeRenderer
{
    public partial class EmptyRenderer
    {
        public override void OnDestroyUIToolkit()
        {
        }

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
        {
            return (null, false);
        }
    }
}
#endif
