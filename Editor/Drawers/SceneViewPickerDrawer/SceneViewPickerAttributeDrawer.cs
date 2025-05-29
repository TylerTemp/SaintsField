using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SceneViewPickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SceneViewPickerAttribute), true)]
    public partial class SceneViewPickerAttributeDrawer: SaintsPropertyDrawer
    {

    }
}
