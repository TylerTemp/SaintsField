#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.AiNavigation;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaDrawer
{
    public class NavMeshAreaIntElement: IntDropdownElement
    {
        private readonly NavMeshAreaAttribute _navMeshAreaAttribute;

        public NavMeshAreaIntElement(NavMeshAreaAttribute attribute)
        {
            _navMeshAreaAttribute = attribute;
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;

            List<string> matched = new List<string>();
            foreach (AiNavigationUtils.NavMeshArea area in AiNavigationUtils.GetNavMeshAreas())
            {
                if (_navMeshAreaAttribute.IsMask)
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
