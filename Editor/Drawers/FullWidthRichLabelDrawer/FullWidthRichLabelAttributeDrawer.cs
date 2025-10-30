using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.FullWidthRichLabelDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(FieldAboveTextAttribute), true)]
    [CustomPropertyDrawer(typeof(FieldBelowTextAttribute), true)]
    [CustomPropertyDrawer(typeof(FullWidthRichLabelAttribute), true)]
    public partial class FullWidthRichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        // public bool IsSaintsPropertyDrawerOverrideLabel;
    }
}
