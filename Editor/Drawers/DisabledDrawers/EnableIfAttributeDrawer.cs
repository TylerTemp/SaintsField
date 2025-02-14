using System.Reflection;
using UnityEditor;

namespace SaintsField.Editor.Drawers.DisabledDrawers
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(EnableIfAttribute), true)]
    public class EnableIfAttributeDrawer: ReadOnlyAttributeDrawer
    {
        protected override (string error, bool disabled) IsDisabled(SerializedProperty property, FieldInfo info, object target)
        {
            // reverse, get disabled
            return ("", !base.IsDisabled(property, info, target).disabled);
        }
    }
}
