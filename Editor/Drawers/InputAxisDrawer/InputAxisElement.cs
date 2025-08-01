#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;

namespace SaintsField.Editor.Drawers.InputAxisDrawer
{
    public class InputAxisElement: StringDropdownElement
    {
        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string axisName in InputAxisUtils.GetAxisNames())
            {
                // ReSharper disable once InvertIf
                if (axisName == newValue)
                {
                    Label.text = newValue;
                    return;
                }
            }

            Label.text = $"<color=red>?</color> {(string.IsNullOrEmpty(newValue)? "": $"({newValue})")}";
        }
    }
}
#endif
