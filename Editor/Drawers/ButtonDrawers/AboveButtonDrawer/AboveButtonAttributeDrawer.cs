using SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer;
using UnityEditor;

namespace SaintsField.Editor.Drawers.ButtonDrawers.AboveButtonDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AboveButtonAttribute), true)]
    public partial class AboveButtonAttributeDrawer: DecButtonAttributeDrawer
    {
    }
}
