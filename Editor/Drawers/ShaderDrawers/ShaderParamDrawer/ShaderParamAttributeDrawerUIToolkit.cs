#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.SceneDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderParamDrawer
{
    public partial class ShaderParamAttributeDrawer
    {
        // private static string DropdownButtonName(SerializedProperty property) => $"{property.propertyPath}__ShaderParam_DropdownButton";
        private static string HelpBoxName(SerializedProperty property) => $"{property.propertyPath}__ShaderParam_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            ShaderParamAttribute shaderParamAttribute = (ShaderParamAttribute) saintsAttribute;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    ShaderParamIntElement intDropdownElement = new ShaderParamIntElement(shaderParamAttribute.PropertyType)
                    {
                        bindingPath = property.propertyPath,
                    };
                    ShaderParamIntField r = new ShaderParamIntField(GetPreferredLabel(property), intDropdownElement);
                    r.AddToClassList(ClassAllowDisable);
                    r.AddToClassList(ShaderParamIntField.alignedFieldUssClassName);
                    return r;
                }
                case SerializedPropertyType.String:
                {
                    ShaderParamStringElement shaderParamStringElement = new ShaderParamStringElement(shaderParamAttribute.PropertyType)
                    {
                        bindingPath = property.propertyPath,
                    };
                    ShaderParamStringField r =
                        new ShaderParamStringField(GetPreferredLabel(property), shaderParamStringElement);
                    r.AddToClassList(ClassAllowDisable);
                    r.AddToClassList(ShaderParamStringField.alignedFieldUssClassName);
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
                    return new HelpBox("", HelpBoxMessageType.Error)
                    {
                        style =
                        {
                            display = DisplayStyle.None,
                            flexGrow = 1,
                        },
                        name = HelpBoxName(property),
                    };
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

        private Shader _currentShader;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(HelpBoxName(property));
            ShaderParamAttribute shaderParamAttribute = (ShaderParamAttribute)saintsAttribute;

            IBindShader bindShader;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    ShaderParamIntField intDropdownField = container.Q<ShaderParamIntField>();
                    intDropdownField.ShaderParamIntElement.BindHelpBox(helpBox);
                    AddContextualMenuManipulator(helpBox, shaderParamAttribute, intDropdownField, property, onValueChangedCallback, info, parent);
                    bindShader = intDropdownField.ShaderParamIntElement;
                }
                    break;
                case SerializedPropertyType.String:
                {
                    ShaderParamStringField stringDropdownField = container.Q<ShaderParamStringField>();
                    stringDropdownField.ShaderParamStringElement.BindHelpBox(helpBox);
                    AddContextualMenuManipulator(helpBox, shaderParamAttribute, stringDropdownField, property, onValueChangedCallback, info, parent);
                    bindShader = stringDropdownField.ShaderParamStringElement;
                }
                    break;
                default:
                    return;
            }

            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(OnSaintsEditorApplicationChanged);
            container.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(OnSaintsEditorApplicationChanged));

            OnSaintsEditorApplicationChanged();

            return;

            void OnSaintsEditorApplicationChanged()
            {
                (string error, Shader shader) = ShaderUtils.GetShader(shaderParamAttribute.TargetName, shaderParamAttribute.Index, property, info, parent);
                ShaderUtils.UpdateHelpBox(helpBox, error);
                if (error != "")
                {
                    return;
                }

                bindShader.BindShader(shader);
            }
        }

        private void AddContextualMenuManipulator(HelpBox helpBox, ShaderParamAttribute shaderParamAttribute, VisualElement root, SerializedProperty property,
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

                (string error, Shader shader) = ShaderUtils.GetShader(shaderParamAttribute.TargetName, shaderParamAttribute.Index, property, info, parent);
                ShaderUtils.UpdateHelpBox(helpBox, error);
                if (error != "")
                {
                    return;
                }

                if (shader != _currentShader)
                {
                    _currentShader = shader;
                    if (isString)
                    {
                        root.Q<ShaderParamStringElement>().BindShader(shader);
                    }
                    else
                    {
                        root.Q<ShaderParamIntElement>().BindShader(shader);
                    }
                }

                bool canBeInt = int.TryParse(clipboardText, out int clipboardInt);

                if (isString)
                {
                    foreach (ShaderParamUtils.ShaderCustomInfo shaderCustomInfo in ShaderParamUtils.GetShaderInfo(shader, shaderParamAttribute.PropertyType))
                    {
                        if (shaderCustomInfo.PropertyName == clipboardText)
                        {
                            evt.menu.AppendAction($"Paste \"{shaderCustomInfo.PropertyName}\"", _ =>
                            {
                                property.stringValue = shaderCustomInfo.PropertyName;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, shaderCustomInfo.PropertyName);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(shaderCustomInfo.PropertyName);
                            });
                            return;
                        }
                    }
                }
                else
                {
                    foreach (ShaderParamUtils.ShaderCustomInfo shaderCustomInfo in ShaderParamUtils.GetShaderInfo(shader, shaderParamAttribute.PropertyType))
                    {
                        if (shaderCustomInfo.PropertyName == clipboardText
                            || canBeInt && shaderCustomInfo.PropertyID == clipboardInt)
                        {
                            evt.menu.AppendAction($"Paste \"{shaderCustomInfo.PropertyName}\"({shaderCustomInfo.PropertyID})", _ =>
                            {
                                property.intValue = shaderCustomInfo.PropertyID;
                                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, shaderCustomInfo.PropertyID);
                                property.serializedObject.ApplyModifiedProperties();
                                onValueChangedCallback.Invoke(shaderCustomInfo.PropertyID);
                            });
                            return;
                        }
                    }
                }
            }));
        }

        private class ShaderParamValueEditString : VisualElement
        {
            public readonly ShaderParamStringField Field;
            public readonly HelpBox HelpBox;

            public ShaderParamValueEditString(ShaderParamStringField field)
            {
                Field = field;
                Add(field);
                HelpBox = new HelpBox("", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        display = DisplayStyle.None,
                        flexGrow = 1,
                    },
                };
                Add(HelpBox);
                field.ShaderParamStringElement.BindHelpBox(HelpBox);
            }
        }

        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, ShaderParamAttribute shaderParamAttribute, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            (string error, Shader shader) = ShaderUtils.GetShaderForShowInInspector(
                value,
                shaderParamAttribute.TargetName,
                shaderParamAttribute.Index,
                targets[0]);

            if (oldElement is ShaderParamValueEditString oldContainer)
            {
                oldContainer.Field.ShaderParamStringElement.value = value;
                oldContainer.Field.ShaderParamStringElement.BindShader(shader);
                ShaderUtils.UpdateHelpBox(oldContainer.HelpBox, error);
                return null;
            }

            ShaderParamStringElement visualInput = new ShaderParamStringElement(shaderParamAttribute.PropertyType)
            {
                value = value,
            };
            ShaderParamStringField field =
                new ShaderParamStringField(label, visualInput)
                {
                    value = value,
                };
            ShaderParamValueEditString element = new ShaderParamValueEditString(field);
            visualInput.BindShader(shader);
            ShaderUtils.UpdateHelpBox(element.HelpBox, error);

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
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

        private class ShaderParamValueEditInt : VisualElement
        {
            public readonly ShaderParamIntField Field;
            public readonly HelpBox HelpBox;

            public ShaderParamValueEditInt(ShaderParamIntField field)
            {
                Field = field;
                Add(field);
                HelpBox = new HelpBox("", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        display = DisplayStyle.None,
                        flexGrow = 1,
                    },
                };
                Add(HelpBox);
                field.ShaderParamIntElement.BindHelpBox(HelpBox);
            }
        }

        public static VisualElement UIToolkitValueEditInt(VisualElement oldElement, ShaderParamAttribute shaderParamAttribute, string label, int value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            (string error, Shader shader) = ShaderUtils.GetShaderForShowInInspector(
                value,
                shaderParamAttribute.TargetName,
                shaderParamAttribute.Index,
                targets[0]);

            if (oldElement is ShaderParamValueEditInt oldContainer)
            {
                oldContainer.Field.ShaderParamIntElement.value = value;
                oldContainer.Field.ShaderParamIntElement.BindShader(shader);
                ShaderUtils.UpdateHelpBox(oldContainer.HelpBox, error);
                return null;
            }

            ShaderParamIntElement visualInput = new ShaderParamIntElement(shaderParamAttribute.PropertyType)
            {
                value = value,
            };
            ShaderParamIntField field =
                new ShaderParamIntField(label, visualInput)
                {
                    value = value,
                };
            ShaderParamValueEditInt element = new ShaderParamValueEditInt(field);
            visualInput.BindShader(shader);
            ShaderUtils.UpdateHelpBox(element.HelpBox, error);

            UIToolkitUtils.UIToolkitValueEditAfterProcess(field, setterOrNull,
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
