using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PropRangeAttribute))]
    public class PropRangeAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        private struct MetaInfo
        {
            public bool IsFloat;
            public float MinValue;
            public float MaxValue;
            public float Step;
            public string Error;
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute, object parentTarget)
        {
            PropRangeAttribute propRangeAttribute = (PropRangeAttribute) saintsAttribute;

            bool isFloat = property.propertyType == SerializedPropertyType.Float;
            // object parentTarget = GetParentTarget(property);
            string error = "";

            float minValue;
            if (propRangeAttribute.MinCallback == null)
            {
                minValue = propRangeAttribute.Min;
            }
            else
            {
                (float getValue, string getError) = Util.GetCallbackFloat(parentTarget, propRangeAttribute.MinCallback);
                error = getError;
                minValue = getValue;
            }

            float maxValue;
            if (propRangeAttribute.MaxCallback == null)
            {
                maxValue = propRangeAttribute.Max;
            }
            else
            {
                (float getValue, string getError) = Util.GetCallbackFloat(parentTarget, propRangeAttribute.MaxCallback);
                error = getError;
                maxValue = getValue;
            }

            if (error != "")
            {
                return new MetaInfo
                {
                    IsFloat = isFloat,
                    Error = error,
                };
            }

            if (maxValue < minValue)
            {
                return new MetaInfo
                {
                    IsFloat = isFloat,
                    Error = $"max({maxValue}) should be greater than min({minValue})",
                };
            }

            return new MetaInfo
            {
                IsFloat = isFloat,
                MinValue = minValue,
                MaxValue = maxValue,
                Step = propRangeAttribute.Step,
                Error = error,
            };
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parentTarget)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS
            Debug.Log($"#PropRange# #DrawField# for {property.propertyPath}");
#endif

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, parentTarget);
            if(metaInfo.Error != "")
            {
                _error = metaInfo.Error;
                DefaultDrawer(position, property, label);
                return;
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool isFloat = metaInfo.IsFloat;
                float curValue = isFloat ? property.floatValue : property.intValue;
                float minValue = metaInfo.MinValue;
                float maxValue = metaInfo.MaxValue;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS
                Debug.Log($"#PropRange# #DrawField# for {property.propertyPath}: {minValue}~{maxValue} {curValue}");
#endif
                float newValue = EditorGUI.Slider(position, label, curValue, minValue, maxValue);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    float parsedValue = GetValue(metaInfo, newValue);
                    if (isFloat)
                    {
                        property.floatValue = parsedValue;
                    }
                    else
                    {
                        property.intValue = (int)parsedValue;
                    }
                }
            }
        }

        private float GetValue(MetaInfo metaInfo, float newValue)
        {
            // property.floatValue = newValue;
            float step = metaInfo.Step;
            bool isFloat = metaInfo.IsFloat;
            // Debug.Log(step);
            if (step <= 0)
            {
                return newValue;
            }
            else
            {
                if (isFloat)
                {
                    return Util.BoundFloatStep(newValue, metaInfo.MinValue, metaInfo.MaxValue, step);
                }
                else
                {
                    return Util.BoundIntStep(newValue, metaInfo.MinValue, metaInfo.MaxValue, (int)step);
                }
            }
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute, object parent, Action<object> onChange)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, parent);
            if(metaInfo.Error != "")
            {
                _error = metaInfo.Error;
                // return DefaultDrawerUIToolKit(property, labelState);
                return new VisualElement();
            }

            // VisualElement container = new VisualElement();

            bool isFloat = metaInfo.IsFloat;
            float curValue = isFloat ? property.floatValue : property.intValue;
            float minValue = metaInfo.MinValue;
            float maxValue = metaInfo.MaxValue;

            // string label;
            // // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            // switch (labelState)
            // {
            //     case LabelState.AsIs:
            //         label = null;
            //         break;
            //     case LabelState.None:
            //         label = "";
            //         break;
            //     case LabelState.EmptySpace:
            //         label = " ";
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(labelState), labelState, null);
            // }

            // return container;
            Slider element = new Slider(property.displayName, minValue, maxValue, SliderDirection.Horizontal, curValue);
            element.RegisterValueChangedCallback(changed =>
            {
                float parsedValue = GetValue(metaInfo, changed.newValue);
                if (isFloat)
                {
                    property.floatValue = parsedValue;
                    onChange?.Invoke(parsedValue);
                }
                else
                {
                    int intValue = (int)parsedValue;
                    property.intValue = intValue;
                    onChange?.Invoke(intValue);
                }

            });

            return element;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

    }
}
