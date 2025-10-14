#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class CapsuleButtonElement: VisualElement
    {
        private static VisualTreeAsset _containerTree;

        public readonly Button Button;

        // ReSharper disable once MemberCanBePrivate.Global
        public CapsuleButtonElement(): this(null) {}

        public CapsuleButtonElement(string label)
        {
            _containerTree ??= Util.LoadResource<VisualTreeAsset>("UIToolkit/DateTime/CapsuleButton.uxml");
            VisualElement element = _containerTree.CloneTree();
            Add(element);

            Button = element.Q<Button>("capsule");
            Button.text = label ?? "";
        }
    }
}
#endif
