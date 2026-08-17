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
            dateTimeElement.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();
                value = evt.newValue;
            });
            dateTimeElement.SetGetWorldBound(() => worldBound);
        }

        public DateTimeField(string label) : this(label, new DateTimeElement())
        {
        }

        public override void SetValueWithoutNotify(long newValue)
        {
            base.SetValueWithoutNotify(newValue);
            DateTimeElement.SetValueWithoutNotify(newValue);
        }

        protected override void UpdateMixedValueContent()
        {
            DateTimeElement.SetShowMixedValue(showMixedValue);
        }
    }
}
#endif
