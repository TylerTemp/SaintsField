using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.TableDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(TableColumnAttribute), true)]
    public class DummyDrawer: SaintsPropertyDrawer
    {

    }
}
