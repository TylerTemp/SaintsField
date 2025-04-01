using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.TagDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(TagAttribute), true)]
    public partial class TagAttributeDrawer: SaintsPropertyDrawer
    {

    }
}
