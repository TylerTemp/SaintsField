using SaintsField.AiNavigation;
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.AiNavigation.NavMeshAreaMaskDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(NavMeshAreaMaskAttribute), true)]
    public partial class NavMeshAreaMaskAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
