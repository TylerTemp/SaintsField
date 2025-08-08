using System.Collections.Generic;
using SaintsField.Editor.UIToolkitElements;

#if UNITY_2021_3_OR_NEWER
namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
    public class FlagsDropdownElement: IntDropdownElement
    {
        public readonly EnumFlagsMetaInfo MetaInfo;

        public FlagsDropdownElement(EnumFlagsMetaInfo metaInfo)
        {
            MetaInfo = metaInfo;
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;

            if (newValue == 0)
            {
                Label.text = "<b>Nothing</b>";
                return;
            }

            if((newValue & MetaInfo.AllCheckedInt) == MetaInfo.AllCheckedInt)
            {
                Label.text = "<b>Everything</b>";
                return;
            }

            List<string> selectedNames = new List<string>();

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (KeyValuePair<int, EnumFlagsUtil.EnumDisplayInfo> kvp in MetaInfo.BitValueToName)
            {
                if ((newValue & kvp.Key) != 0)
                {
                    selectedNames.Add(kvp.Value.Name);
                }
            }

            if (selectedNames.Count == 0)
            {
                Label.text = $"<color=red>?</color> {newValue}";
                return;
            }

            Label.text = string.Join(", ", selectedNames);
        }
    }
}
#endif
