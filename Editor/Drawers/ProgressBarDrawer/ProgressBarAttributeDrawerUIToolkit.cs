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

namespace SaintsField.Editor.Drawers.ProgressBarDrawer
{
    public partial class ProgressBarAttributeDrawer
    {
        // private static string NameProgressBarField(SerializedProperty property) =>
        //     $"{property.propertyPath}__ProgressBarField";
        // private static string NameProgressBar(SerializedProperty property) => $"{property.propertyPath}__ProgressBar";

        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__ProgressBar_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0
            //     ? ReflectUtils.GetElementType(info.FieldType)
            //     : info.FieldType;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                // case SerializedPropertyType.Character:
                {
                    ProgressBarElementInt element = new ProgressBarElementInt();
                    ProgressBarFieldInt field = new ProgressBarFieldInt(GetPreferredLabel(property), element);
                    field.AddToClassList(ProgressBarFieldInt.alignedFieldUssClassName);
                    field.AddToClassList(ClassAllowDisable);
                    return field;
                }
                case SerializedPropertyType.Float:
                {
                    ProgressBarElementDouble element = new ProgressBarElementDouble();
                    ProgressBarFieldDouble field = new ProgressBarFieldDouble(GetPreferredLabel(property), element);
                    field.AddToClassList(ProgressBarFieldDouble.alignedFieldUssClassName);
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
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        public struct CallbackInfo
        {
            public readonly string Error;

            public readonly object MinValue;
            public readonly object MaxValue;
            public readonly Color Color;
            public readonly Color BackgroundColor;

            public CallbackInfo(string error)
            {
                Error = error;
                MinValue = null;
                MaxValue = null;
                Color = default;
                BackgroundColor = default;
            }

            public CallbackInfo(object minValue, object maxValue, Color color, Color backgroundColor)
            {
                Error = "";
                MinValue = minValue;
                MaxValue = maxValue;
                Color = color;
                BackgroundColor = backgroundColor;
            }
        }
        
        private static CallbackInfo GetMinMax(SerializedProperty property, ProgressBarAttribute progressBarAttribute,
            MemberInfo info, object parent)
        {
            object minValue;
            if (progressBarAttribute.MinCallback == null)
            {
                minValue = progressBarAttribute.Min;
            }
            else
            {
                (string getError, object getValue) =
                    Util.GetOf<object>(progressBarAttribute.MinCallback, 0, property, info, parent);
                if (getError != "")
                {
                    return new CallbackInfo(
                        getError
                    );
                }
                minValue = getValue;
            }

            object maxValue;
            if (progressBarAttribute.MaxCallback == null)
            {
                maxValue = progressBarAttribute.Max;
            }
            else
            {
                (string getError, object getValue) =
                    Util.GetOf<object>(progressBarAttribute.MaxCallback, 0f, property, info, parent);
                if (getError != "")
                {
                    return new CallbackInfo(getError);
                }

                maxValue = getValue;
            }

            Color color = progressBarAttribute.Color.GetColor();
            // Debug.Log($"progressBarAttribute.Color={progressBarAttribute.Color}/{color}");

            if(progressBarAttribute.ColorCallback != null)
            {
                (string error, Color value) =
                    GetCallbackColor(progressBarAttribute.ColorCallback, color, property, info, parent);

                if (error != "")
                {
                    return new CallbackInfo(error);
                }

                color = value;
            }

            Color backgroundColor = progressBarAttribute.BackgroundColor.GetColor();
            // ReSharper disable once InvertIf
            if(progressBarAttribute.BackgroundColorCallback != null)
            {
                (string error, Color value) = GetCallbackColor(progressBarAttribute.BackgroundColorCallback, backgroundColor, property, info, parent);
                if (error != "")
                {
                    return new CallbackInfo(error);
                }
                backgroundColor = value;
            }
            return new CallbackInfo(minValue, maxValue, color, backgroundColor);
        }

        private static CallbackInfo GetMinMaxShowInInspector(
            ProgressBarAttribute progressBarAttribute, object curValue, object target)
        {
            object minValue;
            if (progressBarAttribute.MinCallback == null)
            {
                minValue = progressBarAttribute.Min;
            }
            else
            {
                (object getValue, string getError) = GetCallbackForShowInInspector(progressBarAttribute.MinCallback, curValue, target);
                if (getError != "")
                {
                    return new CallbackInfo(
                        getError
                    );
                }
                minValue = getValue;
            }

            object maxValue;
            if (progressBarAttribute.MaxCallback == null)
            {
                maxValue = progressBarAttribute.Max;
            }
            else
            {
                (object getValue, string getError) =
                    GetCallbackForShowInInspector(progressBarAttribute.MaxCallback, curValue, target);
                if (getError != "")
                {
                    return new CallbackInfo(getError);
                }

                maxValue = getValue;
            }

            Color color = progressBarAttribute.Color.GetColor();
            // Debug.Log($"progressBarAttribute.Color={progressBarAttribute.Color}/{color}");

            if(progressBarAttribute.ColorCallback != null)
            {
                (object value, string error) =
                    GetCallbackForShowInInspector(progressBarAttribute.ColorCallback, color, target);

                if (error != "")
                {
                    return new CallbackInfo(error);
                }

                if (value is Color c)
                {
                    color = c;
                }
                else if (value is EColor ec)
                {
                    color = ec.GetColor();
                }
                else
                {
                    return new CallbackInfo($"{value} is not color");
                }
            }

            Color backgroundColor = progressBarAttribute.BackgroundColor.GetColor();
            // ReSharper disable once InvertIf
            if(progressBarAttribute.BackgroundColorCallback != null)
            {
                (object value, string error) =
                    GetCallbackForShowInInspector(progressBarAttribute.BackgroundColorCallback, color, target);
                if (error != "")
                {
                    return new CallbackInfo(error);
                }
                if (value is Color c)
                {
                    backgroundColor = c;
                }
                else if (value is EColor ec)
                {
                    backgroundColor = ec.GetColor();
                }
                else
                {
                    return new CallbackInfo($"{value} is not color");
                }
            }
            return new CallbackInfo(minValue, maxValue, color, backgroundColor);
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

            ProgressBarAttribute progressBarAttribute = (ProgressBarAttribute)saintsAttribute;

            float step = progressBarAttribute.Step;

            Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) >= 0
                ? ReflectUtils.GetElementType(info.FieldType)
                : info.FieldType;

            FieldInfo maxValueProp = rawType.GetField("MaxValue", BindingFlags.Public | BindingFlags.Static);
            FieldInfo minValueProp = rawType.GetField("MinValue", BindingFlags.Public | BindingFlags.Static);

            Func<float, float, float, string> titleCallback = null;
            if (!string.IsNullOrEmpty(progressBarAttribute.TitleCallback))
            {
                const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                              BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (Type type in ReflectUtils.GetSelfAndBaseTypesFromInstance(parent))
                {
                    MethodInfo methodInfo = type.GetMethod(progressBarAttribute.TitleCallback, bindAttr);
                    if (methodInfo == null)
                    {
                        continue;
                    }

                    if (!typeof(string).IsAssignableFrom(methodInfo.ReturnType))
                    {
                        continue;
                    }

                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    if (methodParams.Length != 4)
                    {
                        continue;
                    }

                    if (!typeof(float).IsAssignableFrom(methodParams[0].ParameterType))
                    {
                        continue;
                    }
                    if (!typeof(float).IsAssignableFrom(methodParams[1].ParameterType))
                    {
                        continue;
                    }
                    if (!typeof(float).IsAssignableFrom(methodParams[2].ParameterType))
                    {
                        continue;
                    }
                    if (!typeof(string).IsAssignableFrom(methodParams[3].ParameterType))
                    {
                        continue;
                    }

                    titleCallback = (curValue, minValue, maxValue) => (string)methodInfo.Invoke(parent,
                        new object[] { curValue, minValue, maxValue, property.displayName });
                }
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    ProgressBarFieldInt field = container.Q<ProgressBarFieldInt>();
                    UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                    field.ProgressBarElementInt.BindHelpBox(helpBox);
                    if (titleCallback != null)
                    {
                        field.ProgressBarElementInt.BindGetTitleCallback(titleCallback);
                    }

                    int intStep = (int)step;

                    // ReSharper disable once PossibleNullReferenceException
                    int maxCap = Convert.ToInt32(maxValueProp.GetValue(null));
                    // ReSharper disable once PossibleNullReferenceException
                    int minCap = Convert.ToInt32(minValueProp.GetValue(null));

                    void UpdateMinMax()
                    {
                        CallbackInfo callback =
                            GetMinMax(property, progressBarAttribute, info, parent);
                        if (callback.Error != "")
                        {
                            UIToolkitUtils.SetHelpBox(helpBox, callback.Error);
                            return;
                        }

                        field.ProgressBarElementInt.SetConfig(callback, minCap, maxCap, intStep);
                    }

                    UpdateMinMax();
                    SaintsEditorApplicationChanged.OnAnyEvent.AddListener(UpdateMinMax);
                    container.RegisterCallback<DetachFromPanelEvent>(_ =>
                        SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(UpdateMinMax));

                    field.ProgressBarElementInt.BindProperty(property);
                    field.TrackSerializedObjectValue(property.serializedObject, _ => UpdateMinMax());
                }
                    break;
                case SerializedPropertyType.Float:
                {
                    ProgressBarFieldDouble field = container.Q<ProgressBarFieldDouble>();
                    UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                    field.ProgressBarElementDouble.BindHelpBox(helpBox);
                    if (titleCallback != null)
                    {
                        field.ProgressBarElementDouble.BindGetTitleCallback(titleCallback);
                    }

                    // ReSharper disable once PossibleNullReferenceException
                    double maxCap = Convert.ToDouble(maxValueProp.GetValue(null));
                    // ReSharper disable once PossibleNullReferenceException
                    double minCap = Convert.ToDouble(minValueProp.GetValue(null));

                    string formatter = Util.GetStepFormatter(step);

                    void UpdateMinMax()
                    {
                        CallbackInfo callback =
                            GetMinMax(property, progressBarAttribute, info, parent);
                        if (callback.Error != "")
                        {
                            UIToolkitUtils.SetHelpBox(helpBox, callback.Error);
                            return;
                        }

                        // Debug.Log($"callback={callback.MinValue}/{callback.MaxValue}");

                        field.ProgressBarElementDouble.SetConfig(callback, minCap, maxCap, step, formatter);
                    }

                    UpdateMinMax();
                    SaintsEditorApplicationChanged.OnAnyEvent.AddListener(UpdateMinMax);
                    container.RegisterCallback<DetachFromPanelEvent>(_ =>
                        SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(UpdateMinMax));
                    field.ProgressBarElementDouble.BindProperty(property);  // refresh value
                    field.TrackSerializedObjectValue(property.serializedObject, _ => UpdateMinMax());
                }
                    break;
            }
        }

        public static VisualElement UIToolkitValueEditSByte(VisualElement oldElement, ProgressBarAttribute progressBarAttribute, string label, sbyte value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const sbyte min = sbyte.MinValue;
            const sbyte max = sbyte.MaxValue;
            CallbackInfo info =
                GetMinMaxShowInInspector(progressBarAttribute, value, targets[0]);

            if (oldElement is ProgressBarFieldInt oldF)
            {
                if (info.Error == "")
                {
                    oldF.ProgressBarElementInt.SetConfig(info, min, max, (int)progressBarAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            ProgressBarElementInt element = new ProgressBarElementInt();
            ProgressBarFieldInt field = new ProgressBarFieldInt(label, element);
            if (info.Error == "")
            {
                field.ProgressBarElementInt.SetConfig(info, min, max, (int)progressBarAttribute.Step);
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

        public static VisualElement UIToolkitValueEditByte(VisualElement oldElement, ProgressBarAttribute progressBarAttribute, string label, byte value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const byte min = byte.MinValue;
            const byte max = byte.MaxValue;
            CallbackInfo info =
                GetMinMaxShowInInspector(progressBarAttribute, value, targets[0]);

            if (oldElement is ProgressBarFieldInt oldF)
            {
                if (info.Error == "")
                {
                    oldF.ProgressBarElementInt.SetConfig(info, min, max, (int)progressBarAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            ProgressBarElementInt element = new ProgressBarElementInt();
            ProgressBarFieldInt field = new ProgressBarFieldInt(label, element);
            if (info.Error == "")
            {
                field.ProgressBarElementInt.SetConfig(info, min, max, (int)progressBarAttribute.Step);
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

        public static VisualElement UIToolkitValueEditShort(VisualElement oldElement, ProgressBarAttribute progressBarAttribute, string label, short value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const short min = short.MinValue;
            const short max = short.MaxValue;
            CallbackInfo info =
                GetMinMaxShowInInspector(progressBarAttribute, value, targets[0]);

            if (oldElement is ProgressBarFieldInt oldF)
            {
                if (info.Error == "")
                {
                    oldF.ProgressBarElementInt.SetConfig(info, min, max, (int)progressBarAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            ProgressBarElementInt element = new ProgressBarElementInt();
            ProgressBarFieldInt field = new ProgressBarFieldInt(label, element);
            if (info.Error == "")
            {
                field.ProgressBarElementInt.SetConfig(info, min, max, (int)progressBarAttribute.Step);
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

        public static VisualElement UIToolkitValueEditUShort(VisualElement oldElement, ProgressBarAttribute progressBarAttribute, string label, ushort value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const ushort min = ushort.MinValue;
            const ushort max = ushort.MaxValue;
            CallbackInfo info =
                GetMinMaxShowInInspector(progressBarAttribute, value, targets[0]);

            if (oldElement is ProgressBarFieldInt oldF)
            {
                if (info.Error == "")
                {
                    oldF.ProgressBarElementInt.SetConfig(info, min, max, (int)progressBarAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            ProgressBarElementInt element = new ProgressBarElementInt();
            ProgressBarFieldInt field = new ProgressBarFieldInt(label, element);
            if (info.Error == "")
            {
                field.ProgressBarElementInt.SetConfig(info, min, max, (int)progressBarAttribute.Step);
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

        public static VisualElement UIToolkitValueEditInt(VisualElement oldElement, ProgressBarAttribute progressBarAttribute, string label, int value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const int min = int.MinValue;
            const int max = int.MaxValue;
            CallbackInfo info =
                GetMinMaxShowInInspector(progressBarAttribute, value, targets[0]);

            if (oldElement is ProgressBarFieldInt oldF)
            {
                if (info.Error == "")
                {
                    oldF.ProgressBarElementInt.SetConfig(info, min, max, (int)progressBarAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            ProgressBarElementInt element = new ProgressBarElementInt();
            ProgressBarFieldInt field = new ProgressBarFieldInt(label, element);
            if (info.Error == "")
            {
                field.ProgressBarElementInt.SetConfig(info, min, max, (int)progressBarAttribute.Step);
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

        public static VisualElement UIToolkitValueEditFloat(VisualElement oldElement, ProgressBarAttribute progressBarAttribute, string label, float value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const float min = float.MinValue;
            const float max = float.MaxValue;
            CallbackInfo info =
                GetMinMaxShowInInspector(progressBarAttribute, value, targets[0]);

            string formatter = Util.GetStepFormatter(progressBarAttribute.Step);

            if (oldElement is ProgressBarFieldDouble oldF)
            {
                if (info.Error == "")
                {
                    oldF.ProgressBarElementDouble.SetConfig(info, min, max, progressBarAttribute.Step, formatter);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            ProgressBarElementDouble element = new ProgressBarElementDouble();
            ProgressBarFieldDouble field = new ProgressBarFieldDouble(label, element);
            if (info.Error == "")
            {
                field.ProgressBarElementDouble.SetConfig(info, min, max, progressBarAttribute.Step, formatter);
            }

            field.value = value;

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                element.RegisterValueChangedCallback(evt =>
                {
                    (bool ok, double result) = element.GetNumber(evt.newValue);
                    if (!ok)
                    {
                        return;
                    }

                    beforeSet?.Invoke(value);
                    setterOrNull((float)result);
                });
            }
            return field;
        }
        public static VisualElement UIToolkitValueEditDouble(VisualElement oldElement, ProgressBarAttribute progressBarAttribute, string label, double value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            const double min = double.MinValue;
            const double max = double.MaxValue;
            CallbackInfo info =
                GetMinMaxShowInInspector(progressBarAttribute, value, targets[0]);

            string formatter = Util.GetStepFormatter(progressBarAttribute.Step);

            if (oldElement is ProgressBarFieldDouble oldF)
            {
                if (info.Error == "")
                {
                    oldF.ProgressBarElementDouble.SetConfig(info, min, max, progressBarAttribute.Step, formatter);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            ProgressBarElementDouble element = new ProgressBarElementDouble();
            ProgressBarFieldDouble field = new ProgressBarFieldDouble(label, element);
            if (info.Error == "")
            {
                field.ProgressBarElementDouble.SetConfig(info, min, max, progressBarAttribute.Step, formatter);
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
