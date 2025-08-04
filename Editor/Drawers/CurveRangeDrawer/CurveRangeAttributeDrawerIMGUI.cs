using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.CurveRangeDrawer
{
    public partial class CurveRangeAttributeDrawer
    {
        private string _error;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            _error = CheckHasError(property);
            if (_error != "")
            {
                DefaultDrawer(position, property, label, info);
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

        private static Rect GetRanges(CurveRangeAttribute curveRangeAttribute)
        {
            return new Rect(
                curveRangeAttribute.Min.x,
                curveRangeAttribute.Min.y,
                curveRangeAttribute.Max.x - curveRangeAttribute.Min.x,
                curveRangeAttribute.Max.y - curveRangeAttribute.Min.y);
        }

        private static string CheckHasError(SerializedProperty property)
        {
            return property.propertyType != SerializedPropertyType.AnimationCurve
                ? $"Requires AnimationCurve type, got {property.propertyType}"
                : "";
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

    }
}
