using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderAttributeDrawer : SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabel)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
            {
                _error = $"Expect Vector2 or Vector2Int, get {property.propertyType}";
                DefaultDrawer(position, property);
                return;
            }

            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;
            float minValue = minMaxSliderAttribute.Min;
            float maxValue = minMaxSliderAttribute.Max;

            float leftFieldWidth = property.propertyType == SerializedPropertyType.Vector2
                ? GetNumberFieldWidth(property.vector2Value.x, minMaxSliderAttribute.MinWidth, minMaxSliderAttribute.MaxWidth)
                : GetNumberFieldWidth(property.vector2IntValue.x, minMaxSliderAttribute.MinWidth, minMaxSliderAttribute.MaxWidth);
            float rightFieldWidth = property.propertyType == SerializedPropertyType.Vector2
                ? GetNumberFieldWidth(property.vector2Value.y, minMaxSliderAttribute.MinWidth, minMaxSliderAttribute.MaxWidth)
                : GetNumberFieldWidth(property.vector2IntValue.y, minMaxSliderAttribute.MinWidth, minMaxSliderAttribute.MaxWidth);

            // float floatFieldWidth = EditorGUIUtility.fieldWidth;
            float sliderWidth = position.width - leftFieldWidth - rightFieldWidth;
            float sliderPadding = 5.0f;

            Rect sliderRect = new Rect(
                position.x + leftFieldWidth + sliderPadding,
                position.y,
                sliderWidth - 2.0f * sliderPadding,
                position.height);

            Rect minFloatFieldRect = new Rect(
                position.x,
                position.y,
                leftFieldWidth,
                position.height);

            Rect maxFloatFieldRect = new Rect(
                position.x  + leftFieldWidth + sliderWidth,
                position.y,
                rightFieldWidth,
                position.height);

            // Draw the slider
            EditorGUI.BeginChangeCheck();

            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                Vector2 sliderValue = property.vector2Value;
                EditorGUI.MinMaxSlider(sliderRect, ref sliderValue.x, ref sliderValue.y, minValue, maxValue);

                sliderValue.x = EditorGUI.FloatField(minFloatFieldRect, sliderValue.x);
                sliderValue.x = Mathf.Clamp(sliderValue.x, minValue, Mathf.Min(maxValue, sliderValue.y));

                sliderValue.y = EditorGUI.FloatField(maxFloatFieldRect, sliderValue.y);
                sliderValue.y = Mathf.Clamp(sliderValue.y, Mathf.Max(minValue, sliderValue.x), maxValue);

                if (EditorGUI.EndChangeCheck())
                {
                    property.vector2Value = minMaxSliderAttribute.Step < 0
                        ? sliderValue
                        : BoundV2Step(sliderValue, minValue, maxValue, minMaxSliderAttribute.Step);
                }
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                Vector2Int sliderValue = property.vector2IntValue;
                float xValue = sliderValue.x;
                float yValue = sliderValue.y;
                EditorGUI.MinMaxSlider(sliderRect, ref xValue, ref yValue, minValue, maxValue);

                sliderValue.x = EditorGUI.IntField(minFloatFieldRect, (int)xValue);
                sliderValue.x = (int)Mathf.Clamp(sliderValue.x, minValue, Mathf.Min(maxValue, sliderValue.y));

                sliderValue.y = EditorGUI.IntField(maxFloatFieldRect, (int)yValue);
                sliderValue.y = (int)Mathf.Clamp(sliderValue.y, Mathf.Max(minValue, sliderValue.x), maxValue);

                if (EditorGUI.EndChangeCheck())
                {
                    // Debug.Log(sliderValue);
                    int actualStep = Mathf.Max(1, Mathf.RoundToInt(minMaxSliderAttribute.Step));
                    property.vector2IntValue = actualStep == 1
                        ? sliderValue
                        : BoundV2IntStep(sliderValue, minValue, maxValue, actualStep);
                }
            }
        }

        private float GetNumberFieldWidth(float value, float minWidth, float maxWidth) => GetFieldWidth($"{value}", minWidth, maxWidth);
        private float GetNumberFieldWidth(int value, float minWidth, float maxWidth) => GetFieldWidth($"{value}", minWidth, maxWidth);

        private float GetFieldWidth(string content, float minWidth, float maxWidth)
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

        private Vector2 BoundV2Step(Vector2 curValue, float min, float max, float step)
        {
            return new Vector2(
                BoundFloatStep(curValue.x, min, max, step),
                BoundFloatStep(curValue.y, min, max, step)
            );
        }

        private float BoundFloatStep(float curValue, float start, float end, float step)
        {
            float distance = curValue - start;
            int stepRound = Mathf.RoundToInt(distance / step);
            float newValue = start + stepRound * step;
            return Mathf.Min(newValue, end);
        }

        private Vector2Int BoundV2IntStep(Vector2Int curValue, float min, float max, int step)
        {
            return new Vector2Int(
                BoundIntStep(curValue.x, min, max, step),
                BoundIntStep(curValue.y, min, max, step)
            );
        }

        private int BoundIntStep(float curValue, float start, float end, int step)
        {
            int useStart = Mathf.CeilToInt(start);
            int useEnd = Mathf.FloorToInt(end);

            float distance = curValue - useStart;
            int stepRound = Mathf.RoundToInt(distance / step);
            int newValue = useStart + stepRound * step;
            return Mathf.Clamp(newValue, useStart, useEnd);
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : HelpBox.Draw(position, _error, MessageType.Error);
    }
}
