using SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer;
using UnityEditor;

namespace SaintsField.Editor.Drawers.ButtonDrawers.PostFieldButtonDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(PostFieldButtonAttribute), true)]
    public partial class PostFieldButtonAttributeDrawer : DecButtonAttributeDrawer
    {
    }
}
