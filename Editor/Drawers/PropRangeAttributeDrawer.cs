using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PropRangeAttribute))]
    public class PropRangeAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public bool IsFloat;
            public float MinValue;
            public float MaxValue;
            public float Step;
            public string Error;
            // ReSharper enable InconsistentNaming
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object parentTarget)
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
                (string getError, float getValue) = Util.GetOf(propRangeAttribute.MinCallback, 0f, property, info, parentTarget);
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
                (string getError, float getValue) = Util.GetOf(propRangeAttribute.MaxCallback, 0f, property, info, parentTarget);
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
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parentTarget)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS
            Debug.Log($"#PropRange# #DrawField# for {property.propertyPath}");
#endif

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parentTarget);
            if(metaInfo.Error != "")
            {
                _error = metaInfo.Error;
                DefaultDrawer(position, property, label, info);
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

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        public class PropRangeField: BaseField<float>
        {
            public PropRangeField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static string NameSlider(SerializedProperty property) => $"{property.propertyPath}__PropRange_Slider";
        private static string NameInteger(SerializedProperty property) => $"{property.propertyPath}__PropRange_IntegerField";
        private static string NameFloat(SerializedProperty property) => $"{property.propertyPath}__PropRange_FloatField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__PropRange_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent)
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
            int index, VisualElement container,
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
                    float parsedValue = GetValue(GetMetaInfo(property, saintsAttribute, info, parent), changed.newValue);
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
                    int parsedValue = (int)GetValue(GetMetaInfo(property, saintsAttribute, info, parent), changed.newValue);
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
        #endregion

#endif
    }
}
