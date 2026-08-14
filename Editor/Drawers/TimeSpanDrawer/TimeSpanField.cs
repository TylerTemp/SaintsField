#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TimeSpanDrawer
{
    public class TimeSpanField: BaseField<long>
    {
        public readonly TimeSpanElement TimeSpanElement;

        private TimeSpanField(string label, TimeSpanElement timeSpanElement) : base(label, timeSpanElement)
        {
            style.flexShrink = 1;
            TimeSpanElement = timeSpanElement;
            timeSpanElement.RegisterValueChangedCallback(evt => evt.StopPropagation());
        }

        public TimeSpanField(string label, bool defaultExpand = false) : this(label, new TimeSpanElement(defaultExpand))
        {
        }

        public override void SetValueWithoutNotify(long newValue)
        {
            TimeSpanElement.SetValueWithoutNotify(newValue);
        }

        public override long value
        {
            get => TimeSpanElement.value;
            set
            {
                if (TimeSpanElement.value == value)
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
            TimeSpanElement.SetShowMixedValue(showMixedValue);
        }
    }
}
#endif
