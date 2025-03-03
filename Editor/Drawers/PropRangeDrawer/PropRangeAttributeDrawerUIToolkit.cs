#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    public partial class PropRangeAttributeDrawer
    {
        public class PropRangeField : BaseField<float>
        {
            public PropRangeField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static string NameSlider(SerializedProperty property) => $"{property.propertyPath}__PropRange_Slider";

        private static string NameInteger(SerializedProperty property) =>
            $"{property.propertyPath}__PropRange_IntegerField";

        private static string NameFloat(SerializedProperty property) =>
            $"{property.propertyPath}__PropRange_FloatField";

        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__PropRange_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };

            Slider slider = new Slider("", 0, 1, SliderDirection.Horizontal, 0.5f)
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

            PropRangeField propRangeField = new PropRangeField(property.displayName, root);

            propRangeField.AddToClassList(ClassAllowDisable);
            propRangeField.labelElement.style.overflow = Overflow.Hidden;
            propRangeField.AddToClassList("unity-base-field__aligned");

            return propRangeField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
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
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Slider slider = container.Q<Slider>(NameSlider(property));

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
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
                    float parsedValue = GetValue(GetMetaInfo(property, saintsAttribute, info, parent),
                        changed.newValue);
                    property.floatValue = parsedValue;
                    property.serializedObject.ApplyModifiedProperties();
                    info.SetValue(parent, parsedValue);

                    floatField.SetValueWithoutNotify(parsedValue);
                    slider.SetValueWithoutNotify(parsedValue);
                    onValueChangedCallback.Invoke(parsedValue);
                });
            }
            else
            {
                integerField.value = (int)curValue;
                integerField.RegisterValueChangedCallback(changed =>
                {
                    int parsedValue = (int)GetValue(GetMetaInfo(property, saintsAttribute, info, parent),
                        changed.newValue);
                    property.intValue = parsedValue;
                    property.serializedObject.ApplyModifiedProperties();
                    info.SetValue(parent, parsedValue);

                    slider.SetValueWithoutNotify(parsedValue);
                    integerField.SetValueWithoutNotify(parsedValue);
                    onValueChangedCallback.Invoke(parsedValue);
                });
            }

            slider.RegisterValueChangedCallback(changed =>
            {
                float parsedValue = GetValue(GetMetaInfo(property, saintsAttribute, info, parent), changed.newValue);
                if (property.propertyType == SerializedPropertyType.Float)
                {
                    property.floatValue = parsedValue;
                    property.serializedObject.ApplyModifiedProperties();
                    info.SetValue(parent, parsedValue);

                    floatField.SetValueWithoutNotify(parsedValue);
                    slider.SetValueWithoutNotify(parsedValue);
                    onValueChangedCallback.Invoke(parsedValue);
                }
                else
                {
                    int intValue = (int)parsedValue;
                    property.intValue = intValue;
                    property.serializedObject.ApplyModifiedProperties();
                    info.SetValue(parent, intValue);

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
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            Slider slider = container.Q<Slider>(NameSlider(property));
            MetaInfo curMetaInfo = (MetaInfo)slider.userData;

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            bool changed = false;

            if (metaInfo.Error != curMetaInfo.Error)
            {
                changed = true;
                helpBox.text = metaInfo.Error;
                helpBox.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (metaInfo.MinValue != curMetaInfo.MinValue
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                || metaInfo.MaxValue != curMetaInfo.MaxValue)
            {
                changed = true;
                slider.lowValue = metaInfo.MinValue;
                slider.highValue = metaInfo.MaxValue;
            }

            if (changed)
            {
                slider.userData = metaInfo;
            }
        }


    }
}
#endif
