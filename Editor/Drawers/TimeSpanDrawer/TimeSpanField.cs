#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TimeSpanDrawer
{
    public class TimeSpanField: BaseField<long>
    {
        private readonly TimeSpanElement _timeSpanElement;
        public TimeSpanField(string label, TimeSpanElement timeSpanElement) : base(label, timeSpanElement)
        {
            style.flexShrink = 1;
            _timeSpanElement = timeSpanElement;
        }

        public override void SetValueWithoutNotify(long newValue)
        {
            _timeSpanElement.SetValueWithoutNotify(newValue);
        }

        public override long value
        {
            get => _timeSpanElement.value;
            set => _timeSpanElement.value = value;
        }
    }
}
#endif
