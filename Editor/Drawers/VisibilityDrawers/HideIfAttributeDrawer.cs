using System.Reflection;
using UnityEditor;

namespace SaintsField.Editor.Drawers.VisibilityDrawers
{
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfAttributeDrawer: ShowIfAttributeDrawer
    {
        protected override (string error, bool shown) IsShown(SerializedProperty property,  FieldInfo info, object target)
        {
            // reverse shown
            (string error, bool showResult) = base.IsShown(property, info, target);
            return (error, error != "" || !showResult);
        }
    }
}
