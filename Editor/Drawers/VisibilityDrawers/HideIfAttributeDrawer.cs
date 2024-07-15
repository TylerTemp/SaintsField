using System.Reflection;
using UnityEditor;

namespace SaintsField.Editor.Drawers.VisibilityDrawers
{
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfAttributeDrawer: ShowIfAttributeDrawer
    {
        protected override (string error, bool shown) IsShown(SerializedProperty property,  FieldInfo info, object target)
        {
            // reverse, get shown
            return ("", !base.IsShown(property, info, target).shown);
        }
    }
}
