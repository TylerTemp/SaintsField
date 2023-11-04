using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(FieldDrawerConfigAttribute))]
    public class FieldDrawerConfigAttributeDrawer: SaintsPropertyDrawer
    {
        protected override bool WillDrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return true;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return 0;
        }

        protected override Rect DrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return position;
        }
    }
}
