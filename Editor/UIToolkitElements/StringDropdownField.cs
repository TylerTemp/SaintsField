#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public abstract class StringDropdownElement : BindableElement, INotifyValueChanged<string>
    {
        protected readonly Label Label;

        protected string CachedValue = null;

        public readonly Button Button;

        public StringDropdownElement()
        {
            FancyButton fancyButton = new FancyButton();
            fancyButton.DisplayDropdown();
            // TemplateContainer dropdownElement = UIToolkitUtils.CloneDropdownButtonTree();
            // dropdownElement.style.flexGrow = 1;

            Button = fancyButton.MainButton;

            // Button.style.flexGrow = 1;

            Label = fancyButton.MainLabel;
            Label.RegisterValueChangedCallback(evt => evt.StopPropagation());

            Add(fancyButton);
        }

        protected void SetLabelString(string v)
        {
            Label.text = v;
            // ((INotifyValueChanged<string>)Label).SetValueWithoutNotify(v);
        }

        public abstract void SetValueWithoutNotify(string newValue);

        public string value
        {
            get => CachedValue;
            set
            {
                if (CachedValue == value)
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

        public void SetShowMixedValue(bool showMixedValue)
        {
            Label.text = showMixedValue ? "-" : CachedValue;
        }
    }

    public class StringDropdownField: BaseField<string>
    {
        public readonly Button Button;
        private readonly StringDropdownElement _element;

        public StringDropdownField(string label, StringDropdownElement stringDropdownElement) : base(label, stringDropdownElement)
        {
            _element = stringDropdownElement;

            Button = stringDropdownElement.Button;
            AddToClassList(alignedFieldUssClassName);
            AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
            style.flexShrink = 1;

            stringDropdownElement.RegisterValueChangedCallback(evt => evt.StopPropagation());
        }

        public override string value
        {
            get => _element.value;
            set
            {
                if (_element.value == value)
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

        public override void SetValueWithoutNotify(string newValue)
        {
            _element.SetValueWithoutNotify(newValue);
        }

        protected override void UpdateMixedValueContent()
        {
            _element.SetShowMixedValue(showMixedValue);
        }
    }
}
#endif
