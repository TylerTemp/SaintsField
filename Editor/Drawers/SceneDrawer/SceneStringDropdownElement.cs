#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Linq;
using SaintsField.Editor.UIToolkitElements;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
    public class SceneStringDropdownElement: StringDropdownElement
    {
        private readonly bool _fullPath;

        public SceneStringDropdownElement(bool fullPath)
        {
            _fullPath = fullPath;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;

            foreach ((string scenePath, int index) in SceneUtils.GetTrimedScenePath(_fullPath).WithIndex())
            {
                // ReSharper disable once InvertIf
                if (scenePath == newValue)
                {
                    Label.text = $"{scenePath} <color=#808080>({index})</color>";
                    return;
                }
            }

            Label.text = $"<color=red>?</color> {(string.IsNullOrEmpty(newValue)? "": $"({newValue})")}";
        }
    }
}
#endif
