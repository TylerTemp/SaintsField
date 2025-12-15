#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
    public class DateTimeField: BaseField<long>
    {
        private readonly DateTimeElement _dateTimeElement;

        public DateTimeField(string label, DateTimeElement dateTimeElement) : base(label, dateTimeElement)
        {
            _dateTimeElement = dateTimeElement;
            dateTimeElement.SetGetWorldBound(() => worldBound);
        }

        public override void SetValueWithoutNotify(long newValue)
        {
            _dateTimeElement.SetValueWithoutNotify(newValue);
        }

        public override long value
        {
            get => _dateTimeElement.value;
            set => _dateTimeElement.value = value;
        }
    }
}
#endif
