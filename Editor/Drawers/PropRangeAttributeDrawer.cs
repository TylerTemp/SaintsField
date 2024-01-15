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
                (string getError, float getValue) = Util.GetCallbackFloat(parentTarget, propRangeAttribute.MinCallback);
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
                (string getError, float getValue) = Util.GetCallbackFloat(parentTarget, propRangeAttribute.MaxCallback);
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
                // return newValue;
                return Mathf.Clamp(newValue, metaInfo.MinValue, metaInfo.MaxValue);
            }

            if (isFloat)
            {
                return Util.BoundFloatStep(newValue, metaInfo.MinValue, metaInfo.MaxValue, step);
            }

            return Util.BoundIntStep(newValue, metaInfo.MinValue, metaInfo.MaxValue, (int)step);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        #region UIToolkit

        private static string NameSlider(SerializedProperty property) => $"{property.propertyPath}__PropRange_Slider";
        private static string NameInteger(SerializedProperty property) => $"{property.propertyPath}__PropRange_IntegerField";
        private static string NameFloat(SerializedProperty property) => $"{property.propertyPath}__PropRange_FloatField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__PropRange_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, Label fakeLabel, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                }
            };

            Slider slider = new Slider(new string(' ', property.displayName.Length), 0, 1, SliderDirection.Horizontal, 0.5f)
            {
                name = NameSlider(property),
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            };
            root.Add(slider);

            const int width = 50;

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                root.Add(new IntegerField
                {
                    name = NameInteger(property),
                    value = property.intValue,
                    style =
                    {
                        // flexShrink = 0,
                        flexGrow = 0,
                        width = width,
                    },
                });
            }
            else
            {
                root.Add(new FloatField
                {
                    name = NameFloat(property),
                    value = property.floatValue,
                    style =
                    {
                        // flexShrink = 0,
                        flexGrow = 0,
                        width = width,
                    },
                });
            }

            return root;
            // MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, parent);
            // if(metaInfo.Error != "")
            // {
            //     _error = metaInfo.Error;
            //     return new VisualElement();
            // }
            //
            // bool isFloat = metaInfo.IsFloat;
            // float curValue = isFloat ? property.floatValue : property.intValue;
            // float minValue = metaInfo.MinValue;
            // float maxValue = metaInfo.MaxValue;
            //
            // // return container;
            // Slider element = new Slider(new string(' ', property.displayName.Length), minValue, maxValue, SliderDirection.Horizontal, curValue);
            // element.RegisterValueChangedCallback(changed =>
            // {
            //     float parsedValue = GetValue(metaInfo, changed.newValue);
            //     if (isFloat)
            //     {
            //         property.floatValue = parsedValue;
            //         onChange?.Invoke(parsedValue);
            //     }
            //     else
            //     {
            //         int intValue = (int)parsedValue;
            //         property.intValue = intValue;
            //         onChange?.Invoke(intValue);
            //     }
            //
            // });
            //
            // return element;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                },
                name = NameHelpBox(property),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, object parent)
        {
            Slider slider = container.Q<Slider>(NameSlider(property));

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, parent);
            bool isFloat = metaInfo.IsFloat;
            float curValue = isFloat ? property.floatValue : property.intValue;
            float minValue = metaInfo.MinValue;
            float maxValue = metaInfo.MaxValue;

            slider.lowValue = minValue;
            slider.highValue = maxValue;
            slider.value = curValue;
            slider.userData = metaInfo;

            IntegerField integerField = container.Q<IntegerField>(NameInteger(property));
            FloatField floatField = container.Q<FloatField>(NameFloat(property));

            if (isFloat)
            {
                floatField.value = curValue;
                floatField.RegisterValueChangedCallback(changed =>
                {
                    float parsedValue = GetValue(GetMetaInfo(property, saintsAttribute, parent), changed.newValue);
                    property.floatValue = parsedValue;
                    floatField.SetValueWithoutNotify(parsedValue);
                    slider.SetValueWithoutNotify(parsedValue);
                    onValueChangedCallback.Invoke(parsedValue);

                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            else
            {
                integerField.value = (int)curValue;
                integerField.RegisterValueChangedCallback(changed =>
                {
                    int parsedValue = (int)GetValue(GetMetaInfo(property, saintsAttribute, parent), changed.newValue);
                    property.intValue = parsedValue;
                    slider.SetValueWithoutNotify(parsedValue);
                    integerField.SetValueWithoutNotify(parsedValue);
                    onValueChangedCallback.Invoke(parsedValue);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            slider.RegisterValueChangedCallback(changed =>
            {
                float parsedValue = GetValue(GetMetaInfo(property, saintsAttribute, parent), changed.newValue);
                if (property.propertyType == SerializedPropertyType.Float)
                {
                    property.floatValue = parsedValue;
                    floatField.SetValueWithoutNotify(parsedValue);
                    slider.SetValueWithoutNotify(parsedValue);
                    onValueChangedCallback.Invoke(parsedValue);
                }
                else
                {
                    int intValue = (int)parsedValue;
                    property.intValue = intValue;
                    integerField.SetValueWithoutNotify(intValue);
                    slider.SetValueWithoutNotify(intValue);
                    onValueChangedCallback.Invoke(intValue);
                }

                property.serializedObject.ApplyModifiedProperties();
            });

            // ReSharper disable once InvertIf
            if (metaInfo.Error != "")
            {
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
                helpBox.text = metaInfo.Error;
                helpBox.style.display = DisplayStyle.Flex;
            }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, parent);

            Slider slider = container.Q<Slider>(NameSlider(property));
            MetaInfo curMetaInfo = (MetaInfo) slider.userData;

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            bool changed = false;

            if(metaInfo.Error != curMetaInfo.Error)
            {
                changed = true;
                helpBox.text = metaInfo.Error;
                helpBox.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(metaInfo.MinValue != curMetaInfo.MinValue
               // ReSharper disable once CompareOfFloatsByEqualityOperator
               || metaInfo.MaxValue != curMetaInfo.MaxValue)
            {
                changed = true;
                slider.lowValue = metaInfo.MinValue;
                slider.highValue = metaInfo.MaxValue;
            }

            if(changed)
            {
                slider.userData = metaInfo;
            }
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, string labelOrNull)
        {
            Slider target = container.Q<Slider>(NameSlider(property));
            target.label = labelOrNull;
        }

        #endregion
    }
}
