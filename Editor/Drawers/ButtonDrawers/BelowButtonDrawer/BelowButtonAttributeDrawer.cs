using SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer;
using UnityEditor;

namespace SaintsField.Editor.Drawers.ButtonDrawers.BelowButtonDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(BelowButtonAttribute), true)]
    public partial class BelowButtonAttributeDrawer: DecButtonAttributeDrawer
    {
    }
}
