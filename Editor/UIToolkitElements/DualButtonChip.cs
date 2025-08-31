#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class DualButtonChip : VisualElement
    {
        public readonly Button Button1;
        public readonly Button Button2;
        public readonly Label Label;

        // ReSharper disable once MemberCanBePrivate.Global
        public DualButtonChip() : this(null)
        {
        }

        public DualButtonChip(string label)
        {
            VisualTreeAsset chipTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/Chip/Chip.uxml");
            TemplateContainer chipClone = chipTree.CloneTree();

            VisualElement chipRoot = chipClone.Q<VisualElement>("chip-root");
            Button1 = chipRoot.Q<Button>("chip-button-1");
            Button2 = chipRoot.Q<Button>("chip-button-2");

            Label = chipRoot.Q<Label>("chip-label");
            Label.text = string.IsNullOrEmpty(label) ? "" : label;
            Add(chipClone);
        }
    }
}
#endif
