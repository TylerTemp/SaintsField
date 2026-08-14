#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
    public class DateTimeField: BaseField<long>
    {
        public readonly DateTimeElement DateTimeElement;

        private DateTimeField(string label, DateTimeElement dateTimeElement) : base(label, dateTimeElement)
        {
            style.flexShrink = 1;
            DateTimeElement = dateTimeElement;
            dateTimeElement.RegisterValueChangedCallback(evt => evt.StopPropagation());
            dateTimeElement.SetGetWorldBound(() => worldBound);
        }

        public DateTimeField(string label) : this(label, new DateTimeElement())
        {
        }

        public override void SetValueWithoutNotify(long newValue)
        {
            DateTimeElement.SetValueWithoutNotify(newValue);
        }

        public override long value
        {
            get => DateTimeElement.value;
            set
            {
                if (DateTimeElement.value == value)
                {
                    return;
                }

                long previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<long> evt = ChangeEvent<long>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        protected override void UpdateMixedValueContent()
        {
            DateTimeElement.SetShowMixedValue(showMixedValue);
        }
    }
}
#endif
