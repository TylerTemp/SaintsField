#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class ButtonItem: VisualElement
    {
        public readonly Button Button;
        // private readonly Label _abel;
        public readonly string LabelText;

        public ButtonItem(string label)
        {
            VisualTreeAsset itemAsset = Util.LoadResource<VisualTreeAsset>("UIToolkit/SaintsAdvancedDropdown/ItemRow.uxml");
            TemplateContainer elementItem = itemAsset.CloneTree();

            Button =
                elementItem.Q<Button>(className: "saintsfield-advanced-dropdown-item");

            Button.Q<Image>("item-checked-image").RemoveFromHierarchy();
            Button.Q<Label>("item-content").text = LabelText = string.IsNullOrEmpty(label) ? "" : label;
            Button.Q<Image>("item-icon-image").RemoveFromHierarchy();

            Add(elementItem);
        }

        public void SetHighlighted(bool b)
        {
            if (b)
            {
                Button.AddToClassList(SaintsAdvancedDropdownUIToolkit.KeyboardHoverClass);
            }
            else
            {
                Button.RemoveFromClassList(SaintsAdvancedDropdownUIToolkit.KeyboardHoverClass);
            }
        }
    }
}
#endif
