#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.LayerDrawer
{
    public partial class LayerAttributeDrawer
    {
        // private static string NameLayer(SerializedProperty property) => $"{property.propertyPath}__Layer";
        // private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__Layer_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    LayerIntDropdownElement intDropdownElement = new LayerIntDropdownElement
                    {
                        bindingPath = property.propertyPath
                    };
                    IntDropdownField field = new IntDropdownField(GetPreferredLabel(property), intDropdownElement);
                    field.AddToClassList(IntDropdownField.alignedFieldUssClassName);
                    field.AddToClassList(ClassAllowDisable);
                    return field;
                }
                case SerializedPropertyType.String:
                {
                    LayerStringDropdownElement layerStringStringDropdown = new LayerStringDropdownElement
                    {
                        bindingPath = property.propertyPath
                    };
                    StringDropdownField field = new StringDropdownField(GetPreferredLabel(property), layerStringStringDropdown);
                    field.AddToClassList(IntDropdownField.alignedFieldUssClassName);
                    field.AddToClassList(ClassAllowDisable);
                    return field;
                }
                case SerializedPropertyType.LayerMask:
                {
                    LayerMaskDropdownElement layerMaskDropdownElement = new LayerMaskDropdownElement
                        {
                            bindingPath = property.propertyPath
                        };
                    LayerMaskDropdownField field =
                        new LayerMaskDropdownField(GetPreferredLabel(property), layerMaskDropdownElement)
                        {
                            bindingPath = property.propertyPath
                        };
                    field.AddToClassList(LayerMaskDropdownField.alignedFieldUssClassName);
                    field.AddToClassList(ClassAllowDisable);
                    // layerMaskDropdownElement.BindProperty(property);
                    // Debug.Log($"return {layerMaskDropdownElement}");
                    return field;
                }
                default:
                    return new VisualElement();
            }
            // int curSelected = property.propertyType == SerializedPropertyType.Integer
            //     ? property.intValue
            //     : LayerMask.NameToLayer(property.stringValue);
            //
            // LayerField layerField = new LayerField(GetPreferredLabel(property), curSelected)
            // {
            //     name = NameLayer(property),
            // };
            // layerField.AddToClassList(BaseField<Object>.alignedFieldUssClassName);
            // layerField.AddToClassList(ClassAllowDisable);
            //
            // return layerField;
        }

        // protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index,
        //     IReadOnlyList<PropertyAttribute> allAttributes,
        //     VisualElement container, FieldInfo info, object parent)
        // {
        //     return new HelpBox("", HelpBoxMessageType.Error)
        //     {
        //         style =
        //         {
        //             display = DisplayStyle.None,
        //             flexGrow = 1,
        //         },
        //         name = NameHelpBox(property),
        //     };
        // }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            // HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            // RefreshHelpBox(property, helpBox);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    IntDropdownField intDropdownField = container.Q<IntDropdownField>();
                    AddContextualMenuManipulator(intDropdownField, property, onValueChangedCallback, info, parent);
                    LayerIntDropdownElement layerIntDropdownElement = intDropdownField.Q<LayerIntDropdownElement>();
                    layerIntDropdownElement.BindDrop(intDropdownField);
                    layerIntDropdownElement.RegisterValueChangedCallback(v => onValueChangedCallback(v.newValue));

                    // intDropdownField.Button.clicked += () => MakeDropdown(property, intDropdownField, onValueChangedCallback, info, parent);
                }
                    break;
                case SerializedPropertyType.String:
                {
                    StringDropdownField layerStringField = container.Q<StringDropdownField>();
                    AddContextualMenuManipulator(layerStringField, property, onValueChangedCallback, info, parent);
                    LayerStringDropdownElement layerStringDropdownElement = layerStringField.Q<LayerStringDropdownElement>();
                    layerStringDropdownElement.BindDrop(layerStringField);
                    layerStringDropdownElement.RegisterValueChangedCallback(v => onValueChangedCallback(v.newValue));

                    // layerStringField.Button.clicked += () => MakeDropdown(property, layerStringField, onValueChangedCallback, info, parent);
                }
                    break;
                case SerializedPropertyType.LayerMask:
                {
                    LayerMaskDropdownField layerMaskField = container.Q<LayerMaskDropdownField>();
                    AddContextualMenuManipulator(layerMaskField, property, onValueChangedCallback, info, parent);
                    layerMaskField.RegisterValueChangedCallback(v =>
                        onValueChangedCallback(new LayerMask
                        {
                            value = v.newValue,
                        }));
                }
                    break;
                default:
                    return;
            }
        }

        private static void AddContextualMenuManipulator(VisualElement root, SerializedProperty property,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.AddContextualMenuManipulator(root, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            bool isString = property.propertyType == SerializedPropertyType.String;
            bool isMask = property.propertyType == SerializedPropertyType.LayerMask;
            root.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                string clipboardText = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrEmpty(clipboardText))
                {
                    return;
                }

                bool canBeInt = int.TryParse(clipboardText, out int clipboardInt);

                if (isString)
                {
                    foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
                    {
                        if (layerInfo.Name == clipboardText
                            || canBeInt && layerInfo.Value == clipboardInt)
                        {
                            evt.menu.AppendAction($"Paste \"{layerInfo.Name}\"", _ =>
                            {
                                property.stringValue = layerInfo.Name;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, layerInfo.Name);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(layerInfo.Name);
                            });
                            return;
                        }
                    }
                }
                else
                {
                    foreach (LayerUtils.LayerInfo layerInfo in LayerUtils.GetAllLayers())
                    {
                        if (layerInfo.Name == clipboardText
                            || canBeInt && (
                                    isMask
                                        ? layerInfo.Mask == clipboardInt
                                        : layerInfo.Value == clipboardInt
                                ))
                        {
                            evt.menu.AppendAction($"Paste \"{layerInfo.Name}\"({layerInfo.Value})", _ =>
                            {
                                int targetValue = isMask ? layerInfo.Mask : layerInfo.Value;
                                property.intValue = targetValue;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, targetValue);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(targetValue);
                            });
                            return;
                        }
                    }
                }
            }));
        }

        public static VisualElement UIToolkitValueEditLayerMask(VisualElement oldElement, string label, LayerMask value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is LayerMaskDropdownField lmdf)
            {
                lmdf.SetValueWithoutNotify(value.value);
                return null;
            }

            LayerMaskDropdownElement layerMaskDropdownElement = new LayerMaskDropdownElement
            {
                value = value.value,
            };
            LayerMaskDropdownField element =
                new LayerMaskDropdownField(label, layerMaskDropdownElement)
                {
                    value = value.value,
                };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                layerMaskDropdownElement.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull((LayerMask)evt.newValue);
                });
            }
            return element;
        }


        public static VisualElement UIToolkitValueEditInt(VisualElement oldElement, string label, int value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is IntDropdownField intDropdownField)
            {
                LayerIntDropdownElement layerIntDropdownElement = intDropdownField.Q<LayerIntDropdownElement>();
                if(layerIntDropdownElement != null)
                {
                    layerIntDropdownElement.SetValueWithoutNotify(value);
                    return null;
                }
            }

            LayerIntDropdownElement intDropdownElement = new LayerIntDropdownElement
            {
                value = value,
            };
            IntDropdownField element = new IntDropdownField(label, intDropdownElement);
            intDropdownElement.BindDrop(element);

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                intDropdownElement.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }
            return element;
        }

        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is StringDropdownField stringDropdownField)
            {
                LayerStringDropdownElement layerStringDropdownElement =
                    stringDropdownField.Q<LayerStringDropdownElement>();
                if(layerStringDropdownElement != null)
                {
                    // Debug.Log($"renderer update string {value}");
                    layerStringDropdownElement.SetValueWithoutNotify(value);
                    return null;
                }
            }

            LayerStringDropdownElement stringDropdownElement = new LayerStringDropdownElement
            {
                value = value,
            };
            StringDropdownField element = new StringDropdownField(label, stringDropdownElement);
            stringDropdownElement.BindDrop(element);

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                stringDropdownElement.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    // Debug.Log($"renderer set string {evt.newValue}");
                    setterOrNull(evt.newValue);
                });
            }

            return element;
        }
    }
}
#endif
