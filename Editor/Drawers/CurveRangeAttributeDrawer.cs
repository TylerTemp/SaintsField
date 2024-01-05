using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(CurveRangeAttribute))]
    public class CurveRangeAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            if (CheckHasError(property))
            {
                DefaultDrawer(position, property, label);
                return;
            }

            CurveRangeAttribute curveRangeAttribute = (CurveRangeAttribute)saintsAttribute;
            Rect curveRanges = new Rect(
                curveRangeAttribute.Min.x,
                curveRangeAttribute.Min.y,
                curveRangeAttribute.Max.x - curveRangeAttribute.Min.x,
                curveRangeAttribute.Max.y - curveRangeAttribute.Min.y);


            EditorGUI.CurveField(
                position,
                property,
                curveRangeAttribute.Color.GetColor(),
                curveRanges,
                label);
        }

        private bool CheckHasError(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.AnimationCurve)
            {
                _error = $"Requires AnimationCurve type, got {property.propertyType}";
                return true;
            }

            _error = "";
            return false;
        }


        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) =>
            _error == ""
                ? position
                : HelpBox.Draw(position, _error, MessageType.Error);
    }
}
