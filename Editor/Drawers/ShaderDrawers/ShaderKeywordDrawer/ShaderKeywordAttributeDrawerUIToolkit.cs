#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderKeywordDrawer
{
    public partial class ShaderKeywordAttributeDrawer
    {
        // private static string DropdownButtonName(SerializedProperty property) => $"{property.propertyPath}__ShaderKeyword_DropdownButton";
        private static string HelpBoxName(SerializedProperty property) => $"{property.propertyPath}__ShaderKeyword_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new VisualElement();
            }

            ShaderKeywordElement layerStringStringDropdown = new ShaderKeywordElement();
            layerStringStringDropdown.BindProperty(property);
            return new StringDropdownField(GetPreferredLabel(property), layerStringStringDropdown);

        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new HelpBox($"Type {property.propertyType} is not a string", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                };
            }

            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                },
                name = HelpBoxName(property),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return;
            }

            HelpBox helpBox = container.Q<HelpBox>(HelpBoxName(property));

            ShaderKeywordAttribute shaderKeywordAttribute = (ShaderKeywordAttribute)saintsAttribute;

            StringDropdownField stringDropdownField = container.Q<StringDropdownField>();
            AddContextualMenuManipulator(helpBox, shaderKeywordAttribute, stringDropdownField, property, onValueChangedCallback, info, parent);

            stringDropdownField.Button.clicked += () => MakeDropdown(property, shaderKeywordAttribute, helpBox,stringDropdownField, onValueChangedCallback, info, parent);
        }

        private void MakeDropdown(SerializedProperty property, ShaderKeywordAttribute shaderKeywordAttribute, HelpBox helpBox, StringDropdownField root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            (string error, Shader shader) = ShaderUtils.GetShader(shaderKeywordAttribute.TargetName, shaderKeywordAttribute.Index, property, info, parent);
            UpdateHelpBox(helpBox, error);
            if (error != "")
            {
                return;
            }

            if (shader != _currentShader)
            {
                _currentShader = shader;
                root.Q<ShaderKeywordElement>().BindShader(shader);
            }

            AdvancedDropdownList<string> dropdown = new AdvancedDropdownList<string>();
            dropdown.Add("[Empty String]", string.Empty);
            dropdown.AddSeparator();

            string selected = null;
            foreach (string shaderKeyword in ShaderKeywordUtils.GetShaderKeywords(_currentShader))
            {
                // dropdown.Add(path, (path, index));
                dropdown.Add(shaderKeyword, shaderKeyword);
                // ReSharper disable once InvertIf
                if (shaderKeyword == property.stringValue )
                {
                    selected = shaderKeyword;
                }
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selected is null ? Array.Empty<object>(): new object[] { selected },
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
                    string shaderKeyword = (string)curItem;
                    property.stringValue = shaderKeyword;
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, shaderKeyword);
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(shaderKeyword);
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }

        private Shader _currentShader;

        private void AddContextualMenuManipulator(HelpBox helpBox, ShaderKeywordAttribute shaderKeywordAttribute, StringDropdownField root, SerializedProperty property, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.AddContextualMenuManipulator(root, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            root.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                string clipboardText = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrEmpty(clipboardText))
                {
                    return;
                }

                (string error, Shader shader) = ShaderUtils.GetShader(shaderKeywordAttribute.TargetName, shaderKeywordAttribute.Index, property, info, parent);
                UpdateHelpBox(helpBox, error);
                if (error != "")
                {
                    return;
                }

                if (shader != _currentShader)
                {
                    _currentShader = shader;
                    root.Q<ShaderKeywordElement>().BindShader(shader);
                }

                foreach (string shaderKeyword in ShaderKeywordUtils.GetShaderKeywords(_currentShader))
                {
                    if (shaderKeyword == clipboardText)
                    {
                        evt.menu.AppendAction($"Paste \"{shaderKeyword}\"", _ =>
                        {
                            property.stringValue = shaderKeyword;
                            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, shaderKeyword);
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback.Invoke(shaderKeyword);
                        });
                        return;
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

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return;
            }
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                return;
            }

            ShaderKeywordAttribute shaderKeywordAttribute = (ShaderKeywordAttribute)saintsAttribute;
            HelpBox helpBox = container.Q<HelpBox>(HelpBoxName(property));

            (string error, Shader shader) = ShaderUtils.GetShader(shaderKeywordAttribute.TargetName,
                shaderKeywordAttribute.Index, property, info, parent);
            UpdateHelpBox(helpBox, error);
            if (error != "")
            {
                return;
            }

            // ReSharper disable once InvertIf
            if (shader != _currentShader)
            {
                _currentShader = shader;
                container.Q<ShaderKeywordElement>().BindShader(shader);
            }
        }
    }
}
#endif
