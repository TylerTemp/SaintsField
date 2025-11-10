#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
#if SAINTSFIELD_NEWTONSOFT_JSON
// using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    public partial class PropRangeAttributeDrawer
    {
        // public class PropRangeField : BaseField<float>
        // {
        //     public PropRangeField(string label, VisualElement visualInput) : base(label, visualInput)
        //     {
        //     }
        // }

        // private static string NamePropRange(SerializedProperty property) => $"{property.propertyPath}__PropRange";
        // private static string NameSlider(SerializedProperty property) => $"{property.propertyPath}__PropRange_Slider";
        //
        // private static string NameInteger(SerializedProperty property) =>
        //     $"{property.propertyPath}__PropRange_IntegerField";
        //
        // private static string NameFloat(SerializedProperty property) =>
        //     $"{property.propertyPath}__PropRange_FloatField";

        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__PropRange_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                // case SerializedPropertyType.Character:
                {
                    if (rawType == typeof(uint))
                    {
                        PropRangeElementUInt element = new PropRangeElementUInt(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
                        PropRangeUIntField field = new PropRangeUIntField(GetPreferredLabel(property), element);
                        field.AddToClassList(PropRangeUIntField.alignedFieldUssClassName);
                        return field;
                    }
                    if (rawType == typeof(long))
                    {
                        PropRangeElementLong element = new PropRangeElementLong(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
                        PropRangeLongField field = new PropRangeLongField(GetPreferredLabel(property), element);
                        field.AddToClassList(PropRangeLongField.alignedFieldUssClassName);
                        return field;
                    }
                    if (rawType == typeof(ulong))
                    {
                        PropRangeElementULong element = new PropRangeElementULong(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
                        PropRangeULongField field = new PropRangeULongField(GetPreferredLabel(property), element);
                        field.AddToClassList(PropRangeULongField.alignedFieldUssClassName);
                        return field;
                    }
                    else
                    {
                        PropRangeElementInt element = new PropRangeElementInt(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
                        PropRangeIntField field = new PropRangeIntField(GetPreferredLabel(property), element);
                        field.AddToClassList(PropRangeIntField.alignedFieldUssClassName);
                        return field;
                    }
                }
                case SerializedPropertyType.Float:
                {
                    PropRangeElementDouble element = new PropRangeElementDouble(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
                    PropRangeDoubleField field = new PropRangeDoubleField(GetPreferredLabel(property), element);
                    field.AddToClassList(PropRangeDoubleField.alignedFieldUssClassName);
                    return field;
                }
                default:
                    return PropertyFieldFallbackUIToolkit(property, GetPreferredLabel(property));
            }

            // VisualElement root = new VisualElement
            // {
            //     style =
            //     {
            //         flexDirection = FlexDirection.Row,
            //     },
            // };
            //
            // Slider slider = new Slider("", 0, 1, SliderDirection.Horizontal, 0.5f)
            // {
            //     name = NameSlider(property),
            //     style =
            //     {
            //         flexGrow = 1,
            //         flexShrink = 1,
            //     },
            // };
            // root.Add(slider);
            //
            // const int width = 50;
            //
            // if (property.propertyType == SerializedPropertyType.Integer)
            // {
            //     root.Add(new IntegerField
            //     {
            //         name = NameInteger(property),
            //         value = property.intValue,
            //         style =
            //         {
            //             // flexShrink = 0,
            //             flexGrow = 0,
            //             width = width,
            //         },
            //     });
            // }
            // else
            // {
            //     root.Add(new FloatField
            //     {
            //         name = NameFloat(property),
            //         value = property.floatValue,
            //         style =
            //         {
            //             // flexShrink = 0,
            //             flexGrow = 0,
            //             width = width,
            //         },
            //     });
            // }
            //
            // PropRangeField propRangeField = new PropRangeField(GetPreferredLabel(property), root)
            // {
            //     name = NamePropRange(property),
            // };
            // propRangeField.BindProperty(property);
            //
            // propRangeField.AddToClassList(ClassAllowDisable);
            // propRangeField.labelElement.style.overflow = Overflow.Hidden;
            // propRangeField.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);
            //
            // return propRangeField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
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
                    return helpBox;
                }
                default:
                    return new HelpBox($"{property.propertyType} is not supported", HelpBoxMessageType.Error)
                    {
                        style =
                        {
                            flexGrow = 1,
                        },
                    };
            }

        }

        private static (object minValue, string minError, object maxValue, string maxError) GetMinMax(SerializedProperty property, PropRangeAttribute propRangeAttribute,
            MemberInfo info, object parentTarget)
        {
            object minValue;
            if (propRangeAttribute.MinCallback == null)
            {
                minValue = propRangeAttribute.Min;
            }
            else
            {
                (string getError, object getValue) =
                    Util.GetOf<object>(propRangeAttribute.MinCallback, 0, property, info, parentTarget);
                if (getError != "")
                {
                    return (null, getError, null, "");
                }
                minValue = getValue;
            }

            object maxValue;
            if (propRangeAttribute.MaxCallback == null)
            {
                maxValue = propRangeAttribute.Max;
            }
            else
            {
                (string getError, object getValue) =
                    Util.GetOf<object>(propRangeAttribute.MaxCallback, 0f, property, info, parentTarget);
                if (getError != "")
                {
                    return (minValue, "", null, getError);
                }

                maxValue = getValue;
            }
            return (minValue, "", maxValue, "");
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            PropRangeAttribute propRangeAttribute = (PropRangeAttribute)saintsAttribute;

            double step = propRangeAttribute.Step;

            Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;

            FieldInfo maxValueProp = rawType.GetField("MaxValue", BindingFlags.Public | BindingFlags.Static);
            FieldInfo minValueProp = rawType.GetField("MinValue", BindingFlags.Public | BindingFlags.Static);

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                // case SerializedPropertyType.Character:
                {
                    if (rawType == typeof(uint))
                    {
                        PropRangeUIntField field = container.Q<PropRangeUIntField>();
                        UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                        field.PropRangeElementUInt.BindHelpBox(helpBox);
                        field.PropRangeElementUInt.bindingPath = property.propertyPath;

                        uint intStep = step > 0? (uint)step: 0u;

                        void UpdateMinMax()
                        {
                            (object minValue, string minError, object maxValue, string maxError) =
                                GetMinMax(property, propRangeAttribute, info, parent);
                            if (minError != "")
                            {
                                UIToolkitUtils.SetHelpBox(helpBox, minError);
                                return;
                            }

                            if (maxError != "")
                            {
                                UIToolkitUtils.SetHelpBox(helpBox, maxError);
                                return;
                            }

                            field.PropRangeElementUInt.SetConfig(minValue, maxValue, intStep);
                        }

                        UpdateMinMax();
                        SaintsEditorApplicationChanged.OnAnyEvent.AddListener(UpdateMinMax);
                        container.RegisterCallback<DetachFromPanelEvent>(_ =>
                            SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(UpdateMinMax));
                    }
                    else if (rawType == typeof(long))
                    {
                        PropRangeLongField field = container.Q<PropRangeLongField>();
                        UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                        field.PropRangeElementLong.BindHelpBox(helpBox);
                        field.PropRangeElementLong.bindingPath = property.propertyPath;

                        long intStep = (int)step;

                        void UpdateMinMax()
                        {
                            (object minValue, string minError, object maxValue, string maxError) =
                                GetMinMax(property, propRangeAttribute, info, parent);
                            if (minError != "")
                            {
                                UIToolkitUtils.SetHelpBox(helpBox, minError);
                                return;
                            }

                            if (maxError != "")
                            {
                                UIToolkitUtils.SetHelpBox(helpBox, maxError);
                                return;
                            }

                            field.PropRangeElementLong.SetConfig(minValue, maxValue, intStep);
                        }

                        UpdateMinMax();
                        SaintsEditorApplicationChanged.OnAnyEvent.AddListener(UpdateMinMax);
                        container.RegisterCallback<DetachFromPanelEvent>(_ =>
                            SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(UpdateMinMax));
                    }
                    else if (rawType == typeof(ulong))
                    {
                        PropRangeULongField field = container.Q<PropRangeULongField>();
                        UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                        field.PropRangeElementULong.BindHelpBox(helpBox);
                        field.PropRangeElementULong.bindingPath = property.propertyPath;

                        ulong intStep = (ulong)(step <= 1? 1: step);

                        void UpdateMinMax()
                        {
                            (object minValue, string minError, object maxValue, string maxError) =
                                GetMinMax(property, propRangeAttribute, info, parent);
                            if (minError != "")
                            {
                                UIToolkitUtils.SetHelpBox(helpBox, minError);
                                return;
                            }

                            if (maxError != "")
                            {
                                UIToolkitUtils.SetHelpBox(helpBox, maxError);
                                return;
                            }

                            field.PropRangeElementULong.SetConfig(minValue, maxValue, intStep);
                        }

                        UpdateMinMax();
                        SaintsEditorApplicationChanged.OnAnyEvent.AddListener(UpdateMinMax);
                        container.RegisterCallback<DetachFromPanelEvent>(_ =>
                            SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(UpdateMinMax));
                    }
                    else
                    {
                        PropRangeIntField field = container.Q<PropRangeIntField>();
                        UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                        field.PropRangeElementInt.BindHelpBox(helpBox);
                        field.PropRangeElementInt.bindingPath = property.propertyPath;

                        int intStep = (int)step;

                        // ReSharper disable once PossibleNullReferenceException
                        int maxCap = Convert.ToInt32(maxValueProp.GetValue(null));
                        // ReSharper disable once PossibleNullReferenceException
                        int minCap = Convert.ToInt32(minValueProp.GetValue(null));

                        void UpdateMinMax()
                        {
                            (object minValue, string minError, object maxValue, string maxError) =
                                GetMinMax(property, propRangeAttribute, info, parent);
                            if (minError != "")
                            {
                                UIToolkitUtils.SetHelpBox(helpBox, minError);
                                return;
                            }

                            if (maxError != "")
                            {
                                UIToolkitUtils.SetHelpBox(helpBox, maxError);
                                return;
                            }

                            field.PropRangeElementInt.SetConfig(minValue, minCap, maxValue, maxCap, intStep);
                        }

                        UpdateMinMax();
                        SaintsEditorApplicationChanged.OnAnyEvent.AddListener(UpdateMinMax);
                        container.RegisterCallback<DetachFromPanelEvent>(_ =>
                            SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(UpdateMinMax));
                    }
                }
                    break;
                case SerializedPropertyType.Float:
                {
                    PropRangeDoubleField field = container.Q<PropRangeDoubleField>();
                    UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                    field.PropRangeElementDouble.BindHelpBox(helpBox);
                    field.PropRangeElementDouble.bindingPath = property.propertyPath;

                    // int intStep = (int)step;

                    // ReSharper disable once PossibleNullReferenceException
                    double maxCap = Convert.ToDouble(maxValueProp.GetValue(null));
                    // ReSharper disable once PossibleNullReferenceException
                    double minCap = Convert.ToDouble(minValueProp.GetValue(null));

                    void UpdateMinMax()
                    {
                        (object minValue, string minError, object maxValue, string maxError) =
                            GetMinMax(property, propRangeAttribute, info, parent);
                        if (minError != "")
                        {
                            UIToolkitUtils.SetHelpBox(helpBox, minError);
                            return;
                        }

                        if (maxError != "")
                        {
                            UIToolkitUtils.SetHelpBox(helpBox, maxError);
                            return;
                        }

                        // Debug.Log($"update min max {minValue}-{maxValue}");
                        field.PropRangeElementDouble.SetConfig(minValue, minCap, maxValue, maxCap, step);
                    }

                    UpdateMinMax();
                    SaintsEditorApplicationChanged.OnAnyEvent.AddListener(UpdateMinMax);
                    container.RegisterCallback<DetachFromPanelEvent>(_ =>
                        SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(UpdateMinMax));
                }
                    break;
            }


            // AdaptAttribute adaptAttribute = allAttributes.OfType<AdaptAttribute>().FirstOrDefault();

            // PropRangeField propRangeField = container.Q<PropRangeField>(name: NamePropRange(property));


            // Slider slider = propRangeField.Q<Slider>(NameSlider(property));

            // PropRangeAttribute propRangeAttribute = (PropRangeAttribute)saintsAttribute;

            // MetaInfo metaInfo = GetMetaInfo(property, propRangeAttribute, info, parent);

            // bool isFloat = metaInfo.IsFloat;
            // (string error, double value) curValueInfo = GetPreValue(isFloat ? property.floatValue : property.intValue, adaptAttribute);
            // if (curValueInfo.error != "")
            // {
            //     return;
            // }
            // float curValue = (float) curValueInfo.value;
            //
            // (string error, double value) minValueInfo = GetPreValue(metaInfo.MinValue, adaptAttribute);
            // if (minValueInfo.error != "")
            // {
            //     return;
            // }
            // float minValue = (float)minValueInfo.value;
            //
            // (string error, double value) maxValueInfo = GetPreValue(metaInfo.MaxValue, adaptAttribute);
            // if (maxValueInfo.error != "")
            // {
            //     return;
            // }
            // float maxValue = (float)maxValueInfo.value;
            //
            // // Debug.Log($"{minValue}/{maxValue}");
            //
            // slider.lowValue = minValue;
            // slider.highValue = maxValue;
            // slider.value = curValue;
            // slider.userData = metaInfo;
            //
            // IntegerField integerField = container.Q<IntegerField>(NameInteger(property));
            // FloatField floatField = container.Q<FloatField>(NameFloat(property));
            //
            // if (isFloat)
            // {
            //     floatField.value = curValue;
            //     floatField.RegisterValueChangedCallback(changed =>
            //     {
            //         float adaptedValue = changed.newValue;
            //         (string error, double value) postValueInfo = GetPostValue(changed.newValue, adaptAttribute);
            //         if (postValueInfo.error != "")
            //         {
            //             return;
            //         }
            //         float parsedValue = GetValue(GetMetaInfo(property, saintsAttribute, info, parent),
            //             (float)postValueInfo.value);
            //         property.doubleValue = _cachedChangeValue = parsedValue;
            //         property.serializedObject.ApplyModifiedProperties();
            //
            //         floatField.SetValueWithoutNotify(adaptedValue);
            //         slider.SetValueWithoutNotify(adaptedValue);
            //         info.SetValue(parent, parsedValue);
            //         onValueChangedCallback.Invoke(parsedValue);
            //     });
            // }
            // else
            // {
            //     integerField.value = (int)curValue;
            //     integerField.RegisterValueChangedCallback(changed =>
            //     {
            //         float adaptedValue = changed.newValue;
            //         (string error, double value) postValueInfo = GetPostValue(changed.newValue, adaptAttribute);
            //         if (postValueInfo.error != "")
            //         {
            //             return;
            //         }
            //         int parsedValue = (int)GetValue(GetMetaInfo(property, saintsAttribute, info, parent),
            //             (float)postValueInfo.value);
            //         property.intValue = parsedValue;
            //         _cachedChangeValue = property.intValue;
            //         property.serializedObject.ApplyModifiedProperties();
            //
            //         floatField.SetValueWithoutNotify(adaptedValue);
            //         slider.SetValueWithoutNotify(adaptedValue);
            //         info.SetValue(parent, parsedValue);
            //         onValueChangedCallback.Invoke(parsedValue);
            //     });
            // }
            //
            // slider.RegisterValueChangedCallback(changed =>
            // {
            //     float adaptedValue = changed.newValue;
            //     (string error, double value) postValueInfo = GetPostValue(adaptedValue, adaptAttribute);
            //     if (postValueInfo.error != "")
            //     {
            //         return;
            //     }
            //
            //     float parsedValue = GetValue(GetMetaInfo(property, saintsAttribute, info, parent), (float)postValueInfo.value);
            //
            //     (string error, double value) preValueInfo = GetPreValue(parsedValue, adaptAttribute);
            //     if (preValueInfo.error != "")
            //     {
            //         return;
            //     }
            //
            //     if (property.propertyType == SerializedPropertyType.Float)
            //     {
            //         property.doubleValue = parsedValue;
            //         _cachedChangeValue = parsedValue;
            //         property.serializedObject.ApplyModifiedProperties();
            //
            //         floatField.SetValueWithoutNotify((float)preValueInfo.value);
            //         slider.SetValueWithoutNotify((float)preValueInfo.value);
            //         ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, parsedValue);
            //         onValueChangedCallback.Invoke(parsedValue);
            //     }
            //     else
            //     {
            //         int intValue = (int)parsedValue;
            //         property.intValue = intValue;
            //         _cachedChangeValue = intValue;
            //         property.serializedObject.ApplyModifiedProperties();
            //
            //         integerField.SetValueWithoutNotify((int) preValueInfo.value);
            //         slider.SetValueWithoutNotify((int) preValueInfo.value);
            //         // info.SetValue(parent, intValue);
            //         ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, intValue);
            //         onValueChangedCallback.Invoke(intValue);
            //     }
            //
            //     property.serializedObject.ApplyModifiedProperties();
            // });
            //
            // HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            // if (metaInfo.Error != "")
            // {
            //     helpBox.text = metaInfo.Error;
            //     helpBox.style.display = DisplayStyle.Flex;
            // }
            //
            // helpBox.TrackPropertyValue(property, _ => UpdateExternal());
            // helpBox.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(UpdateExternal));
            // SaintsEditorApplicationChanged.OnAnyEvent.AddListener(UpdateExternal);
            //
            // return;
            //
            // void UpdateExternal()
            // {
            //     if (metaInfo.IsFloat)
            //     {
            //         if (Mathf.Approximately(property.floatValue, _cachedChangeValue))
            //         {
            //             return;
            //         }
            //     }
            //     else
            //     {
            //         if (Mathf.Approximately(property.intValue, _cachedChangeValue))
            //         {
            //             return;
            //         }
            //     }
            //     UpdateDisplay(property, propRangeAttribute, adaptAttribute, container, info);
            // }
        }
        //
        // private static void UpdateDisplay(SerializedProperty property, PropRangeAttribute propRangeAttribute,
        //     AdaptAttribute adaptAttribute,
        //     VisualElement container, FieldInfo info)
        // {
        //     object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
        //
        //     MetaInfo metaInfo = GetMetaInfo(property, propRangeAttribute, info, parent);
        //
        //     Slider slider = container.Q<Slider>(NameSlider(property));
        //     // MetaInfo curMetaInfo = (MetaInfo)slider.userData;
        //
        //     HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
        //
        //     if (metaInfo.Error != helpBox.text)
        //     {
        //         helpBox.text = metaInfo.Error;
        //         helpBox.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
        //         if (metaInfo.Error != "")
        //         {
        //             return;
        //         }
        //     }
        //
        //     if (!string.IsNullOrEmpty(propRangeAttribute.MinCallback))
        //     {
        //         slider.lowValue = metaInfo.MinValue;
        //     }
        //
        //     if (!string.IsNullOrEmpty(propRangeAttribute.MaxCallback))
        //     {
        //         slider.highValue = metaInfo.MaxValue;
        //     }
        //
        //     (string error, double value) curValueInfo = GetPreValue(metaInfo.IsFloat ? property.floatValue : property.intValue, adaptAttribute);
        //     if (curValueInfo.error != "")
        //     {
        //         return;
        //     }
        //     float curValue = (float) curValueInfo.value;
        //
        //     IntegerField integerField = container.Q<IntegerField>(NameInteger(property));
        //     FloatField floatField = container.Q<FloatField>(NameFloat(property));
        //
        //     if (metaInfo.IsFloat)
        //     {
        //         floatField.SetValueWithoutNotify(curValue);
        //     }
        //     else
        //     {
        //         integerField.SetValueWithoutNotify((int)curValue);
        //     }
        //
        //     // let it trigger the change
        //     slider.value = curValue;
        // }
    }
}
#endif
