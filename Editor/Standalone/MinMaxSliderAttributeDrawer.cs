using ExtInspector.Editor.Utils;
using ExtInspector.Standalone;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor.Standalone
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderAttributeDrawer : SaintsPropertyDrawer
    {
        private const string KVectorMinName = "x";
        private const string KVectorMaxName = "y";
        private const float KSpacing = 2f;
        private const float KRoundingValue = 100f;

        private static readonly int ControlHash = "Foldout".GetHashCode();
        private static readonly GUIContent Unsupported = EditorGUIUtility.TrTextContent("Unsupported field type");

        private bool _pressed;

        private static float Round(float value, float roundingValue)
        {
            return roundingValue == 0 ? value : Mathf.Round(value * roundingValue) / roundingValue;
        }

        private static float FlexibleFloatFieldWidth(float min, float max)
        {
            float n = Mathf.Max(Mathf.Abs(min), Mathf.Abs(max));
            return 14f + Mathf.Floor(Mathf.Log10(Mathf.Abs(n)) + 1) * 2.5f;
        }

        private static void SetVectorValue(SerializedProperty property, ref float min, ref float max, bool round)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2:
                {
                    if (round)
                    {
                        min = Round(min, KRoundingValue);
                        max = Round(max, KRoundingValue);
                    }

                    property.vector2Value = new Vector2(min, max);
                }
                    break;
                case SerializedPropertyType.Vector2Int:
                {
                    property.vector2IntValue = new Vector2Int((int)min, (int)max);
                }
                    break;
                // default:
                //     break;
            }
        }

        protected override float GetLabelFieldHeight(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return base.GetPropertyHeight(property, label);
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            // base.OnGUI(position, property, label);

            float min, max;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2:
                {
                    Vector2 v = property.vector2Value;
                    min = v.x;
                    max = v.y;
                }
                    break;
                case SerializedPropertyType.Vector2Int:
                {
                    Vector2Int v = property.vector2IntValue;
                    min = v.x;
                    max = v.y;
                }
                    break;
                default:
                    EditorGUI.LabelField(position, label, Unsupported);
                    return;
            }

            // MinMaxSliderAttribute attr = (MinMaxSliderAttribute)attribute;
            // MinMaxSliderAttribute attr = SerializedUtil.GetAttribute<MinMaxSliderAttribute>(property);;
            MinMaxSliderAttribute attr = (MinMaxSliderAttribute)saintsAttribute;

            float ppp = EditorGUIUtility.pixelsPerPoint;
            float spacing = KSpacing * ppp;
            float fieldWidth = ppp * FlexibleFloatFieldWidth(attr.Min, attr.Max);

            int indent = EditorGUI.indentLevel;

            // int id = GUIUtility.GetControlID(ControlHash, FocusType.Keyboard, position);
            // Rect r = EditorGUI.PrefixLabel(position, id, label);

            Rect sliderPos = position;

            sliderPos.x += fieldWidth + spacing;
            sliderPos.width -= (fieldWidth + spacing) * 2;

            if (Event.current.type == EventType.MouseDown &&
                sliderPos.Contains(Event.current.mousePosition))
            {
                _pressed = true;
                min = Mathf.Clamp(min, attr.Min, attr.Max);
                max = Mathf.Clamp(max, attr.Min, attr.Max);
                SetVectorValue(property, ref min, ref max, attr.Round);
                GUIUtility.keyboardControl = 0; // TODO keep focus but stop editing
            }

            if (_pressed && Event.current.type == EventType.MouseUp)
            {
                if (attr.Round)
                {
                    SetVectorValue(property, ref min, ref max, true);
                }

                _pressed = false;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.indentLevel = 0;
            EditorGUI.MinMaxSlider(sliderPos, ref min, ref max, attr.Min, attr.Max);
            EditorGUI.indentLevel = indent;
            if (EditorGUI.EndChangeCheck())
            {
                SetVectorValue(property, ref min, ref max, false);
            }

            Rect minPos = position;
            minPos.width = fieldWidth;

            SerializedProperty vectorMinProp = property.FindPropertyRelative(KVectorMinName);
            EditorGUI.showMixedValue = vectorMinProp.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            EditorGUI.indentLevel = 0;
            min = EditorGUI.DelayedFloatField(minPos, min);
            EditorGUI.indentLevel = indent;
            if (EditorGUI.EndChangeCheck())
            {
                if (attr.Bound)
                {
                    min = Mathf.Max(min, attr.Min);
                    min = Mathf.Min(min, max);
                }

                SetVectorValue(property, ref min, ref max, attr.Round);
            }

            vectorMinProp.Dispose();

            Rect maxPos = position;
            maxPos.x += maxPos.width - fieldWidth;
            maxPos.width = fieldWidth;

            SerializedProperty vectorMaxProp = property.FindPropertyRelative(KVectorMaxName);
            EditorGUI.showMixedValue = vectorMaxProp.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            EditorGUI.indentLevel = 0;
            max = EditorGUI.DelayedFloatField(maxPos, max);
            EditorGUI.indentLevel = indent;
            if (EditorGUI.EndChangeCheck())
            {
                if (attr.Bound)
                {
                    max = Mathf.Min(max, attr.Max);
                    max = Mathf.Max(max, min);
                }

                SetVectorValue(property, ref min, ref max, attr.Round);
            }

            vectorMaxProp.Dispose();

            EditorGUI.showMixedValue = false;
        }
    }
}
