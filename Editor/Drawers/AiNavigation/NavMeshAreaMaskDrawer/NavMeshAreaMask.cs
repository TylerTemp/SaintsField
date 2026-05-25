#if UNITY_2021_2_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.UIToolkitElements;

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaMaskDrawer
{
    public class NavMeshAreaMask: IntDropdownElement
    {
        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;


        }
    }
}
#endif
