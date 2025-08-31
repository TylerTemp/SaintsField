#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette.UIToolkit
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class ColorPaletteLabelPlaceholder: VisualElement
    {
        public ColorPaletteLabelPlaceholder() : this(null)
        {
        }

        public ColorPaletteLabelPlaceholder(string label)
        {
            DualButtonChip dualButtonChip = new DualButtonChip();
            dualButtonChip.SetEnabled(false);

            dualButtonChip.Label.text = string.IsNullOrEmpty(label) ? "" : label;

            Add(dualButtonChip);
        }
    }
}
#endif
