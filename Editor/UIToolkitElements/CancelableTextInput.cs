using System.Web;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    [UxmlElement]
    public partial class CancelableTextInput: VisualElement
    {
        public readonly Button CloseButton;
        public readonly TextField TextField;

        // ReSharper disable once MemberCanBePrivate.Global
        public CancelableTextInput(): this(null)
        {
        }

        public CancelableTextInput(string value)
        {
            VisualTreeAsset chipInputTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/Chip/ChipInput.uxml");
            VisualElement chipInputClone = chipInputTree.CloneTree();
            VisualElement chipInputRoot = chipInputClone.Q<VisualElement>("chip-input-root");

            CloseButton = chipInputRoot.Q<Button>("chip-input-close");
            TextField = chipInputRoot.Q<TextField>("chip-input");

            TextField.SetValueWithoutNotify(string.IsNullOrEmpty(value) ? "" : value);

            Add(chipInputClone);
        }
    }
}
