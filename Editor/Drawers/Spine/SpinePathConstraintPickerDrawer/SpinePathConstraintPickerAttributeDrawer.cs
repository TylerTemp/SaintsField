using SaintsField.Editor.Core;
using SaintsField.Spine;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Spine.SpinePathConstraintPickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SpinePathConstraintPickerAttribute), true)]
    public partial class SpinePathConstraintPickerAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
