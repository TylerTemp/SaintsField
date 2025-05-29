using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.FullWidthRichLabelDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AboveRichLabelAttribute), true)]
    [CustomPropertyDrawer(typeof(BelowRichLabelAttribute), true)]
    [CustomPropertyDrawer(typeof(FullWidthRichLabelAttribute), true)]
    public partial class FullWidthRichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        // public bool IsSaintsPropertyDrawerOverrideLabel;
    }
}
