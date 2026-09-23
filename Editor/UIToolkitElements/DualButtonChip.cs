#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine;
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
        public readonly VisualElement ChipRoot;

        // ReSharper disable once MemberCanBePrivate.Global
        public DualButtonChip() : this(null)
        {
        }

        public DualButtonChip(string label)
        {
            VisualTreeAsset chipTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/Chip/Chip.uxml");
            // TemplateContainer chipClone = chipTree.CloneTree();
            chipTree.CloneTree(this);

            ChipRoot = this.Q<VisualElement>("chip-root");
            Button1 = ChipRoot.Q<Button>("chip-button-1");
            Button2 = ChipRoot.Q<Button>("chip-button-2");

            Label = ChipRoot.Q<Label>("chip-label");
            Label.text = string.IsNullOrEmpty(label) ? "" : label;
            // Add(chipClone);

            UIToolkitUtils.OnAttachToPanelOnceWithEnsure(this, () => SaveColor());

        }

        private Color? defaultColor;

        private Color SaveColor()
        {
            defaultColor = ChipRoot.resolvedStyle.backgroundColor;
            if (defaultColor == Color.clear)
            {
                defaultColor = new Color(91 / 255f, 91 / 255f, 91 / 255f);
            }

            return defaultColor.Value;
        }

        private Color GetDefaultColor()
        {
            return defaultColor ?? SaveColor();
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public virtual void SetColor(Color color)
        {
            Color rootColor = GetDefaultColor();

            Color bgColor = Color.Lerp(color, rootColor, 0.7f);
            ChipRoot.style.backgroundColor = bgColor;

            // Button1.style.backgroundColor
            //     = Button2.style.backgroundColor
            //         = StyleKeyword.Null;

            Color borderBottomColor = Color.Lerp(color, rootColor, 0.1f);
            Color borderColor = Color.Lerp(color, rootColor, 0.5f);
            borderColor.a = 0.5f;
            ChipRoot.style.borderLeftWidth
                = ChipRoot.style.borderTopWidth
                    = ChipRoot.style.borderRightWidth
                        = ChipRoot.style.borderBottomWidth
                            = 1;
            ChipRoot.style.borderBottomColor = borderBottomColor;
            ChipRoot.style.borderLeftColor
                = ChipRoot.style.borderTopColor
                    = ChipRoot.style.borderRightColor
                            = borderColor;
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public virtual void UnsetColor()
        {
            ChipRoot.style.backgroundColor = GetDefaultColor();
            // Button1.style.backgroundColor
            //     = Button2.style.backgroundColor
            //         = StyleKeyword.Null;

            ChipRoot.style.borderLeftWidth
                = ChipRoot.style.borderTopWidth
                    = ChipRoot.style.borderRightWidth
                        = ChipRoot.style.borderBottomWidth
                            = 0;
        }
    }
}
#endif
