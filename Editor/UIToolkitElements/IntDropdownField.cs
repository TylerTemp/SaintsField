#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public abstract class IntDropdownElement: BindableElement, INotifyValueChanged<int>
    {
        protected readonly Label Label;

        protected int CachedValue = -1;

        public readonly Button Button;

        protected IntDropdownElement()
        {
            TemplateContainer dropdownElement = UIToolkitUtils.CloneDropdownButtonTree();
            dropdownElement.style.flexGrow = 1;

            Button = dropdownElement.Q<Button>();

            Button.style.flexGrow = 1;

            Label = Button.Q<Label>();

            Add(dropdownElement);
        }

        public abstract void SetValueWithoutNotify(int newValue);

        public int value
        {
            get => CachedValue;
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
    }

    public class IntDropdownField: BaseField<int>
    {
        public readonly Button Button;

        public IntDropdownField(string label, IntDropdownElement intDropdownElement) : base(label, intDropdownElement)
        {
            Button = intDropdownElement.Button;
            AddToClassList(alignedFieldUssClassName);
            AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
        }
    }
}
#endif
