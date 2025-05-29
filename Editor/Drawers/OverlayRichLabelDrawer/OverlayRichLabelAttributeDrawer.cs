using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.OverlayRichLabelDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(OverlayRichLabelAttribute), true)]
    public partial class OverlayRichLabelAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
