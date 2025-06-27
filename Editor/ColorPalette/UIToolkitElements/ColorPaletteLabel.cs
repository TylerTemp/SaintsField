#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class ColorPaletteLabel: BaseField<string>
    {
        private readonly DualButtonChip _dualButtonChip;
        private readonly CancelableTextInput _cancelableTextInput;

        public readonly UnityEvent OnDeleteClicked = new UnityEvent();

        public ColorPaletteLabel() : base(null, null)
        {
            _dualButtonChip = new DualButtonChip();
            _dualButtonChip.Button1.clicked += OnEditClicked;
            // _dualButtonChip.Button2.clicked += OnDeleteClicked;
            _dualButtonChip.Button2.clicked += OnDeleteClicked.Invoke;
            Add(_dualButtonChip);

            _cancelableTextInput = new CancelableTextInput
            {
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            _cancelableTextInput.CloseButton.clicked += OnInputCancelClicked;
            _cancelableTextInput.TextField.RegisterCallback<KeyUpEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    OnTextInputEnter();
                }
            }, TrickleDown.TrickleDown);
            Add(_cancelableTextInput);
        }

        private void OnTextInputEnter()
        {
            OnInputCancelClicked();

            string newValue = _cancelableTextInput.TextField.value;
            if (newValue == value)
            {
                return;
            }


            _dualButtonChip.Label.text = newValue;
            _cancelableTextInput.TextField.SetValueWithoutNotify(newValue);
        }

        private void OnEditClicked()
        {
            _cancelableTextInput.TextField.SetValueWithoutNotify(value);
            _cancelableTextInput.style.display = DisplayStyle.Flex;
            _dualButtonChip.style.display = DisplayStyle.None;
        }

        // private void OnDeleteClicked()
        // {
        //     WillDelete = !WillDelete;
        //     if (WillDelete)
        //     {
        //         _dualButtonChip.Button1.style.visibility = Visibility.Hidden;
        //         _dualButtonChip.Button2.AddToClassList("chip-button-reset");
        //         _dualButtonChip.Button2.RemoveFromClassList("chip-button-close");
        //         _dualButtonChip.Label.SetEnabled(false);
        //     }
        //     else
        //     {
        //         _dualButtonChip.Button1.style.visibility = Visibility.Visible;
        //         _dualButtonChip.Button2.AddToClassList("chip-button-close");
        //         _dualButtonChip.Button2.RemoveFromClassList("chip-button-reset");
        //         _dualButtonChip.Label.SetEnabled(true);
        //     }
        // }

        private void OnInputCancelClicked()
        {
            _cancelableTextInput.style.display = DisplayStyle.None;
            _dualButtonChip.style.display = DisplayStyle.Flex;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);

            _dualButtonChip.Label.text = newValue;
            _cancelableTextInput.TextField.SetValueWithoutNotify(newValue);
        }

        public override string value
        {
            get => _dualButtonChip.Label.text;
            set
            {
                _dualButtonChip.Label.text = value;
                _cancelableTextInput.TextField.SetValueWithoutNotify(value);
            }
        }
    }
}
#endif
