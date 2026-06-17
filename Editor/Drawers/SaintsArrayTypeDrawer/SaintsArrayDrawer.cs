using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SaintsArrayTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsArrayAttribute), true)]
    [CustomPropertyDrawer(typeof(SaintsList<>), true)]
    [CustomPropertyDrawer(typeof(SaintsArray<>), true)]
    public partial class SaintsArrayDrawer: SaintsPropertyDrawer
    {
        private const double DebounceTime = 0.6d;
    }
}
