#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
    public class DateTimeField: BaseField<long>
    {
        // public readonly DateTimeElement DateTimeElement;

        public DateTimeField(string label, DateTimeElement dateTimeElement) : base(label, dateTimeElement)
        {
            // DateTimeElement = dateTimeElement;
            dateTimeElement.SetGetWorldBound(() => worldBound);
        }
    }
}
#endif
