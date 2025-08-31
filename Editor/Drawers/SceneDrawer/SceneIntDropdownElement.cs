#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Linq;
using SaintsField.Editor.UIToolkitElements;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
    public class SceneIntDropdownElement: IntDropdownElement
    {
        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;

            foreach ((string scenePath, int index) in SceneUtils.GetTrimedScenePath(false).WithIndex())
            {
                // ReSharper disable once InvertIf
                if (index == newValue)
                {
                    Label.text = $"{scenePath} <color=#808080>({index})</color>";
                    return;
                }

            }

            Label.text = $"<color=red>?</color> ({newValue})";
        }
    }
}
#endif
