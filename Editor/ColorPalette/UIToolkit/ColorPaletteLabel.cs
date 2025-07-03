#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette.UIToolkit
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class ColorPaletteLabel: BindableElement, INotifyValueChanged<string>
    {
        public new class UxmlTraits : BindableElement.UxmlTraits { }
        public new class UxmlFactory : UxmlFactory<ColorPaletteLabel, UxmlTraits> { }

        private readonly DualButtonChip _dualButtonChip;
        private readonly CancelableTextInput _cancelableTextInput;

        public readonly UnityEvent OnDeleteClicked = new UnityEvent();

        public bool Editing { get; private set; }

        public ColorPaletteLabel()
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

            string newValue = _cancelableTextInput.TextField.value.Trim();
            if (newValue == value)
            {
                return;
            }

            // Debug.Log(binding);

            value = newValue;
            // SetValueWithoutNotify(value);
            // _dualButtonChip.Label.text = newValue;
            // _cancelableTextInput.TextField.SetValueWithoutNotify(newValue);
        }

        private void OnEditClicked()
        {
            Editing = true;
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
            Editing = false;
            _cancelableTextInput.style.display = DisplayStyle.None;
            _dualButtonChip.style.display = DisplayStyle.Flex;
        }

        public void SetValueWithoutNotify(string newValue)
        {
            _dualButtonChip.Label.text = newValue;
            _cancelableTextInput.TextField.SetValueWithoutNotify(newValue);
        }

        public string value
        {
            get => _dualButtonChip.Label.text;
            set
            {
                if (value == this.value)
                {
                    return;
                }

                string previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<string> evt = ChangeEvent<string>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);

                // Debug.Log($"value={value}");
                // _dualButtonChip.Label.text = value;
                // _cancelableTextInput.TextField.SetValueWithoutNotify(value);
            }
        }
    }
}
#endif
