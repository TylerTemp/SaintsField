using SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer;
using UnityEditor;

namespace SaintsField.Editor.Drawers.ButtonDrawers.BelowButtonDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(BelowButtonAttribute), true)]
    public partial class BelowButtonAttributeDrawer: DecButtonAttributeDrawer
    {
    }
}
