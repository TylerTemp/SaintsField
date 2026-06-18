#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;

namespace SaintsField.Editor.Drawers.AiNavigation
{
    public class NavMeshAreaIntElement: IntDropdownElement
    {
        private readonly bool _isMask;

        public NavMeshAreaIntElement(bool isMask)
        {
            _isMask = isMask;
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;

            AiNavigationUtils.NavMeshArea[] allAreas = AiNavigationUtils.GetNavMeshAreas().ToArray();

            if (_isMask)
            {
                if (newValue == 0)
                {
                    Label.text = "<b>Noting</b>";
                    return;
                }

                int allMask = allAreas.Aggregate(0, (current, area) => current | area.Mask);
                if ((newValue & allMask) == allMask)
                {
                    Label.text = "<b>Everything</b>";
                    return;
                }
            }

            List<string> matched = new List<string>();
            foreach (AiNavigationUtils.NavMeshArea area in allAreas)
            {
                if (_isMask)
                {
                    if ((newValue & area.Mask) != 0)
                    {
                        matched.Add(area.Name);
                    }
                }
                else
                {
                    if (area.Value == newValue)
                    {
                        Label.text = $"{area.Name} <color=#808080>({area.Value})</color>";
                        return;
                    }
                }
            }

            Label.text = matched.Count == 0
                ? $"<color=red>?</color> {newValue}"
                : string.Join(", ", matched);
        }
    }
}
#endif
