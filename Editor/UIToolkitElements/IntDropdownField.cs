#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public abstract class IntDropdownElement: BindableElement, INotifyValueChanged<int>
    {
        protected readonly Label Label;

        protected int? CachedValue = null;

        public readonly Button Button;

        protected IntDropdownElement()
        {
            FancyButton fancyButton = new FancyButton();
            fancyButton.DisplayDropdown();

            Button = fancyButton.MainButton;

            // Button.style.flexGrow = 1;

            Label = fancyButton.MainLabel;

            Add(fancyButton);
        }

        public abstract void SetValueWithoutNotify(int newValue);

        public int value
        {
            get => CachedValue ?? 0;
            set
            {
                if (CachedValue == value)
                {
                    return;
                }

                int previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<int> evt = ChangeEvent<int>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        public void SetShowMixedValue(bool showMixedValue)
        {
            if (showMixedValue)
            {
                Label.text = "-";
            }
            else
            {
                SetValueWithoutNotify(value);
            }
        }
    }

    public class IntDropdownField: BaseField<int>
    {
        public readonly Button Button;
        private readonly IntDropdownElement _element;

        public IntDropdownField(string label, IntDropdownElement intDropdownElement) : base(label, intDropdownElement)
        {
            _element = intDropdownElement;
            Button = intDropdownElement.Button;
            AddToClassList(alignedFieldUssClassName);
            AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);

            style.flexShrink = 1;
        }

        public override int value
        {
            get => _element.value;
            set
            {
                if (_element.value == value)
                {
                    return;
                }

                int previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<int> evt = ChangeEvent<int>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        public override void SetValueWithoutNotify(int newValue)
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
