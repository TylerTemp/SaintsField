using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(TreeDropdownAttribute), true)]
    public partial class TreeDropdownAttributeDrawer: SaintsPropertyDrawer
    {

    }
}
