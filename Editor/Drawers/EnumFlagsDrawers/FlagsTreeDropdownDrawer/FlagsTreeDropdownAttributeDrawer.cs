using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.FlagsTreeDropdownDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(FlagsTreeDropdownAttribute), true)]
    public partial class FlagsTreeDropdownAttributeDrawer: SaintsPropertyDrawer
    {

    }
}
