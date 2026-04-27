#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.SceneDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
    public partial class SortingLayerAttributeDrawer
    {

        // private static string NameButtonField(SerializedProperty property) =>
        //     $"{property.propertyPath}__SortingLayer_Button";
        //
        // private static string NameHelpBox(SerializedProperty property) =>
        //     $"{property.propertyPath}__SortingLayer_HelpBox";

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
                    SortingLayerIntElement sortingLayerIntElement = new SortingLayerIntElement
                    {
                        bindingPath = property.propertyPath,
                    };
                    SortingLayerIntField r = new SortingLayerIntField(GetPreferredLabel(property), sortingLayerIntElement);

                    r.AddToClassList(ClassAllowDisable);
                    r.AddToClassList(SortingLayerIntField.alignedFieldUssClassName);
                    return r;
                }
                case SerializedPropertyType.String:
                {
                    SortingLayerStringElement sortingLayerStringElement = new SortingLayerStringElement
                    {
                        bindingPath = property.propertyPath,
                    };

                    SortingLayerStringField r = new SortingLayerStringField(GetPreferredLabel(property), sortingLayerStringElement);
                    r.AddToClassList(ClassAllowDisable);
                    r.AddToClassList(SortingLayerStringField.alignedFieldUssClassName);
                    return r;
                }
                default:
                    return new VisualElement();
            }
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.String:
                    return null;
                default:
                    return new HelpBox($"Type {property.propertyType} is not int or string.", HelpBoxMessageType.Error)
                    {
                        style =
                        {
                            flexGrow = 1,
                        },
                    };
            }
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    SortingLayerIntField intDropdownField = container.Q<SortingLayerIntField>();
                    AddContextualMenuManipulator(intDropdownField, property, onValueChangedCallback, info, parent);
                    intDropdownField.TrackPropertyValue(property, p => onValueChangedCallback(p.intValue));
                }
                    break;
                case SerializedPropertyType.String:
                {
                    SortingLayerStringField layerStringField = container.Q<SortingLayerStringField>();
                    AddContextualMenuManipulator(layerStringField, property, onValueChangedCallback, info, parent);
                    layerStringField.TrackPropertyValue(property, p => onValueChangedCallback(p.stringValue));
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
                    foreach (SortingLayer sortingLayer in SortingLayer.layers)
                    {
                        if (sortingLayer.name == clipboardText)
                        {
                            evt.menu.AppendAction($"Paste \"{sortingLayer.name}\"", _ =>
                            {
                                property.stringValue = sortingLayer.name;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, sortingLayer.name);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(sortingLayer.name);
                            });
                            return;
                        }
                    }
                }
                else
                {
                    foreach (SortingLayer sortingLayer in SortingLayer.layers)
                    {
                        // ReSharper disable once InvertIf
                        if (sortingLayer.name == clipboardText
                            || canBeInt && sortingLayer.value == clipboardInt)
                        {
                            evt.menu.AppendAction($"Paste \"{sortingLayer.name}\"({sortingLayer.value})", _ =>
                            {
                                property.intValue = sortingLayer.value;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, sortingLayer.value);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(sortingLayer.value);
                            });
                            return;
                        }
                    }
                }
            }));
        }


        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, SortingLayerAttribute sortingLayerAttribute, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is SortingLayerStringField sls)
            {
                sls.SetValueWithoutNotify(value);
                return null;
            }

            SortingLayerStringElement visualInput = new SortingLayerStringElement
            {
                value = value,
            };
            SortingLayerStringField element =
                new SortingLayerStringField(label, visualInput)
                {
                    value = value,
                };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                visualInput.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }
            return element;
        }

        public static VisualElement UIToolkitValueEditInt(VisualElement oldElement, SortingLayerAttribute sortingLayerAttribute, string label, int value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes)
        {
            if (oldElement is SortingLayerIntField sls)
            {
                sls.SetValueWithoutNotify(value);
                return null;
            }

            SortingLayerIntElement visualInput = new SortingLayerIntElement
            {
                value = value,
            };
            SortingLayerIntField element =
                new SortingLayerIntField(label, visualInput)
                {
                    value = value,
                };

            UIToolkitUtils.UIToolkitValueEditAfterProcess(element, setterOrNull,
                labelGrayColor, inHorizontalLayout);

            if (setterOrNull != null)
            {
                visualInput.RegisterValueChangedCallback(evt =>
                {
                    beforeSet?.Invoke(value);
                    setterOrNull(evt.newValue);
                });
            }
            return element;
        }
    }
}
#endif
