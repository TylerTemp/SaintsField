using ExtInspector.Standalone;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CustomPropertyDrawer(typeof(BelowButtonAttribute))]
    public class BelowButtonAttributeDrawer: DecButtonAttributeDrawer
    {
        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute) => GetExtraHeight(property, label, width, saintsAttribute);


        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return true;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            // Debug.Log($"draw below {position}");
            return Draw(position, property, label, saintsAttribute);
        }
    }
}
