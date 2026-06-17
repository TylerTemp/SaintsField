using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.ValueButtonsDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(ValueButtonsAttribute), true)]
    [CustomPropertyDrawer(typeof(OptionsValueButtonsAttribute), true)]
    [CustomPropertyDrawer(typeof(PairsValueButtonsAttribute), true)]
    public partial class ValueButtonsAttributeDrawer: SaintsPropertyDrawer
    {
    }
}
