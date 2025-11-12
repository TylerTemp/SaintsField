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
                        PropRangeElementUInt element =
                            new PropRangeElementUInt(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
                        PropRangeUIntField field = new PropRangeUIntField(GetPreferredLabel(property), element);
                        field.AddToClassList(PropRangeUIntField.alignedFieldUssClassName);
                        field.AddToClassList(ClassAllowDisable);
                        return field;
                    }
                    if (rawType == typeof(long))
                    {
                        PropRangeElementLong element = new PropRangeElementLong(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
                        PropRangeLongField field = new PropRangeLongField(GetPreferredLabel(property), element);
                        field.AddToClassList(PropRangeLongField.alignedFieldUssClassName);
                        field.AddToClassList(ClassAllowDisable);
                        return field;
                    }
                    if (rawType == typeof(ulong))
                    {
                        PropRangeElementULong element = new PropRangeElementULong(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
                        PropRangeULongField field = new PropRangeULongField(GetPreferredLabel(property), element);
                        field.AddToClassList(PropRangeULongField.alignedFieldUssClassName);
                        field.AddToClassList(ClassAllowDisable);
                        return field;
                    }
                    else
                    {
                        PropRangeElementInt element = new PropRangeElementInt(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
                        PropRangeIntField field = new PropRangeIntField(GetPreferredLabel(property), element);
                        field.AddToClassList(PropRangeIntField.alignedFieldUssClassName);
                        field.AddToClassList(ClassAllowDisable);
                        return field;
                    }
                }
                case SerializedPropertyType.Float:
                {
                    PropRangeElementDouble element = new PropRangeElementDouble(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
                    PropRangeDoubleField field = new PropRangeDoubleField(GetPreferredLabel(property), element);
                    field.AddToClassList(PropRangeDoubleField.alignedFieldUssClassName);
                    field.AddToClassList(ClassAllowDisable);
                    return field;
                }
                default:
                    return PropertyFieldFallbackUIToolkit(property, GetPreferredLabel(property));
            }
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

        private static (object minValue, string minError, object maxValue, string maxError) GetMinMaxForShowInInspector(
            PropRangeAttribute propRangeAttribute, object curValue, object target)
        {
            object minValue;
            if (propRangeAttribute.MinCallback == null)
            {
                minValue = propRangeAttribute.Min;
            }
            else
            {
                (object getValue, string getError) = GetCallbackForShowInInspector(propRangeAttribute.MinCallback, curValue, target);
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
                (object getValue, string getError) = GetCallbackForShowInInspector(propRangeAttribute.MaxCallback, curValue, target);
                if (getError != "")
                {
                    return (null, getError, null, "");
                }

                maxValue = getValue;
            }
            return (minValue, "", maxValue, "");
        }

        private static (object getValue, string getError) GetCallbackForShowInInspector(string callback, object curValue, object target)
        {
            foreach (Type type in ReflectUtils.GetSelfAndBaseTypesFromInstance(target))
            {
                (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) = ReflectUtils.GetProp(type, callback);

                switch (getPropType)
                {
                    case ReflectUtils.GetPropType.NotFound:
                        continue;

                    case ReflectUtils.GetPropType.Property:
                    {
                        object genResult = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);
                        if(genResult != null)
                        {
                            return (genResult, "");
                        }
                    }
                        break;
                    case ReflectUtils.GetPropType.Field:
                    {
                        FieldInfo fInfo = (FieldInfo)fieldOrMethodInfo;
                        object genResult = fInfo.GetValue(target);
                        if(genResult != null)
                        {
                            return (genResult, "");
                        }
                        // Debug.Log($"{fInfo}/{fInfo.Name}, target={target} genResult={genResult}");
                    }
                        break;
                    case ReflectUtils.GetPropType.Method:
                    {
                        MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;

                        object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), new[]
                        {
                            curValue,
                        });


                        object genResult;
                        try
                        {
                            genResult = methodInfo.Invoke(target, passParams);
                        }
                        catch (TargetInvocationException e)
                        {
                            return (e.InnerException?.Message ?? e.Message, null);
                        }
                        catch (Exception e)
                        {
                            return (e.Message, null);
                        }

                        if (genResult != null)
                        {
                            return (genResult, "");
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
                }
            }

            return ($"Target `{callback}` not found", null);
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
                        field.TrackSerializedObjectValue(property.serializedObject, _ => UpdateMinMax());
                        field.PropRangeElementUInt.BindProperty(property);
                    }
                    else if (rawType == typeof(long))
                    {
                        PropRangeLongField field = container.Q<PropRangeLongField>();
                        UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                        field.PropRangeElementLong.BindHelpBox(helpBox);

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
                        field.TrackSerializedObjectValue(property.serializedObject, _ => UpdateMinMax());
                        field.PropRangeElementLong.BindProperty(property);
                    }
                    else if (rawType == typeof(ulong))
                    {
                        PropRangeULongField field = container.Q<PropRangeULongField>();
                        UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                        field.PropRangeElementULong.BindHelpBox(helpBox);

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
                        field.TrackSerializedObjectValue(property.serializedObject, _ => UpdateMinMax());
                        field.PropRangeElementULong.BindProperty(property);
                    }
                    else
                    {
                        PropRangeIntField field = container.Q<PropRangeIntField>();
                        UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                        field.PropRangeElementInt.BindHelpBox(helpBox);

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
                        field.TrackSerializedObjectValue(property.serializedObject, _ => UpdateMinMax());
                        field.PropRangeElementInt.BindProperty(property);
                    }
                }
                    break;
                case SerializedPropertyType.Float:
                {
                    PropRangeDoubleField field = container.Q<PropRangeDoubleField>();
                    UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                    field.PropRangeElementDouble.BindHelpBox(helpBox);

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
                    field.TrackSerializedObjectValue(property.serializedObject, _ => UpdateMinMax());
                    field.PropRangeElementDouble.BindProperty(property);
                }
                    break;
            }
        }

        public static VisualElement UIToolkitValueEditSByte(VisualElement oldElement, PropRangeAttribute propRangeAttribute, string label, sbyte value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const sbyte min = sbyte.MinValue;
            const sbyte max = sbyte.MaxValue;
            (object minValue, string minError, object maxValue, string maxError) =
                GetMinMaxForShowInInspector(propRangeAttribute, value, targets[0]);

            if (oldElement is PropRangeIntField oldF)
            {
                if (minError == "" && maxError == "")
                {
                    oldF.PropRangeElementInt.SetConfig(minValue, min, maxValue, max, (int)propRangeAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            PropRangeElementInt element = new PropRangeElementInt(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
            PropRangeIntField field =
                new PropRangeIntField(label, element);
            if (minError == "" && maxError == "")
            {
                field.PropRangeElementInt.SetConfig(minValue, min, maxValue, max, (int)propRangeAttribute.Step);
            }

            field.value = value;

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    (bool ok, int result) = element.GetNumber(evt.newValue);
                    if (!ok)
                    {
                        return;
                    }

                    beforeSet?.Invoke(value);
                    setterOrNull((sbyte)result);
                });
            }
            return field;
        }

        public static VisualElement UIToolkitValueEditByte(VisualElement oldElement, PropRangeAttribute propRangeAttribute, string label, byte value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const byte min = byte.MinValue;
            const byte max = byte.MaxValue;
            (object minValue, string minError, object maxValue, string maxError) =
                GetMinMaxForShowInInspector(propRangeAttribute, value, targets[0]);

            if (oldElement is PropRangeIntField oldF)
            {
                if (minError == "" && maxError == "")
                {
                    oldF.PropRangeElementInt.SetConfig(minValue, min, maxValue, max, (int)propRangeAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            PropRangeElementInt element = new PropRangeElementInt(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
            PropRangeIntField field =
                new PropRangeIntField(label, element);
            if (minError == "" && maxError == "")
            {
                field.PropRangeElementInt.SetConfig(minValue, min, maxValue, max, (int)propRangeAttribute.Step);
            }

            field.value = value;

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    (bool ok, int result) = element.GetNumber(evt.newValue);
                    if (!ok)
                    {
                        return;
                    }

                    beforeSet?.Invoke(value);
                    setterOrNull((byte)result);
                });
            }
            return field;
        }

        public static VisualElement UIToolkitValueEditShort(VisualElement oldElement, PropRangeAttribute propRangeAttribute, string label, short value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const short min = short.MinValue;
            const short max = short.MaxValue;
            (object minValue, string minError, object maxValue, string maxError) =
                GetMinMaxForShowInInspector(propRangeAttribute, value, targets[0]);

            if (oldElement is PropRangeIntField oldF)
            {
                if (minError == "" && maxError == "")
                {
                    oldF.PropRangeElementInt.SetConfig(minValue, min, maxValue, max, (int)propRangeAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            PropRangeElementInt element = new PropRangeElementInt(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
            PropRangeIntField field =
                new PropRangeIntField(label, element);
            if (minError == "" && maxError == "")
            {
                field.PropRangeElementInt.SetConfig(minValue, min, maxValue, max, (int)propRangeAttribute.Step);
            }

            field.value = value;

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    (bool ok, int result) = element.GetNumber(evt.newValue);
                    if (!ok)
                    {
                        return;
                    }

                    beforeSet?.Invoke(value);
                    setterOrNull((short)result);
                });
            }
            return field;
        }

        public static VisualElement UIToolkitValueEditUShort(VisualElement oldElement, PropRangeAttribute propRangeAttribute, string label, ushort value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const ushort min = ushort.MinValue;
            const ushort max = ushort.MaxValue;
            (object minValue, string minError, object maxValue, string maxError) =
                GetMinMaxForShowInInspector(propRangeAttribute, value, targets[0]);

            if (oldElement is PropRangeIntField oldF)
            {
                if (minError == "" && maxError == "")
                {
                    oldF.PropRangeElementInt.SetConfig(minValue, min, maxValue, max, (int)propRangeAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            PropRangeElementInt element = new PropRangeElementInt(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
            PropRangeIntField field =
                new PropRangeIntField(label, element);
            if (minError == "" && maxError == "")
            {
                field.PropRangeElementInt.SetConfig(minValue, min, maxValue, max, (int)propRangeAttribute.Step);
            }

            field.value = value;

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    (bool ok, int result) = element.GetNumber(evt.newValue);
                    if (!ok)
                    {
                        return;
                    }

                    beforeSet?.Invoke(value);
                    setterOrNull((ushort)result);
                });
            }
            return field;
        }

        public static VisualElement UIToolkitValueEditInt(VisualElement oldElement, PropRangeAttribute propRangeAttribute, string label, int value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const int min = int.MinValue;
            const int max = int.MaxValue;
            (object minValue, string minError, object maxValue, string maxError) =
                GetMinMaxForShowInInspector(propRangeAttribute, value, targets[0]);

            if (oldElement is PropRangeIntField oldF)
            {
                if (minError == "" && maxError == "")
                {
                    oldF.PropRangeElementInt.SetConfig(minValue, min, maxValue, max, (int)propRangeAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            PropRangeElementInt element = new PropRangeElementInt(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
            PropRangeIntField field =
                new PropRangeIntField(label, element);
            if (minError == "" && maxError == "")
            {
                field.PropRangeElementInt.SetConfig(minValue, min, maxValue, max, (int)propRangeAttribute.Step);
            }

            field.value = value;

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }
            return field;
        }

        public static VisualElement UIToolkitValueEditUInt(VisualElement oldElement, PropRangeAttribute propRangeAttribute, string label, uint value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            (object minValue, string minError, object maxValue, string maxError) =
                GetMinMaxForShowInInspector(propRangeAttribute, value, targets[0]);
            uint useStep = (propRangeAttribute.Step < 1 ? 0 : (uint)propRangeAttribute.Step);

            if (oldElement is PropRangeUIntField oldF)
            {
                if (minError == "" && maxError == "")
                {
                    oldF.PropRangeElementUInt.SetConfig(minValue, maxValue, useStep);
                }
                // Debug.Log($"update {value}");

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            PropRangeElementUInt element = new PropRangeElementUInt(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
            PropRangeUIntField field =
                new PropRangeUIntField(label, element);
            if (minError == "" && maxError == "")
            {
                field.PropRangeElementUInt.SetConfig(minValue, maxValue, useStep);
            }

            // Debug.Log($"init {value}");
            field.value = value;

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }
            return field;
        }

        public static VisualElement UIToolkitValueEditLong(VisualElement oldElement, PropRangeAttribute propRangeAttribute, string label, long value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            (object minValue, string minError, object maxValue, string maxError) =
                GetMinMaxForShowInInspector(propRangeAttribute, value, targets[0]);
            long useStep = (long)propRangeAttribute.Step;

            if (oldElement is PropRangeLongField oldF)
            {
                if (minError == "" && maxError == "")
                {
                    oldF.PropRangeElementLong.SetConfig(minValue, maxValue, useStep);
                }
                // Debug.Log($"update {value}");

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            PropRangeElementLong element = new PropRangeElementLong(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
            PropRangeLongField field =
                new PropRangeLongField(label, element);
            if (minError == "" && maxError == "")
            {
                field.PropRangeElementLong.SetConfig(minValue, maxValue, useStep);
            }

            // Debug.Log($"init {value}");
            field.value = value;

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }
            return field;
        }

        public static VisualElement UIToolkitValueEditULong(VisualElement oldElement, PropRangeAttribute propRangeAttribute, string label, ulong value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            (object minValue, string minError, object maxValue, string maxError) =
                GetMinMaxForShowInInspector(propRangeAttribute, value, targets[0]);
            ulong useStep = (propRangeAttribute.Step < 1 ? 0 : (ulong)propRangeAttribute.Step);

            if (oldElement is PropRangeULongField oldF)
            {
                if (minError == "" && maxError == "")
                {
                    oldF.PropRangeElementULong.SetConfig(minValue, maxValue, useStep);
                }
                // Debug.Log($"update {value}");

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            PropRangeElementULong element = new PropRangeElementULong(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
            PropRangeULongField field =
                new PropRangeULongField(label, element);
            if (minError == "" && maxError == "")
            {
                field.PropRangeElementULong.SetConfig(minValue, maxValue, useStep);
            }

            // Debug.Log($"init {value}");
            field.value = value;

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }
            return field;
        }

        public static VisualElement UIToolkitValueEditFloat(VisualElement oldElement, PropRangeAttribute propRangeAttribute, string label, float value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const float min = float.MinValue;
            const float max = float.MaxValue;
            (object minValue, string minError, object maxValue, string maxError) =
                GetMinMaxForShowInInspector(propRangeAttribute, value, targets[0]);

            if (oldElement is PropRangeDoubleField oldF)
            {
                if (minError == "" && maxError == "")
                {
                    oldF.PropRangeElementDouble.SetConfig(minValue, min, maxValue, max, propRangeAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            PropRangeElementDouble element = new PropRangeElementDouble(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
            PropRangeDoubleField field =
                new PropRangeDoubleField(label, element);
            if (minError == "" && maxError == "")
            {
                field.PropRangeElementDouble.SetConfig(minValue, min, maxValue, max, (int)propRangeAttribute.Step);
            }

            field.value = value;

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull((float)evt.newValue);
                });
            }
            return field;
        }

        public static VisualElement UIToolkitValueEditDouble(VisualElement oldElement, PropRangeAttribute propRangeAttribute, string label, double value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const double min = double.MinValue;
            const double max = double.MaxValue;
            (object minValue, string minError, object maxValue, string maxError) =
                GetMinMaxForShowInInspector(propRangeAttribute, value, targets[0]);

            if (oldElement is PropRangeDoubleField oldF)
            {
                if (minError == "" && maxError == "")
                {
                    oldF.PropRangeElementDouble.SetConfig(minValue, min, maxValue, max, propRangeAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            PropRangeElementDouble element = new PropRangeElementDouble(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
            PropRangeDoubleField field =
                new PropRangeDoubleField(label, element);
            if (minError == "" && maxError == "")
            {
                field.PropRangeElementDouble.SetConfig(minValue, min, maxValue, max, (int)propRangeAttribute.Step);
            }

            field.value = value;

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }
            return field;
        }
    }
}
#endif
