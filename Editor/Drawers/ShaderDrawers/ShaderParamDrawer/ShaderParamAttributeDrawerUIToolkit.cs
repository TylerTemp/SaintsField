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
                    ShaderParamIntElement intDropdownElement = new ShaderParamIntElement(shaderParamAttribute.PropertyType);
                    intDropdownElement.BindProperty(property);
                    return new IntDropdownField(GetPreferredLabel(property), intDropdownElement);
                }
                case SerializedPropertyType.String:
                {
                    ShaderParamStringElement shaderParamStringElement = new ShaderParamStringElement(shaderParamAttribute.PropertyType);
                    shaderParamStringElement.BindProperty(property);
                    return new StringDropdownField(GetPreferredLabel(property), shaderParamStringElement);
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

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    IntDropdownField intDropdownField = container.Q<IntDropdownField>();
                    AddContextualMenuManipulator(helpBox, shaderParamAttribute, intDropdownField, property, onValueChangedCallback, info, parent);

                    intDropdownField.Button.clicked += () => MakeDropdown(property, shaderParamAttribute, helpBox, intDropdownField, onValueChangedCallback, info, parent);
                }
                    break;
                case SerializedPropertyType.String:
                {
                    StringDropdownField stringDropdownField = container.Q<StringDropdownField>();
                    AddContextualMenuManipulator(helpBox, shaderParamAttribute, stringDropdownField, property, onValueChangedCallback, info, parent);

                    stringDropdownField.Button.clicked += () => MakeDropdown(property, shaderParamAttribute, helpBox,stringDropdownField, onValueChangedCallback, info, parent);
                }
                    break;
                default:
                    return;
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
                UpdateHelpBox(helpBox, error);
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

        private static void UpdateHelpBox(HelpBox helpBox, string error)
        {
            if (helpBox.text == error)
            {
                return;
            }

            if (string.IsNullOrEmpty(error))
            {
                helpBox.style.display = DisplayStyle.None;
                helpBox.text = "";
            }
            else
            {
                helpBox.text = error;
                helpBox.style.display = DisplayStyle.Flex;
            }
        }

        private void MakeDropdown(SerializedProperty property, ShaderParamAttribute shaderParamAttribute, HelpBox helpBox, VisualElement root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            (string error, Shader shader) = ShaderUtils.GetShader(shaderParamAttribute.TargetName, shaderParamAttribute.Index, property, info, parent);
            UpdateHelpBox(helpBox, error);
            if (error != "")
            {
                return;
            }

            bool isString = property.propertyType == SerializedPropertyType.String;

            if (_currentShader != shader)
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

            AdvancedDropdownList<ShaderParamUtils.ShaderCustomInfo> dropdown = new AdvancedDropdownList<ShaderParamUtils.ShaderCustomInfo>();
            if (isString)
            {
                dropdown.Add("[Empty String]", new ShaderParamUtils.ShaderCustomInfo("", "", default, -1));
                dropdown.AddSeparator();
            }

            bool selected = false;
            ShaderParamUtils.ShaderCustomInfo selectedInfo = default;
            foreach (ShaderParamUtils.ShaderCustomInfo shaderCustomInfo in ShaderParamUtils.GetShaderInfo(shader, shaderParamAttribute.PropertyType))
            {
                // dropdown.Add(path, (path, index));
                dropdown.Add(shaderCustomInfo.GetString(false), shaderCustomInfo);
                // ReSharper disable once InvertIf
                if (isString && shaderCustomInfo.PropertyName == property.stringValue
                    || !isString && shaderCustomInfo.PropertyID == property.intValue)
                {
                    selected = true;
                    selectedInfo = shaderCustomInfo;
                }
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selected ? new object[] { selectedInfo } : Array.Empty<object>(),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (_, curItem) =>
                {
                    ShaderParamUtils.ShaderCustomInfo shaderCustomInfo = (ShaderParamUtils.ShaderCustomInfo)curItem;
                    if (isString)
                    {
                        property.stringValue = shaderCustomInfo.PropertyName;
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, shaderCustomInfo.PropertyName);
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback.Invoke(shaderCustomInfo.PropertyName);
                    }
                    else
                    {
                        property.intValue = shaderCustomInfo.PropertyID;
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, shaderCustomInfo.PropertyID);
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback.Invoke(shaderCustomInfo.PropertyID);
                    }
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                return;
            }

            bool isString = property.propertyType == SerializedPropertyType.String;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.String:
                {
                    ShaderParamAttribute shaderParamAttribute = (ShaderParamAttribute)saintsAttribute;
                    HelpBox helpBox = container.Q<HelpBox>(HelpBoxName(property));

                    (string error, Shader shader) = ShaderUtils.GetShader(shaderParamAttribute.TargetName, shaderParamAttribute.Index, property, info, parent);
                    UpdateHelpBox(helpBox, error);
                    if (error != "")
                    {
                        return;
                    }

                    if (_currentShader != shader)
                    {
                        _currentShader = shader;
                        if (isString) {
                            container.Q<ShaderParamStringElement>().BindShader(shader);
                        }
                        else
                        {
                            container.Q<ShaderParamIntElement>().BindShader(shader);
                        }
                    }
                }
                    break;
                default:
                    return;
            }


        }
    }
}
#endif
