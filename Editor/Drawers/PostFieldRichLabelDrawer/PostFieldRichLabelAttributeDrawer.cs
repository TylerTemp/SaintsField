using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.PostFieldRichLabelDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(EndTextAttribute), true)]
    public partial class PostFieldRichLabelAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
