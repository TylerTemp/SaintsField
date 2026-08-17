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
            timeSpanElement.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();
                value = evt.newValue;
            });
        }

        public TimeSpanField(string label, bool defaultExpand = false) : this(label, new TimeSpanElement(defaultExpand))
        {
        }

        public override void SetValueWithoutNotify(long newValue)
        {
            base.SetValueWithoutNotify(newValue);
            TimeSpanElement.SetValueWithoutNotify(newValue);
        }

        protected override void UpdateMixedValueContent()
        {
            TimeSpanElement.SetShowMixedValue(showMixedValue);
        }
    }
}
#endif
