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

        protected StringDropdownElement()
        {
            TemplateContainer dropdownElement = UIToolkitUtils.CloneDropdownButtonTree();
            dropdownElement.style.flexGrow = 1;

            Button = dropdownElement.Q<Button>();

            Button.style.flexGrow = 1;

            Label = Button.Q<Label>();

            Add(dropdownElement);
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
    }

    public class StringDropdownField: BaseField<string>
    {
        public readonly Button Button;

        public StringDropdownField(string label, StringDropdownElement stringDropdownElement) : base(label, stringDropdownElement)
        {
            Button = stringDropdownElement.Button;
            AddToClassList(alignedFieldUssClassName);
            AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
        }
    }
}
#endif
