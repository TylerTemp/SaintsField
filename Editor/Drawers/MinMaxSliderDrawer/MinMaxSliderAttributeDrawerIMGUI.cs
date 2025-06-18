using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.MinMaxSliderDrawer
{
    public partial class MinMaxSliderAttributeDrawer
    {

        private static readonly Dictionary<string, Vector2> IdToMinMaxRange = new Dictionary<string, Vector2>();

        private static string GetKey(SerializedProperty property) =>
            $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

        private string _error = "";
        private string _cacheKey = "";

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            IdToMinMaxRange.Remove(_cacheKey);
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            ImGuiEnsureDispose(property.serializedObject.targetObject);
            _cacheKey = GetKey(property);

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
            _error = metaInfo.Error;
            if (_error != "")
            {
                DefaultDrawer(position, property, label, info);
                return;
            }

            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;
            float minValue = metaInfo.MinValue;
            float maxValue = metaInfo.MaxValue;

            float labelWidth = label.text == "" ? 0 : EditorGUIUtility.labelWidth;

            float leftFieldWidth = property.propertyType == SerializedPropertyType.Vector2
                ? GetNumberFieldWidth(property.vector2Value.x, minMaxSliderAttribute.MinWidth,
                    minMaxSliderAttribute.MaxWidth)
                : GetNumberFieldWidth(property.vector2IntValue.x, minMaxSliderAttribute.MinWidth,
                    minMaxSliderAttribute.MaxWidth);
            leftFieldWidth += 5f;
            float rightFieldWidth = property.propertyType == SerializedPropertyType.Vector2
                ? GetNumberFieldWidth(property.vector2Value.y, minMaxSliderAttribute.MinWidth,
                    minMaxSliderAttribute.MaxWidth)
                : GetNumberFieldWidth(property.vector2IntValue.y, minMaxSliderAttribute.MinWidth,
                    minMaxSliderAttribute.MaxWidth);

            // float floatFieldWidth = EditorGUIUtility.fieldWidth;
            float sliderWidth = position.width - labelWidth - leftFieldWidth - rightFieldWidth;
            const float sliderPadding = 4f;

            (Rect labelWithMinFieldRect, Rect fieldRect) =
                RectUtils.SplitWidthRect(position, labelWidth + leftFieldWidth);

            (Rect sliderRect, Rect field3Rect) = RectUtils.SplitWidthRect(new Rect(fieldRect)
            {
                x = fieldRect.x + sliderPadding,
            }, sliderWidth - sliderPadding);

            (Rect maxFloatFieldRect, Rect _) = RectUtils.SplitWidthRect(new Rect(field3Rect)
            {
                x = field3Rect.x + sliderPadding,
            }, rightFieldWidth);

            bool freeInput = minMaxSliderAttribute.FreeInput;
            // Draw the slider
            ImGuiEnsureDispose(property.serializedObject.targetObject);
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                Vector2 sliderValue = property.vector2Value;

                if (IdToMinMaxRange.TryGetValue(GetKey(property), out Vector2 freeRange))
                {
                    minValue = Mathf.Min(minValue, freeInput ? freeRange.x : minValue, sliderValue.x);
                    maxValue = Mathf.Max(maxValue, freeInput ? freeRange.y : maxValue, sliderValue.y);
                    freeRange = new Vector2(minValue, maxValue);
                }
                else
                {
                    minValue = Mathf.Min(minValue, sliderValue.x);
                    maxValue = Mathf.Max(maxValue, sliderValue.y);
                    IdToMinMaxRange[GetKey(property)] = freeRange = new Vector2(minValue, maxValue);
                }

                bool hasChange = false;
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUI.MinMaxSlider(sliderRect, ref sliderValue.x, ref sliderValue.y, minValue, maxValue);
                    if (changed.changed)
                    {
                        Vector2 v = AdjustFloatSliderInput(sliderValue, minMaxSliderAttribute.Step, minValue, maxValue);
                        sliderValue.x = v.x;
                        sliderValue.y = v.y;
                        hasChange = true;
                    }
                }

                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    float sliderX = EditorGUI.FloatField(labelWithMinFieldRect, label, sliderValue.x);
                    if (changed.changed)
                    {
                        // sliderValue.x = minMaxSliderAttribute.FreeInput? sliderX: Mathf.Clamp(sliderX, minValue, Mathf.Min(maxValue, sliderValue.y));
                        Vector2 v = AdjustFloatInput(sliderX, sliderValue.y, minMaxSliderAttribute.Step, minValue,
                            maxValue,
                            minMaxSliderAttribute.FreeInput);
                        if (minMaxSliderAttribute.FreeInput && v.x < minValue)
                        {
                            freeRange.x = v.x;
                        }

                        sliderValue.x = v.x;
                        hasChange = true;
                    }
                }

                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    float sliderY = EditorGUI.FloatField(maxFloatFieldRect, sliderValue.y);
                    if (changed.changed)
                    {
                        // sliderValue.y = minMaxSliderAttribute.FreeInput? sliderY: Mathf.Clamp(sliderY, Mathf.Max(minValue, sliderValue.x), maxValue);
                        Vector2 v = AdjustFloatInput(sliderY, sliderValue.x, minMaxSliderAttribute.Step, minValue,
                            maxValue,
                            minMaxSliderAttribute.FreeInput);
                        if (minMaxSliderAttribute.FreeInput && v.y > maxValue)
                        {
                            freeRange.y = v.y;
                        }

                        sliderValue.y = v.y;
                        hasChange = true;
                    }
                }

                if (hasChange)
                {
                    property.vector2Value = sliderValue;
                    onGUIPayload.SetValue(sliderValue);
                    property.serializedObject.ApplyModifiedProperties();
                }

                IdToMinMaxRange[GetKey(property)] = freeRange;
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                Vector2 sliderValue = property.vector2IntValue;

                if (IdToMinMaxRange.TryGetValue(GetKey(property), out Vector2 freeRange))
                {
                    minValue = Mathf.Min(minValue, freeInput ? freeRange.x : minValue, sliderValue.x);
                    maxValue = Mathf.Max(maxValue, freeInput ? freeRange.y : maxValue, sliderValue.y);
                    freeRange = new Vector2(minValue, maxValue);
                }
                else
                {
                    minValue = Mathf.Min(minValue, sliderValue.x);
                    maxValue = Mathf.Max(maxValue, sliderValue.y);
                    IdToMinMaxRange[GetKey(property)] = freeRange = new Vector2(minValue, maxValue);
                }

                bool hasChange = false;
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUI.MinMaxSlider(sliderRect, ref sliderValue.x, ref sliderValue.y, minValue, maxValue);
                    if (changed.changed)
                    {
                        Vector2Int v = AdjustIntSliderInput(sliderValue, minMaxSliderAttribute.Step, minValue,
                            maxValue);
                        sliderValue.x = v.x;
                        sliderValue.y = v.y;
                        hasChange = true;
                    }
                }

                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int sliderX = EditorGUI.IntField(labelWithMinFieldRect, label, (int)sliderValue.x);
                    if (changed.changed)
                    {
                        // sliderValue.x = minMaxSliderAttribute.FreeInput? sliderX: Mathf.Clamp(sliderX, minValue, Mathf.Min(maxValue, sliderValue.y));
                        Vector2Int v = AdjustIntInput(sliderX, (int)sliderValue.y, minMaxSliderAttribute.Step, minValue,
                            maxValue,
                            minMaxSliderAttribute.FreeInput);
                        if (minMaxSliderAttribute.FreeInput && v.x < minValue)
                        {
                            freeRange.x = v.x;
                        }

                        sliderValue.x = v.x;
                        hasChange = true;
                    }
                }

                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int sliderY = EditorGUI.IntField(maxFloatFieldRect, (int)sliderValue.y);
                    if (changed.changed)
                    {
                        // sliderValue.y = minMaxSliderAttribute.FreeInput? sliderY: Mathf.Clamp(sliderY, Mathf.Max(minValue, sliderValue.x), maxValue);
                        Vector2Int v = AdjustIntInput(sliderY, (int)sliderValue.x, minMaxSliderAttribute.Step, minValue,
                            maxValue,
                            minMaxSliderAttribute.FreeInput);
                        if (minMaxSliderAttribute.FreeInput && v.y > maxValue)
                        {
                            freeRange.y = v.y;
                        }

                        sliderValue.y = v.y;
                        hasChange = true;
                    }
                }

                if (hasChange)
                {
                    property.vector2IntValue = new Vector2Int((int)sliderValue.x, (int)sliderValue.y);
                    onGUIPayload.SetValue(sliderValue);
                    property.serializedObject.ApplyModifiedProperties();
                }

                IdToMinMaxRange[GetKey(property)] = freeRange;
            }

            // ClickFocus(labelWithMinFieldRect, _fieldControlName);
        }

        private static float GetNumberFieldWidth(float value, float minWidth, float maxWidth) =>
            GetFieldWidth($"{value}", minWidth, maxWidth);

        private static float GetNumberFieldWidth(int value, float minWidth, float maxWidth) =>
            GetFieldWidth($"{value}", minWidth, maxWidth);

        private static float GetFieldWidth(string content, float minWidth, float maxWidth)
        {
            float actualWidth = EditorStyles.numberField.CalcSize(new GUIContent(content)).x;
            if (minWidth > 0 && actualWidth < minWidth)
            {
                return minWidth;
            }

            if (maxWidth > 0 && actualWidth > maxWidth)
            {
                return maxWidth;
            }

            return actualWidth;
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
            _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

    }
}
