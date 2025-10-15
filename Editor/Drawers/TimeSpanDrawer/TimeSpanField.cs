#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TimeSpanDrawer
{
    public class TimeSpanField: BaseField<long>
    {
        public TimeSpanField(string label, TimeSpanElement timeSpanElement) : base(label, timeSpanElement)
        {
        }
    }
}
#endif
