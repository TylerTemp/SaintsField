using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    [UxmlElement]
    public partial class CleanableTextInput: VisualElement
    {
        private readonly Button _chipInputClean;

        // ReSharper disable once MemberCanBePrivate.Global
        public CleanableTextInput()
        {
            VisualTreeAsset chipInputTree = Util.LoadResource<VisualTreeAsset>("UIToolkit/Chip/ChipInput.uxml");
            VisualElement chipInputRoot = chipInputTree.CloneTree().Q<VisualElement>("chip-input-root");

            TextField chipInput = chipInputRoot.Q<TextField>("chip-input");
            _chipInputClean = chipInputRoot.Q<Button>("chip-input-close");
            _chipInputClean.clicked += () =>
            {
                chipInput.value = string.Empty;
                chipInput.Focus();
            };

            chipInput.RegisterValueChangedCallback(evt => UpdateCleanButtonDisplay(evt.newValue));
            UpdateCleanButtonDisplay(chipInput.value);

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
