using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.PostFieldRichLabelDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(PostFieldRichLabelAttribute), true)]
    public partial class PostFieldRichLabelAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
