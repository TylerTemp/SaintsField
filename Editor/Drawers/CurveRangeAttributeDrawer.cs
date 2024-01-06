using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
            _error = CheckHasError(property);
            if (_error != "")
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
            return property.propertyType != SerializedPropertyType.AnimationCurve ? $"Requires AnimationCurve type, got {property.propertyType}" : "";
        }


        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);


        #region UIToolkit

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, object parent,
            Action<object> onChange)
        {
            CurveField element = new CurveField(property.displayName)
            {
                value = property.animationCurveValue,
                ranges = GetRanges((CurveRangeAttribute) saintsAttribute),
                // color wont work for it
                // style =
                // {
                //     color = Color.red,
                // },
            };

            element.RegisterValueChangedCallback(v =>
            {
                property.animationCurveValue = v.newValue;
                property.serializedObject.ApplyModifiedProperties();
                onChange?.Invoke(v.newValue);
            });

            return element;
        }

        #endregion
    }
}
