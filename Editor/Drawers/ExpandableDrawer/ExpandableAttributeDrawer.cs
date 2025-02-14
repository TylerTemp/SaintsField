using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.ExpandableDrawer
{

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ExpandableAttribute), true)]
    public partial class ExpandableAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
