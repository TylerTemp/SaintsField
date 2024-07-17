using System.Reflection;
using UnityEditor;

namespace SaintsField.Editor.Drawers.DisabledDrawers
{
    [CustomPropertyDrawer(typeof(EnableIfAttribute))]
    public class EnableIfAttributeDrawer: ReadOnlyAttributeDrawer
    {
        protected override (string error, bool disabled) IsDisabled(SerializedProperty property, FieldInfo info, object target)
        {
            // reverse, get disabled
            return ("", !base.IsDisabled(property, info, target).disabled);
        }
    }
}
