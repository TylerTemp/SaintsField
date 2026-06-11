using SaintsField.Editor.Core;
using SaintsField.Spine;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Spine.SpineBonePickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SpineBonePickerAttribute), true)]
    public partial class SpineBonePickerAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
