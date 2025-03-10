using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.DefaultExpandDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(DefaultExpandAttribute), true)]
    public partial class DefaultExpandAttributeDrawer: SaintsPropertyDrawer
    {

    }
}
