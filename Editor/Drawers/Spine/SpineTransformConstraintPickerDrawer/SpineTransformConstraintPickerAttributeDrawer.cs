using SaintsField.Editor.Core;
using SaintsField.Spine;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Spine.SpineTransformConstraintPickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SpineTransformConstraintPickerAttribute), true)]
    public partial class SpineTransformConstraintPickerAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
