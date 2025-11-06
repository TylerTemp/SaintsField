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

            ShaderKeywordElement shaderKeywordElement = new ShaderKeywordElement
            {
                bindingPath = property.propertyPath,
            };
            ShaderKeywordField r = new ShaderKeywordField(GetPreferredLabel(property), shaderKeywordElement);
            r.AddToClassList(ClassAllowDisable);
            r.AddToClassList(ShaderKeywordField.alignedFieldUssClassName);
            return r;
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

            ShaderKeywordField shaderKeywordField = container.Q<ShaderKeywordField>();
            AddContextualMenuManipulator(helpBox, shaderKeywordAttribute, shaderKeywordField, property, onValueChangedCallback, info, parent);

            RefreshShader();

            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(RefreshShader);
            container.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(RefreshShader));

            return;

            void RefreshShader()
            {
                (string error, Shader shader) = ShaderUtils.GetShader(shaderKeywordAttribute.TargetName, shaderKeywordAttribute.Index, property, info, parent);
                if (error != "")
                {
                    ShaderUtils.UpdateHelpBox(helpBox, error);
                }
                else
                {
                    shaderKeywordField.ShaderKeywordElement.BindShader(shader);
                }
            }
        }

        private static void AddContextualMenuManipulator(HelpBox helpBox, ShaderKeywordAttribute shaderKeywordAttribute, VisualElement root, SerializedProperty property, Action<object> onValueChangedCallback, FieldInfo info, object parent)
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
                ShaderUtils.UpdateHelpBox(helpBox, error);
                if (error != "")
                {
                    return;
                }

                root.Q<ShaderKeywordElement>().BindShader(shader);

                foreach (string shaderKeyword in ShaderKeywordUtils.GetShaderKeywords(shader))
                {
                    // ReSharper disable once InvertIf
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

        private class ShaderKeywordValueEdit : VisualElement
        {
            public readonly ShaderKeywordField Field;
            public readonly HelpBox HelpBox;

            public ShaderKeywordValueEdit(ShaderKeywordField field)
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
                field.ShaderKeywordElement.BindHelpBox(HelpBox);
            }
        }

        public static VisualElement UIToolkitValueEditString(VisualElement oldElement, ShaderKeywordAttribute shaderKeywordAttribute, string label, string value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets)
        {
            (string error, Shader shader) = ShaderUtils.GetShaderForShowInInspector(
                value,
                shaderKeywordAttribute.TargetName,
                shaderKeywordAttribute.Index,
                targets[0]);

            if (oldElement is ShaderKeywordValueEdit oldContainer)
            {
                oldContainer.Field.ShaderKeywordElement.value = value;
                oldContainer.Field.ShaderKeywordElement.BindShader(shader);
                ShaderUtils.UpdateHelpBox(oldContainer.HelpBox, error);
                return null;
            }

            ShaderKeywordElement visualInput = new ShaderKeywordElement()
            {
                value = value,
            };
            ShaderKeywordField field =
                new ShaderKeywordField(label, visualInput)
                {
                    value = value,
                };
            ShaderKeywordValueEdit element = new ShaderKeywordValueEdit(field);
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
