using SaintsField.Editor.Core;
using SaintsField.Spine;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Spine.SpineIkConstraintPickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SpineIkConstraintPickerAttribute), true)]
    public partial class SpineIkConstraintPickerAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
