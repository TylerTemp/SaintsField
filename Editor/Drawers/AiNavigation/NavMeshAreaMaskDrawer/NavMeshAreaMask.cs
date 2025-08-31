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
