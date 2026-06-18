#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaDrawer
{
    public class NavMeshAreaStringElement: StringDropdownElement
    {
        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;

            foreach (AiNavigationUtils.NavMeshArea area in AiNavigationUtils.GetNavMeshAreas())
            {
                // ReSharper disable once InvertIf
                if (area.Name == newValue)
                {
                    Label.text = $"{area.Name} <color=#808080>({area.Value})</color>";
                    return;
                }
            }

            Label.text = string.IsNullOrEmpty(newValue)
                ? "-"
                : $"<color=red>?</color> {newValue}";
        }
    }
}
#endif
