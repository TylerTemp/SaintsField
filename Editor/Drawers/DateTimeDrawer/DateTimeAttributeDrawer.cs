using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(DateTimeAttribute), true)]
    public partial class DateTimeAttributeDrawer : SaintsPropertyDrawer
    {
    }
}
