#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class CleanableTextInput: VisualElement
    {
        private readonly Button _chipInputClean;
        public readonly TextField TextField;

        // ReSharper disable once MemberCanBePrivate.Global
        public CleanableTextInput(): this(null){}
        public CleanableTextInput(string label)
        {
            VisualTreeAsset chipInputTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/Chip/ChipInput.uxml");
            VisualElement chipInputRoot = chipInputTree.CloneTree().Q<VisualElement>("chip-input-root");

            TextField = chipInputRoot.Q<TextField>("chip-input");
            _chipInputClean = chipInputRoot.Q<Button>("chip-input-close");
            _chipInputClean.clicked += () =>
            {
                TextField.value = string.Empty;
                TextField.Focus();
            };

            TextField.SetValueWithoutNotify(string.IsNullOrEmpty(label)? "": label);

            TextField.RegisterValueChangedCallback(evt => UpdateCleanButtonDisplay(evt.newValue));
            UpdateCleanButtonDisplay(TextField.value);

            Add(chipInputRoot);
        }

        private void UpdateCleanButtonDisplay(string value)
        {
            DisplayStyle display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
            if (_chipInputClean.style.display != display)
            {
                _chipInputClean.style.display = display;
            }
        }
    }
}
#endif
