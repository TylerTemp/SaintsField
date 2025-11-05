using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    public partial class HexInputLengthElement: BindableElement, INotifyValueChanged<string>
    {
        public readonly TextField TextField;
        private string _cachedValue = "";
        private int _length;

        private Regex _regex;
        private static readonly Color WarningColor = new Color(0.8490566f, 0.3003738f, 0.3003738f);
        private readonly StyleColor _originColor;

        // ReSharper disable once InconsistentNaming
        public int length
        {
            get => _length;
            set
            {
                _length = value;
                _regex = new Regex($"^[0-9a-f]{{{value}}}$");
                TextField.maxLength = value;
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public HexInputLengthElement(): this(4) {}

        // ReSharper disable once MemberCanBePrivate.Global
        public HexInputLengthElement(int length)
        {
            _length = length;
            _regex = new Regex($"^[0-9a-f]{{{length}}}$");
            TextField = new TextField
            {
                maxLength = length,
                style =
                {
                    marginLeft = 0,
                    marginRight = 0,
                },
            };
            _originColor = TextField.style.backgroundColor;
            TextField.RegisterValueChangedCallback(evt => value = evt.newValue);
            Add(TextField);
        }

        public void SetValueWithoutNotify(string newValue)
        {
            _cachedValue = newValue;

            // ReSharper disable once InvertIf
            if (TextField.value != newValue)
            {
                TextField.SetValueWithoutNotify(newValue);
            }
            style.backgroundColor = IsValid(newValue, _length, _regex) ? _originColor : WarningColor;
        }

        public string value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                string previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<string> evt = ChangeEvent<string>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        private static bool IsValid(string v, int maxLength, Regex regex)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (v.Length != maxLength)
            {
                return false;
            }

            return regex.Match(v).Success;
        }
    }
}
