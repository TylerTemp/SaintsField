#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.MinMaxSliderDrawer
{
    public partial class MinMaxSliderAttributeDrawer
    {

        private static string NameMinMaxSliderField(SerializedProperty property) =>
            $"{property.propertyPath}__MinMaxSliderField";

        public class MinMaxSliderField : BaseField<Vector2>
        {
            public MinMaxSliderField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static string NameSlider(SerializedProperty property) =>
            $"{property.propertyPath}__MinMaxSlider_Slider";

        private static string NameMinInteger(SerializedProperty property) =>
            $"{property.propertyPath}__MinMaxSlider_MinIntegerField";

        private static string NameMaxInteger(SerializedProperty property) =>
            $"{property.propertyPath}__MinMaxSlider_MaxIntegerField";

        private static string NameMinFloat(SerializedProperty property) =>
            $"{property.propertyPath}__MinMaxSlider_MinFloatField";

        private static string NameMaxFloat(SerializedProperty property) =>
            $"{property.propertyPath}__MinMaxSlider_MaxFloatField";

        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__MinMaxSlider_HelpBox";

        private const int InputWidth = 50;

        private record UserData
        {
            public float FreeMin;
            public float FreeMax;
        }

        // Oh hell... I don't want to properly do this anymore
        private class BindableV2IntField: BaseField<Vector2Int>
        {
            public BindableV2IntField(VisualElement visualInput) : base(null, visualInput)
            {
                style.marginLeft = style.marginRight = 0;
            }
        }

        public class BindableV2Field : BaseField<Vector2>
        {
            public BindableV2Field(VisualElement visualInput) : base(null, visualInput)
            {
                style.marginLeft = style.marginRight = 0;
            }
        }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
            {
                PropertyField fallback = PropertyFieldFallbackUIToolkit(property, GetPreferredLabel(property));
                fallback.AddToClassList(ClassFieldUIToolkit(property));
                return fallback;
            }

            bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };

            Vector2 sliderValue = isInt ? property.vector2IntValue : property.vector2Value;

            MinMaxSlider minMaxSlider = new MinMaxSlider(sliderValue.x, sliderValue.y,
                Mathf.Min(sliderValue.x, sliderValue.y), Mathf.Max(sliderValue.x, sliderValue.y))
            {
                name = NameSlider(property),
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    paddingLeft = 5,
                    paddingRight = 5,
                },
                userData = new UserData
                {
                    FreeMin = sliderValue.x,
                    FreeMax = sliderValue.y,
                },
            };

            if (isInt)
            {
                Vector2Int curValue = property.vector2IntValue;

                root.Add(new IntegerField
                {
                    isDelayed = true,
                    value = curValue.x,
                    name = NameMinInteger(property),
                    style =
                    {
                        flexGrow = 0,
                        width = InputWidth,
                    },
                });
                root.Add(minMaxSlider);
                root.Add(new IntegerField
                {
                    isDelayed = true,
                    value = curValue.y,
                    name = NameMaxInteger(property),
                    style =
                    {
                        flexGrow = 0,
                        width = InputWidth,
                    },
                });
            }
            else
            {
                Vector2 curValue = property.vector2Value;
                // slider.SetValueWithoutNotify(curValue);

                root.Add(new FloatField
                {
                    isDelayed = true,
                    value = curValue.x,
                    name = NameMinFloat(property),
                    style =
                    {
                        flexGrow = 0,
                        width = InputWidth,
                    },
                });
                root.Add(minMaxSlider);
                root.Add(new FloatField
                {
                    isDelayed = true,
                    value = curValue.y,
                    name = NameMaxFloat(property),
                    style =
                    {
                        flexGrow = 0,
                        width = InputWidth,
                    },
                });
            }

            MinMaxSliderField minMaxSliderField = new MinMaxSliderField(GetPreferredLabel(property), root)
            {
                name = NameMinMaxSliderField(property),
            };
            minMaxSliderField.labelElement.style.overflow = Overflow.Hidden;
            minMaxSliderField.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);

            minMaxSliderField.AddToClassList(ClassAllowDisable);

            if (isInt)
            {
                BindableV2IntField wrapper = new BindableV2IntField(minMaxSliderField);
                wrapper.BindProperty(property);
                return wrapper;
            }
            else
            {
                BindableV2Field wrapper = new BindableV2Field(minMaxSliderField);
                wrapper.BindProperty(property);
                return wrapper;
            }

            // return minMaxSliderField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                },
                name = NameHelpBox(property),
            };

            helpBox.AddToClassList(ClassAllowDisable);

            // ReSharper disable once InvertIf
            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
            {
                helpBox.text = $"Expect Vector2/Vector2Int, get {property.propertyType}";
                helpBox.style.display = DisplayStyle.Flex;
            }

            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
            {
                return;
            }
            MinMaxSliderField minMaxSliderField =
                container.Q<MinMaxSliderField>(NameMinMaxSliderField(property));

            UIToolkitUtils.AddContextualMenuManipulator(minMaxSliderField.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;

            MinMaxSlider minMaxSlider = container.Q<MinMaxSlider>(NameSlider(property));
            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;
            UserData userData = (UserData)minMaxSlider.userData;

            // That's the KEY!!!!!!!!!!!!!
            minMaxSlider.TrackPropertyValue(property, p =>
            {
                Vector2 newValue = property.propertyType == SerializedPropertyType.Vector2Int
                    ? p.vector2IntValue
                    : p.vector2Value;

                if (newValue.y < newValue.x)
                {
                    newValue.y = newValue.x;
                }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                Debug.Log($"slider update value {newValue} while self range {minMaxSlider.lowLimit}~{minMaxSlider.highLimit}");
#endif
                if (minMaxSlider.lowLimit > newValue.x)
                {
                    minMaxSlider.lowLimit = newValue.x;
                }

                if (minMaxSlider.highLimit < newValue.y)
                {
                    minMaxSlider.highLimit = newValue.y;
                }

                minMaxSlider.SetValueWithoutNotify(newValue);
            });

            MetaInfo metaInfo = GetMetaInfo(property, minMaxSliderAttribute, info, parent);
            userData.FreeMin = metaInfo.MinValue;
            userData.FreeMax = metaInfo.MaxValue;
            // Debug.Log($"{minMaxSlider.highLimit}~{minMaxSlider.lowLimit}: {metaInfo.MinValue}~{metaInfo.MaxValue}");
            if (metaInfo.MinValue >= minMaxSlider.highLimit)
            {
                minMaxSlider.highLimit = metaInfo.MaxValue;
                minMaxSlider.lowLimit = metaInfo.MinValue;
            }
            else
            {
                minMaxSlider.lowLimit = metaInfo.MinValue;
                minMaxSlider.highLimit = metaInfo.MaxValue;
            }

            AdjustFreeRangeHighAndLow(isInt ? property.vector2IntValue : property.vector2Value, property,
                minMaxSliderAttribute, container, info, parent);

            if (isInt)
            {
                IntegerField minIntField = container.Q<IntegerField>(NameMinInteger(property));
                IntegerField maxIntField = container.Q<IntegerField>(NameMaxInteger(property));
                minMaxSlider.RegisterValueChangedCallback(changed =>
                {
                    float min = userData.FreeMin;
                    float max = userData.FreeMax;
                    Vector2Int inputValue =
                        AdjustIntSliderInput(changed.newValue, minMaxSliderAttribute.Step, min, max);
                    ApplyIntValue(property, inputValue, onValueChangedCallback, minMaxSliderAttribute, container, info,
                        parent);
                });
                minIntField.RegisterValueChangedCallback(changed =>
                {
                    int newValue = changed.newValue;
                    Vector2Int inputValue = AdjustIntInput(newValue, maxIntField.value, minMaxSliderAttribute.Step,
                        (int)userData.FreeMin, (int)userData.FreeMax, minMaxSliderAttribute.FreeInput);
                    ApplyIntValue(property,
                        inputValue, onValueChangedCallback, minMaxSliderAttribute, container, info, parent);
                    // if (minMaxSliderAttribute.FreeInput)
                    // {
                    //     userData.FreeMin = Mathf.Min(userData.FreeMin, inputValue.x);
                    //     userData.FreeMax = Mathf.Max(userData.FreeMax, inputValue.y);
                    //     // Debug.Log($"update min max {userData.FreeMin}~{userData.FreeMax}");
                    // }
                });
                maxIntField.RegisterValueChangedCallback(changed =>
                {
                    int newValue = changed.newValue;
                    Vector2Int inputValue = AdjustIntInput(newValue, minIntField.value, minMaxSliderAttribute.Step,
                        (int)userData.FreeMin, (int)userData.FreeMax, minMaxSliderAttribute.FreeInput);
                    ApplyIntValue(property,
                        inputValue, onValueChangedCallback, minMaxSliderAttribute, container, info, parent);
                    // if (minMaxSliderAttribute.FreeInput)
                    // {
                    //     userData.FreeMin = Mathf.Min(userData.FreeMin, inputValue.x);
                    //     userData.FreeMax = Mathf.Max(userData.FreeMax, inputValue.y);
                    //     // Debug.Log($"update min max {userData.FreeMin}~{userData.FreeMax}");
                    // }
                });
            }
            else
            {
                FloatField minFloatField = container.Q<FloatField>(NameMinFloat(property));
                FloatField maxFloatField = container.Q<FloatField>(NameMaxFloat(property));

                minMaxSlider.RegisterValueChangedCallback(changed =>
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                    Debug.Log($"slider changed: {changed.newValue}");
#endif
                    float min = userData.FreeMin;
                    float max = userData.FreeMax;
                    Vector2 inputValue = AdjustFloatSliderInput(changed.newValue, minMaxSliderAttribute.Step, min, max);
                    ApplyFloatValue(property, inputValue, onValueChangedCallback, minMaxSliderAttribute, container,
                        info, parent);
                });
                minFloatField.RegisterValueChangedCallback(changed =>
                {
                    float newValue = changed.newValue;
                    Vector2 inputValue = AdjustFloatInput(newValue, maxFloatField.value, minMaxSliderAttribute.Step,
                        userData.FreeMin, userData.FreeMax, minMaxSliderAttribute.FreeInput);
                    ApplyFloatValue(property,
                        inputValue, onValueChangedCallback, minMaxSliderAttribute, container, info, parent);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                    Debug.Log($"minFloat onChange changed={newValue}, maxValue={maxFloatField.value}, inputValue={inputValue}, {userData.FreeMin}, {userData.FreeMax}");
#endif
                });
                maxFloatField.RegisterValueChangedCallback(changed =>
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                    Debug.Log($"changed={changed.newValue}, minValue={minFloatField.value}");
#endif
                    float newValue = changed.newValue;
                    Vector2 inputValue = AdjustFloatInput(newValue, minFloatField.value, minMaxSliderAttribute.Step,
                        userData.FreeMin, userData.FreeMax, minMaxSliderAttribute.FreeInput);
                    ApplyFloatValue(property,
                        inputValue, onValueChangedCallback, minMaxSliderAttribute, container, info, parent);
                    // if (minMaxSliderAttribute.FreeInput)
                    // {
                    //     userData.FreeMin = Mathf.Min(userData.FreeMin, inputValue.x);
                    //     userData.FreeMax = Mathf.Max(userData.FreeMax, inputValue.y);
                    // }
                });
            }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
            {
                return;
            }

            bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;
            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            AdjustFreeRangeHighAndLow(isInt ? property.vector2IntValue : property.vector2Value, property,
                minMaxSliderAttribute, container, info, parent);

            if (isInt)
            {
                Vector2Int value = property.vector2IntValue;
                IntegerField minIntField = container.Q<IntegerField>(NameMinInteger(property));
                IntegerField maxIntField = container.Q<IntegerField>(NameMaxInteger(property));
                if (minIntField.value != value.x)
                {
                    minIntField.SetValueWithoutNotify(value.x);
                }

                if (maxIntField.value != value.y)
                {
                    maxIntField.SetValueWithoutNotify(value.y);
                }
            }
            else
            {
                Vector2 value = property.vector2Value;
                FloatField minFloatField = container.Q<FloatField>(NameMinFloat(property));
                FloatField maxFloatField = container.Q<FloatField>(NameMaxFloat(property));
                if (!Mathf.Approximately(minFloatField.value, value.x))
                {
                    minFloatField.SetValueWithoutNotify(value.x);
                }

                if (!Mathf.Approximately(maxFloatField.value, value.y))
                {
                    maxFloatField.SetValueWithoutNotify(value.y);
                }
            }
        }

        private static void AdjustFreeRangeHighAndLow(Vector2 newValue, SerializedProperty property,
            MinMaxSliderAttribute minMaxSliderAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;
            MinMaxSlider minMaxSlider = container.Q<MinMaxSlider>(NameSlider(property));
            UserData userData = (UserData)minMaxSlider.userData;

            MetaInfo metaInfo = GetMetaInfo(property, minMaxSliderAttribute, info, parent);

            if (isInt)
            {
                Vector2Int value = property.vector2IntValue;
                Vector2Int sliderValue = value.y < value.x
                    ? new Vector2Int(value.x, value.x)
                    : value;

                // if the new value is out of range, let's expand the low/high limit. But we need to stick to the `step` for low if it's a external change
                // high is free, we don't care about it because the setting value process with handle it
                // this is not fun, at all
                float step = minMaxSliderAttribute.Step;
                if (minMaxSliderAttribute.FreeInput)
                {
                    int leftValue = Mathf.RoundToInt(sliderValue.x);
                    float originMin = userData.FreeMin;
                    // Debug.Log($"originMin={originMin}, metaInfo.MinValue={metaInfo.MinValue} leftValue={leftValue}");
                    bool leftValueOutOfRange = leftValue < originMin;
                    bool leftMetaAdjust = false;
                    if (metaInfo.Error == "")
                    {
                        int intMetaMinValue = Mathf.RoundToInt(metaInfo.MinValue);
                        if (intMetaMinValue > leftValue)
                        {
                            leftValue = intMetaMinValue;
                            leftMetaAdjust = true;
                        }
                        // else if(intMetaMinValue > originMin && intMetaMinValue < leftValue)
                        // {
                        //     leftMetaAdjust = true;
                        // }
                    }

                    if (leftValueOutOfRange || leftMetaAdjust)
                    {
                        if (leftValueOutOfRange && step > 0)
                        {
                            int newMin =
                                Mathf.RoundToInt(originMin + Mathf.FloorToInt((leftValue - originMin) / step) * step);
                            minMaxSlider.lowLimit = userData.FreeMin = newMin;
                        }
                        else
                        {
                            minMaxSlider.lowLimit = userData.FreeMin = Mathf.Min(leftValue, metaInfo.MinValue);
                        }
                    }

                    float rightValue = sliderValue.y;
                    float useMax = Mathf.Max(rightValue, userData.FreeMax, metaInfo.MaxValue);
                    if (!Mathf.Approximately(minMaxSlider.highLimit, useMax))
                    {
                        minMaxSlider.highLimit = userData.FreeMax = useMax;
                    }
                }
                else if (metaInfo.Error == "") // for non-free input, the limit is fixed
                {
                    if (!Mathf.Approximately(minMaxSlider.lowLimit, metaInfo.MinValue))
                    {
                        minMaxSlider.lowLimit = userData.FreeMin = metaInfo.MinValue;
                    }

                    if (!Mathf.Approximately(minMaxSlider.highLimit, metaInfo.MaxValue))
                    {
                        minMaxSlider.highLimit = userData.FreeMax = metaInfo.MaxValue;
                    }
                }
            }
            else
            {
                Vector2 value = property.vector2Value;

                Vector2 sliderValue = value.y < value.x
                    ? new Vector2(value.x, value.x)
                    : value;

                // if the new value is out of range, let's expand the low/high limit. But we need to stick to the `step` for low if it's a external change
                // high is free, we don't care about it because the setting value process with handle it
                // this is not fun, at all
                float step = minMaxSliderAttribute.Step;
                if (minMaxSliderAttribute.FreeInput)
                {
                    float leftValue = sliderValue.x;
                    float originMin = userData.FreeMin;
                    // Debug.Log($"originMin={originMin}, metaInfo.MinValue={metaInfo.MinValue} leftValue={leftValue}");
                    bool leftValueOutOfRange = leftValue < originMin;
                    bool leftMetaAdjust = false;
                    if (metaInfo.Error == "")
                    {
                        if (metaInfo.MinValue > leftValue)
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                            Debug.Log($"leftMetaAdjust as metaInfo.MinValue {metaInfo.MinValue} > leftValue {leftValue}");
#endif
                            leftValue = metaInfo.MinValue;
                            leftMetaAdjust = true;
                        }
//                         else if(metaInfo.MinValue > originMin && metaInfo.MinValue < leftValue)
//                         {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
//                             Debug.Log($"leftMetaAdjust as metaInfo.MinValue {metaInfo.MinValue} out of {leftValue} ~ {originMin}");
// #endif
//                             leftMetaAdjust = true;
//                         }
                    }

                    if (leftValueOutOfRange || leftMetaAdjust)
                    {
                        if (leftValueOutOfRange && step > 0)
                        {
                            float newMin = originMin + Mathf.FloorToInt((leftValue - originMin) / step) * step;
                            minMaxSlider.lowLimit = userData.FreeMin = newMin;
                        }
                        else
                        {
                            minMaxSlider.lowLimit = userData.FreeMin = Mathf.Min(leftValue, metaInfo.MinValue);
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                        Debug.Log($"lowLimit changed {minMaxSlider.lowLimit}: leftValueOutOfRange={leftValueOutOfRange}, leftMetaAdjust={leftMetaAdjust}");
#endif
                    }

                    float rightValue = sliderValue.y;
                    float useMax = Mathf.Max(rightValue, userData.FreeMax, metaInfo.MaxValue);
                    if (!Mathf.Approximately(minMaxSlider.highLimit, useMax))
                    {
                        minMaxSlider.highLimit = userData.FreeMax = useMax;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                        Debug.Log($"set highLimit to {useMax}, rightValue={rightValue}, userData.FreeMax={userData.FreeMax}, metaInfo.MaxValue={metaInfo.MaxValue}");
#endif
                    }
                }
                else if (metaInfo.Error == "") // for non-free input, the limit is fixed
                {
                    if (!Mathf.Approximately(minMaxSlider.lowLimit, metaInfo.MinValue))
                    {
                        minMaxSlider.lowLimit = userData.FreeMin = metaInfo.MinValue;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                        Debug.Log($"lowLimit changed {minMaxSlider.lowLimit}");
#endif
                    }

                    if (!Mathf.Approximately(minMaxSlider.highLimit, metaInfo.MaxValue))
                    {
                        minMaxSlider.highLimit = userData.FreeMax = metaInfo.MaxValue;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
                        Debug.Log($"highLimit changed {minMaxSlider.highLimit}");
#endif
                    }
                }
            }
        }

        private static void ApplyIntValue(SerializedProperty property, Vector2Int sliderValue,
            Action<object> onValueChangedCallback, MinMaxSliderAttribute sliderAttribute, VisualElement container,
            FieldInfo info, object parent)
        {
            // ReSharper disable once InlineTemporaryVariable
            Vector2Int vector2IntValue = sliderValue;

            AdjustFreeRangeHighAndLow(sliderValue, property, sliderAttribute, container, info, parent);
            property.vector2IntValue = vector2IntValue;
            property.serializedObject.ApplyModifiedProperties();
            onValueChangedCallback.Invoke(vector2IntValue);

            // if (freeInput)
            // {
            //     if(slider.lowLimit > vector2IntValue.x)
            //     {
            //         // Debug.Log($"free adjust low {slider.lowLimit} -> {vector2IntValue.x}");
            //         slider.lowLimit = vector2IntValue.x;
            //     }
            //     if(slider.highLimit < vector2IntValue.y)
            //     {
            //         // Debug.Log($"free adjust high {slider.highLimit} -> {vector2IntValue.y}");
            //         slider.highLimit = vector2IntValue.y;
            //     }
            // }

            // slider.SetValueWithoutNotify(vector2IntValue);
            // minField.SetValueWithoutNotify(vector2IntValue.x);
            // maxField.SetValueWithoutNotify(vector2IntValue.y);
        }

        private static void ApplyFloatValue(SerializedProperty property, Vector2 sliderValue,
            Action<object> onValueChangedCallback, MinMaxSliderAttribute sliderAttribute, VisualElement container,
            FieldInfo info, object parent)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_SLIDER
            Debug.Log($"apply  {sliderValue}");
#endif
            AdjustFreeRangeHighAndLow(sliderValue, property, sliderAttribute, container, info, parent);
            property.vector2Value = sliderValue;
            // Debug.Log($"apply float {vector2Value}");
            property.serializedObject.ApplyModifiedProperties();
            onValueChangedCallback.Invoke(sliderValue);

            // if (freeInput)
            // {
            //     if(slider.minValue > sliderValue.x)
            //     {
            //         slider.lowLimit = sliderValue.x;
            //     }
            //     if(slider.maxValue < sliderValue.y)
            //     {
            //         slider.highLimit = sliderValue.y;
            //     }
            // }

            // slider.SetValueWithoutNotify(sliderValue);
            // minField.SetValueWithoutNotify(sliderValue.x);
            // maxField.SetValueWithoutNotify(sliderValue.y);
        }
    }
}
#endif
