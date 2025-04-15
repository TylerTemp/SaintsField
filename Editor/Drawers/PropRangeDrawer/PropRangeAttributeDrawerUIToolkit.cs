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
        public class PropRangeField : BaseField<float>
        {
            public PropRangeField(string label, VisualElement visualInput) : base(label, visualInput)
            {
            }
        }

        private static string NamePropRange(SerializedProperty property) => $"{property.propertyPath}__PropRange";
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

            PropRangeField propRangeField = new PropRangeField(GetPreferredLabel(property), root)
            {
                name = NamePropRange(property),
            };

            propRangeField.AddToClassList(ClassAllowDisable);
            propRangeField.labelElement.style.overflow = Overflow.Hidden;
            propRangeField.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);

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

        private float _cachedChangeValue;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {

            AdaptAttribute adaptAttribute = allAttributes.OfType<AdaptAttribute>().FirstOrDefault();

            PropRangeField propRangeField = container.Q<PropRangeField>(name: NamePropRange(property));
            UIToolkitUtils.AddContextualMenuManipulator(propRangeField.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
//             propRangeField.labelElement.AddManipulator(new ContextualMenuManipulator(evt =>
//             {
//                 evt.menu.AppendAction("Copy Property Path", _ => EditorGUIUtility.systemCopyBuffer = property.propertyPath);
// #if SAINTSFIELD_NEWTONSOFT_JSON
//                 evt.menu.AppendSeparator();
//                 evt.menu.AppendAction("Copy", _ =>
//                 {
//                     string content;
//                     // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
//                     if (property.propertyType == SerializedPropertyType.Integer)
//                     {
//                         content = ClipboardUtils.CopyGenericType(property.intValue);
//                     }
//                     else
//                     {
//                         content = ClipboardUtils.CopyGenericType(property.floatValue);
//                     }
//                     EditorGUIUtility.systemCopyBuffer = content;
//                 });
//
//                 JToken jtoken;
//                 try
//                 {
//                     jtoken = JToken.Parse(EditorGUIUtility.systemCopyBuffer);
//                 }
//                 catch (Exception)
//                 {
//                     evt.menu.AppendAction("Paste", _ => { }, DropdownMenuAction.Status.Disabled);
//                     return;
//                 }
//
//                 int intResult = 0;
//                 float floatResult = 0;
//
//                 // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
//                 switch (jtoken.Type)
//                 {
//                     case JTokenType.Integer:
//                         intResult = jtoken.Value<int>();
//                         break;
//                     case JTokenType.Float:
//                         floatResult = jtoken.Value<float>();
//                         break;
//                     default:
//                         evt.menu.AppendAction("Paste", _ => { }, DropdownMenuAction.Status.Disabled);
//                         return;
//                 }
//
//                 evt.menu.AppendAction("Paste", _ =>
//                 {
//                     if (property.propertyType == SerializedPropertyType.Integer)
//                     {
// #if SAINTSFIELD_DEBUG
//                         Debug.Log($"Pasted: {intResult}");
// #endif
//                         property.intValue = intResult;
//                     }
//                     else
//                     {
// #if SAINTSFIELD_DEBUG
//                         Debug.Log($"Pasted: {floatResult}");
// #endif
//                         property.floatValue = floatResult;
//                     }
//                     property.serializedObject.ApplyModifiedProperties();
//                     if (property.propertyType == SerializedPropertyType.Integer)
//                     {
//                         onValueChangedCallback.Invoke(intResult);
//                     }
//                     else
//                     {
//                         onValueChangedCallback.Invoke(floatResult);
//                     }
//                 });
// #endif
//             }));

            Slider slider = propRangeField.Q<Slider>(NameSlider(property));

            PropRangeAttribute propRangeAttribute = (PropRangeAttribute)saintsAttribute;

            MetaInfo metaInfo = GetMetaInfo(property, propRangeAttribute, info, parent);
            bool isFloat = metaInfo.IsFloat;
            (string error, double value) curValueInfo = GetPreValue(isFloat ? property.floatValue : property.intValue, adaptAttribute);
            if (curValueInfo.error != "")
            {
                return;
            }
            float curValue = (float) curValueInfo.value;

            (string error, double value) minValueInfo = GetPreValue(metaInfo.MinValue, adaptAttribute);
            if (minValueInfo.error != "")
            {
                return;
            }
            float minValue = (float)minValueInfo.value;

            (string error, double value) maxValueInfo = GetPreValue(metaInfo.MaxValue, adaptAttribute);
            if (maxValueInfo.error != "")
            {
                return;
            }
            float maxValue = (float)maxValueInfo.value;

            // Debug.Log($"{minValue}/{maxValue}");

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
                    float adaptedValue = changed.newValue;
                    (string error, double value) postValueInfo = GetPostValue(changed.newValue, adaptAttribute);
                    if (postValueInfo.error != "")
                    {
                        return;
                    }
                    float parsedValue = GetValue(GetMetaInfo(property, saintsAttribute, info, parent),
                        (float)postValueInfo.value);
                    property.doubleValue = _cachedChangeValue = parsedValue;
                    property.serializedObject.ApplyModifiedProperties();

                    floatField.SetValueWithoutNotify(adaptedValue);
                    slider.SetValueWithoutNotify(adaptedValue);
                    info.SetValue(parent, parsedValue);
                    onValueChangedCallback.Invoke(parsedValue);
                });
            }
            else
            {
                integerField.value = (int)curValue;
                integerField.RegisterValueChangedCallback(changed =>
                {
                    float adaptedValue = changed.newValue;
                    (string error, double value) postValueInfo = GetPostValue(changed.newValue, adaptAttribute);
                    if (postValueInfo.error != "")
                    {
                        return;
                    }
                    int parsedValue = (int)GetValue(GetMetaInfo(property, saintsAttribute, info, parent),
                        (float)postValueInfo.value);
                    property.intValue = parsedValue;
                    _cachedChangeValue = property.intValue;
                    property.serializedObject.ApplyModifiedProperties();

                    floatField.SetValueWithoutNotify(adaptedValue);
                    slider.SetValueWithoutNotify(adaptedValue);
                    info.SetValue(parent, parsedValue);
                    onValueChangedCallback.Invoke(parsedValue);
                });
            }

            slider.RegisterValueChangedCallback(changed =>
            {
                float adaptedValue = changed.newValue;
                (string error, double value) postValueInfo = GetPostValue(adaptedValue, adaptAttribute);
                if (postValueInfo.error != "")
                {
                    return;
                }

                float parsedValue = GetValue(GetMetaInfo(property, saintsAttribute, info, parent), (float)postValueInfo.value);

                (string error, double value) preValueInfo = GetPreValue(parsedValue, adaptAttribute);
                if (preValueInfo.error != "")
                {
                    return;
                }

                if (property.propertyType == SerializedPropertyType.Float)
                {
                    property.doubleValue = parsedValue;
                    _cachedChangeValue = parsedValue;
                    property.serializedObject.ApplyModifiedProperties();

                    floatField.SetValueWithoutNotify((float)preValueInfo.value);
                    slider.SetValueWithoutNotify((float)preValueInfo.value);
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, parsedValue);
                    onValueChangedCallback.Invoke(parsedValue);
                }
                else
                {
                    int intValue = (int)parsedValue;
                    property.intValue = intValue;
                    _cachedChangeValue = intValue;
                    property.serializedObject.ApplyModifiedProperties();

                    integerField.SetValueWithoutNotify((int) preValueInfo.value);
                    slider.SetValueWithoutNotify((int) preValueInfo.value);
                    // info.SetValue(parent, intValue);
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, intValue);
                    onValueChangedCallback.Invoke(intValue);
                }

                property.serializedObject.ApplyModifiedProperties();
            });

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            if (metaInfo.Error != "")
            {
                helpBox.text = metaInfo.Error;
                helpBox.style.display = DisplayStyle.Flex;
            }

            helpBox.TrackPropertyValue(property, _ => UpdateExternal());
            helpBox.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(UpdateExternal));
            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(UpdateExternal);

            return;

            void UpdateExternal()
            {
                if (metaInfo.IsFloat)
                {
                    if (Mathf.Approximately(property.floatValue, _cachedChangeValue))
                    {
                        return;
                    }
                }
                else
                {
                    if (Mathf.Approximately(property.intValue, _cachedChangeValue))
                    {
                        return;
                    }
                }
                UpdateDisplay(property, propRangeAttribute, adaptAttribute, container, info);
            }
        }

        private static void UpdateDisplay(SerializedProperty property, PropRangeAttribute propRangeAttribute,
            AdaptAttribute adaptAttribute,
            VisualElement container, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            MetaInfo metaInfo = GetMetaInfo(property, propRangeAttribute, info, parent);

            Slider slider = container.Q<Slider>(NameSlider(property));
            // MetaInfo curMetaInfo = (MetaInfo)slider.userData;

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

            if (metaInfo.Error != helpBox.text)
            {
                helpBox.text = metaInfo.Error;
                helpBox.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                if (metaInfo.Error != "")
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(propRangeAttribute.MinCallback))
            {
                slider.lowValue = metaInfo.MinValue;
            }

            if (!string.IsNullOrEmpty(propRangeAttribute.MaxCallback))
            {
                slider.highValue = metaInfo.MaxValue;
            }

            (string error, double value) curValueInfo = GetPreValue(metaInfo.IsFloat ? property.floatValue : property.intValue, adaptAttribute);
            if (curValueInfo.error != "")
            {
                return;
            }
            float curValue = (float) curValueInfo.value;

            IntegerField integerField = container.Q<IntegerField>(NameInteger(property));
            FloatField floatField = container.Q<FloatField>(NameFloat(property));

            if (metaInfo.IsFloat)
            {
                floatField.SetValueWithoutNotify(curValue);
            }
            else
            {
                integerField.SetValueWithoutNotify((int)curValue);
            }

            // let it trigger the change
            slider.value = curValue;
        }
    }
}
#endif
