using SaintsField.Editor.Core;
using SaintsField.Spine;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Spine.SpineEventPickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SpineEventPickerAttribute), true)]
    public partial class SpineEventPickerAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
