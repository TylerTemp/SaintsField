using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(CurveRangeAttribute))]
    public class CurveRangeAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error;

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
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
            return property.propertyType != SerializedPropertyType.AnimationCurve ? $"Requires AnimationCurve type, got {property.propertyType}" : "";
        }


        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameCurveField(SerializedProperty property) => $"{property.propertyPath}__CurveRange";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent)
        {
            CurveRangeAttribute curveRangeAttribute = (CurveRangeAttribute)saintsAttribute;
            CurveField createFieldElement = new CurveField(property.displayName)
            {
                value = property.animationCurveValue,
                ranges = GetRanges(curveRangeAttribute),
                name = NameCurveField(property),
                // style =
                // {
                //     unity
                // }
            };

            Type type = typeof(CurveField);
            FieldInfo colorFieldInfo = type.GetField("m_CurveColor", BindingFlags.NonPublic | BindingFlags.Instance);
            if (colorFieldInfo != null)
            {
                colorFieldInfo.SetValue(createFieldElement, curveRangeAttribute.Color.GetColor());
            }

            createFieldElement.AddToClassList(ClassAllowDisable);
            createFieldElement.AddToClassList("unity-base-field__aligned");

            return createFieldElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            container.Q<CurveField>(NameCurveField(property)).RegisterValueChangedCallback(v =>
            {
                property.animationCurveValue = v.newValue;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(v.newValue);
            });
        }

        // protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
        //     IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        // {
        //     CurveField target = container.Q<CurveField>(NameCurveField(property));
        //     target.label = labelOrNull;
        // }

        #endregion

#endif
    }
}
