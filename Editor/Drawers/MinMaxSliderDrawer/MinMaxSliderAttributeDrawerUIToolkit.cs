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
    }
}
#endif
