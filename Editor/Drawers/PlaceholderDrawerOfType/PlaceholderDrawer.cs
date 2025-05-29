using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.PlaceholderDrawerOfType
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(Placeholder))]
    public partial class PlaceholderDrawer: SaintsPropertyDrawer
    {
    }
}
