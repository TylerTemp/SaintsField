#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using UnityEditorInternal;

namespace SaintsField.Editor.Drawers.TagDrawer
{
    public class TagElement: StringDropdownElement
    {
        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string tags in InternalEditorUtility.tags)
            {
                // ReSharper disable once InvertIf
                if (tags == newValue)
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
