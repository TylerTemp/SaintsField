using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    [UxmlElement]
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class ColorPaletteLabel: VisualElement
    {
        private readonly DualButtonChip _dualButtonChip;
        private readonly CancelableTextInput _cancelableTextInput;

        public readonly string InitLabel;
        public string Label { get; private set; }

        public ColorPaletteLabel() : this(null)
        {
        }
        public ColorPaletteLabel(string label)
        {
            InitLabel = Label = label;

            _dualButtonChip = new DualButtonChip(label);
            _dualButtonChip.Button1.clicked += OnEditClicked;
            _dualButtonChip.Button2.clicked += OnDeleteClicked;
            Add(_dualButtonChip);

            _cancelableTextInput = new CancelableTextInput
            {
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            _cancelableTextInput.CloseButton.clicked += OnInputCancelClicked;
            Add(_cancelableTextInput);
        }

        private void OnEditClicked()
        {
            _cancelableTextInput.TextField.SetValueWithoutNotify(Label);
            _cancelableTextInput.style.display = DisplayStyle.Flex;
            _dualButtonChip.style.display = DisplayStyle.None;
        }

        public bool WillDelete { get; private set; }

        private void OnDeleteClicked()
        {
            WillDelete = !WillDelete;
            if (WillDelete)
            {
                _dualButtonChip.Button1.style.visibility = Visibility.Hidden;
                _dualButtonChip.Button2.AddToClassList("chip-button-reset");
                _dualButtonChip.Button2.RemoveFromClassList("chip-button-close");
                _dualButtonChip.Label.SetEnabled(false);
            }
            else
            {
                _dualButtonChip.Button1.style.visibility = Visibility.Visible;
                _dualButtonChip.Button2.AddToClassList("chip-button-close");
                _dualButtonChip.Button2.RemoveFromClassList("chip-button-reset");
                _dualButtonChip.Label.SetEnabled(true);
            }
        }

        private void OnInputCancelClicked()
        {
            _cancelableTextInput.style.display = DisplayStyle.None;
            _dualButtonChip.style.display = DisplayStyle.Flex;
        }
    }
}
