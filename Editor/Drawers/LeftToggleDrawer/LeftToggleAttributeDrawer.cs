using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.LeftToggleDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(LeftToggleAttribute), true)]
    public partial class LeftToggleAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
