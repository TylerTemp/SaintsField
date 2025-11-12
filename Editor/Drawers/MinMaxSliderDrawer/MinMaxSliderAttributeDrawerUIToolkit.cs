#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.PropRangeDrawer;
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

        // private static string NameMinMaxSliderField(SerializedProperty property) =>
        //     $"{property.propertyPath}__MinMaxSliderField";
        //
        // // public class MinMaxSliderField : BaseField<Vector2>
        // // {
        // //     public MinMaxSliderField(string label, VisualElement visualInput) : base(label, visualInput)
        // //     {
        // //     }
        // // }
        //
        // private static string NameSlider(SerializedProperty property) =>
        //     $"{property.propertyPath}__MinMaxSlider_Slider";
        //
        // private static string NameMinInteger(SerializedProperty property) =>
        //     $"{property.propertyPath}__MinMaxSlider_MinIntegerField";
        //
        // private static string NameMaxInteger(SerializedProperty property) =>
        //     $"{property.propertyPath}__MinMaxSlider_MaxIntegerField";
        //
        // private static string NameMinFloat(SerializedProperty property) =>
        //     $"{property.propertyPath}__MinMaxSlider_MinFloatField";
        //
        // private static string NameMaxFloat(SerializedProperty property) =>
        //     $"{property.propertyPath}__MinMaxSlider_MaxFloatField";
        //
        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__MinMaxSlider_HelpBox";
        //
        // private const int InputWidth = 50;
        //
        // private record UserData
        // {
        //     public float FreeMin;
        //     public float FreeMax;
        // }

        // // Oh hell... I don't want to properly do this anymore
        // private class BindableV2IntField: BaseField<Vector2Int>
        // {
        //     public BindableV2IntField(VisualElement visualInput) : base(null, visualInput)
        //     {
        //         style.marginLeft = style.marginRight = 0;
        //     }
        // }
        //
        // public class BindableV2Field : BaseField<Vector2>
        // {
        //     public BindableV2Field(VisualElement visualInput) : base(null, visualInput)
        //     {
        //         style.marginLeft = style.marginRight = 0;
        //     }
        // }

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2Int:
                {
                    MinMaxSliderElementInt element = new MinMaxSliderElementInt(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
                    MinMaxSliderFieldInt field = new MinMaxSliderFieldInt(GetPreferredLabel(property), element);
                    field.AddToClassList(PropRangeIntField.alignedFieldUssClassName);
                    field.AddToClassList(ClassAllowDisable);
                    return field;
                }
                case SerializedPropertyType.Vector2:
                {
                    MinMaxSliderElementFloat element = new MinMaxSliderElementFloat(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
                    MinMaxSliderFieldFloat field = new MinMaxSliderFieldFloat(GetPreferredLabel(property), element);
                    field.AddToClassList(PropRangeIntField.alignedFieldUssClassName);
                    field.AddToClassList(ClassAllowDisable);
                    return field;
                }
                default:
                {
                    PropertyField fallback = PropertyFieldFallbackUIToolkit(property, GetPreferredLabel(property));
                    fallback.AddToClassList(ClassFieldUIToolkit(property));
                    fallback.AddToClassList(ClassAllowDisable);
                    return fallback;
                }
            }
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

        private static (object minValue, string minError, object maxValue, string maxError) GetMinMax(SerializedProperty property, MinMaxSliderAttribute minMaxSliderAttribute,
            MemberInfo info, object parentTarget)
        {
            object minValue;
            if (minMaxSliderAttribute.MinCallback == null)
            {
                minValue = minMaxSliderAttribute.Min;
            }
            else
            {
                (string getError, object getValue) =
                    Util.GetOf<object>(minMaxSliderAttribute.MinCallback, 0, property, info, parentTarget);
                if (getError != "")
                {
                    return (null, getError, null, "");
                }
                minValue = getValue;
            }

            object maxValue;
            if (minMaxSliderAttribute.MaxCallback == null)
            {
                maxValue = minMaxSliderAttribute.Max;
            }
            else
            {
                (string getError, object getValue) =
                    Util.GetOf<object>(minMaxSliderAttribute.MaxCallback, 0f, property, info, parentTarget);
                if (getError != "")
                {
                    return (minValue, "", null, getError);
                }

                maxValue = getValue;
            }
            return (minValue, "", maxValue, "");
        }

        private static (object minValue, string minError, object maxValue, string maxError) GetMinMaxForShowInInspector(
            MinMaxSliderAttribute minMaxSliderAttribute, object curValue, object target)
        {
            object minValue;
            if (minMaxSliderAttribute.MinCallback == null)
            {
                minValue = minMaxSliderAttribute.Min;
            }
            else
            {
                (object getValue, string getError) = GetCallbackForShowInInspector(minMaxSliderAttribute.MinCallback, curValue, target);
                if (getError != "")
                {
                    return (null, getError, null, "");
                }
                minValue = getValue;
            }

            object maxValue;
            if (minMaxSliderAttribute.MaxCallback == null)
            {
                maxValue = minMaxSliderAttribute.Max;
            }
            else
            {
                (object getValue, string getError) = GetCallbackForShowInInspector(minMaxSliderAttribute.MaxCallback, curValue, target);
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
            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;
            float step = minMaxSliderAttribute.Step;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2Int:
                {
                    MinMaxSliderFieldInt field = container.Q<MinMaxSliderFieldInt>();
                    UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                    field.MinMaxSliderElementInt.BindHelpBox(helpBox);

                    int intStep = (int)step;

                    void UpdateMinMax()
                    {
                        (object minValue, string minError, object maxValue, string maxError) =
                            GetMinMax(property, minMaxSliderAttribute, info, parent);
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

                        field.MinMaxSliderElementInt.SetConfig(minValue, maxValue, intStep);
                    }

                    UpdateMinMax();
                    SaintsEditorApplicationChanged.OnAnyEvent.AddListener(UpdateMinMax);
                    container.RegisterCallback<DetachFromPanelEvent>(_ =>
                        SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(UpdateMinMax));

                    // Well, this does not work, tf Unity
                    // field.MinMaxSliderElementInt.bindingPath = property.propertyPath;
                    field.bindingPath = property.propertyPath;  // just let prefab blue bar works
                    field.BindProperty(property);  // just let prefab blue bar works
                    field.value = property.vector2IntValue;
                    field.RegisterValueChangedCallback(evt =>
                    {
                        // Debug.Log(evt.newValue);
                        Vector2Int newValue = evt.newValue;
                        property.vector2IntValue = newValue;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback(newValue);
                    });
                }
                    break;
                case SerializedPropertyType.Vector2:
                {
                    MinMaxSliderFieldFloat field = container.Q<MinMaxSliderFieldFloat>();
                    UIToolkitUtils.AddContextualMenuManipulator(field.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
                    field.MinMaxSliderElementFloat.BindHelpBox(helpBox);

                    void UpdateMinMax()
                    {
                        (object minValue, string minError, object maxValue, string maxError) =
                            GetMinMax(property, minMaxSliderAttribute, info, parent);
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

                        field.MinMaxSliderElementFloat.SetConfig(minValue, maxValue, step);
                    }

                    UpdateMinMax();
                    SaintsEditorApplicationChanged.OnAnyEvent.AddListener(UpdateMinMax);
                    container.RegisterCallback<DetachFromPanelEvent>(_ =>
                        SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(UpdateMinMax));

                    // Well, this does not work, tf Unity
                    // field.MinMaxSliderElementInt.bindingPath = property.propertyPath;
                    field.bindingPath = property.propertyPath;  // just let prefab blue bar works
                    field.BindProperty(property);  // just let prefab blue bar works
                    field.value = property.vector2Value;
                    field.RegisterValueChangedCallback(evt =>
                    {
                        // Debug.Log(evt.newValue);
                        Vector2 newValue = evt.newValue;
                        property.vector2Value = newValue;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback(newValue);
                    });
                }
                    break;
                default:
                    return;
            }
        }

        public static VisualElement UIToolkitValueEditVector2(VisualElement oldElement, MinMaxSliderAttribute minMaxSliderAttribute, string label, Vector2 value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            (object minValue, string minError, object maxValue, string maxError) =
                GetMinMaxForShowInInspector(minMaxSliderAttribute, value, targets[0]);

            if (oldElement is MinMaxSliderFieldFloat oldF)
            {
                if (minError == "" && maxError == "")
                {
                    oldF.MinMaxSliderElementFloat.SetConfig(minValue, maxValue, minMaxSliderAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            MinMaxSliderElementFloat element = new MinMaxSliderElementFloat(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
            MinMaxSliderFieldFloat field =
                new MinMaxSliderFieldFloat(label, element);
            if (minError == "" && maxError == "")
            {
                field.MinMaxSliderElementFloat.SetConfig(minValue, maxValue, minMaxSliderAttribute.Step);
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

        public static VisualElement UIToolkitValueEditVector2Int(VisualElement oldElement, MinMaxSliderAttribute minMaxSliderAttribute, string label, Vector2Int value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            (object minValue, string minError, object maxValue, string maxError) =
                GetMinMaxForShowInInspector(minMaxSliderAttribute, value, targets[0]);

            if (oldElement is MinMaxSliderFieldInt oldF)
            {
                if (minError == "" && maxError == "")
                {
                    oldF.MinMaxSliderElementInt.SetConfig(minValue, maxValue, (int)minMaxSliderAttribute.Step);
                }

                oldF.SetValueWithoutNotify(value);
                return null;
            }

            MinMaxSliderElementInt element = new MinMaxSliderElementInt(allAttributes.OfType<AdaptAttribute>().FirstOrDefault());
            MinMaxSliderFieldInt field =
                new MinMaxSliderFieldInt(label, element);
            if (minError == "" && maxError == "")
            {
                field.MinMaxSliderElementInt.SetConfig(minValue, maxValue, (int)minMaxSliderAttribute.Step);
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
