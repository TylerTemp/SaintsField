#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ShaderDrawers.ShaderKeywordDrawer
{
    public partial class ShaderKeywordAttributeDrawer
    {
        private static string DropdownButtonName(SerializedProperty property) => $"{property.propertyPath}__ShaderKeyword_DropdownButton";
        private static string HelpBoxName(SerializedProperty property) => $"{property.propertyPath}__ShaderKeyword_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            dropdownButton.name = DropdownButtonName(property);
            dropdownButton.AddToClassList(ClassAllowDisable);
            return dropdownButton;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
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
            HelpBox helpBox = container.Q<HelpBox>(HelpBoxName(property));

            string typeMismatchError = GetTypeMismatchError(property);
            if (typeMismatchError != "")
            {
                if(helpBox.text != typeMismatchError)
                {
                    helpBox.text = typeMismatchError;
                    helpBox.style.display = DisplayStyle.Flex;
                }
                return;
            }

            ShaderKeywordAttribute shaderKeywordAttribute = (ShaderKeywordAttribute) saintsAttribute;

            UpdateDisplay(container, shaderKeywordAttribute, property, info, parent);

            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(DropdownButtonName(property));
            UIToolkitUtils.AddContextualMenuManipulator(dropdownButton.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));
            dropdownButton.ButtonElement.clicked += () =>
            {
                (string error, Shader shader) = ShaderUtils.GetShader(shaderKeywordAttribute.TargetName, shaderKeywordAttribute.Index, property, info, parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    UpdateDisplay(container, shaderKeywordAttribute, property, info, parent);
                    return;
                }

                string[] shaderKeywords = GetShaderKeywords(shader).ToArray();
                string selectedShaderKeyword = property.stringValue;
                int selectedIndex = Array.IndexOf(shaderKeywords, selectedShaderKeyword);

                AdvancedDropdownMetaInfo dropdownMetaInfo = GetMetaInfo(selectedIndex, shaderKeywords, false);

                float maxHeight = Screen.currentResolution.height - dropdownButton.worldBound.y - dropdownButton.worldBound.height - 100;
                Rect worldBound = dropdownButton.worldBound;
                if (maxHeight < 100)
                {
                    worldBound.y -= 100 + worldBound.height;
                    maxHeight = 100;
                }

                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    dropdownMetaInfo,
                    dropdownButton.worldBound.width,
                    maxHeight,
                    false,
                    (_, curItem) =>
                    {
                        string shaderKeyword = (string) curItem;
                        property.stringValue = shaderKeyword;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback(shaderKeyword);
                    }
                ));
            };
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            UpdateDisplay(container, (ShaderKeywordAttribute) saintsAttribute, property, info, parent);
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, string labelOrNull, IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried,
            RichTextDrawer richTextDrawer)
        {
            UIToolkitUtils.DropdownButtonField dropdownField = container.Q<UIToolkitUtils.DropdownButtonField>(DropdownButtonName(property));
            UIToolkitUtils.SetLabel(dropdownField.labelElement, richTextChunks, richTextDrawer);
        }

        private static void UpdateDisplay(VisualElement container, ShaderKeywordAttribute shaderKeywordAttribute, SerializedProperty property, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(DropdownButtonName(property));
            HelpBox helpBox = container.Q<HelpBox>(HelpBoxName(property));

            string useLabel = string.IsNullOrEmpty(property.stringValue)? "-": property.stringValue;
            if (dropdownButton.ButtonLabelElement.text != useLabel)
            {
                dropdownButton.ButtonLabelElement.text = useLabel;
            }

            (string error, Shader shader) = ShaderUtils.GetShader(shaderKeywordAttribute.TargetName, shaderKeywordAttribute.Index, property, info, parent);
            if (error != "")
            {
                // dropdownButton.SetEnabled(false);

                // ReSharper disable once InvertIf
                if(helpBox.text != error)
                {
                    helpBox.text = error;
                    helpBox.style.display = DisplayStyle.Flex;
                }

                return;
            }

            string selectedShaderKeyword = property.stringValue;
            string[] shaderKeywords = GetShaderKeywords(shader).ToArray();
            int selectedIndex = Array.IndexOf(shaderKeywords, selectedShaderKeyword);

            if(selectedIndex == -1)
            {
                // dropdownButton.SetEnabled(true);
                string stringValue = property.stringValue;
                if (string.IsNullOrEmpty(stringValue))
                {
                    // ReSharper disable once InvertIf
                    if(helpBox.style.display != DisplayStyle.None)
                    {
                        helpBox.text = "";
                        helpBox.style.display = DisplayStyle.None;
                    }
                    return;
                }

                string notFoundError = $"{stringValue} not found in shader";
                // ReSharper disable once InvertIf
                if(helpBox.text != notFoundError)
                {
                    helpBox.text = notFoundError;
                    helpBox.style.display = DisplayStyle.Flex;
                }
                return;
            }

            if(helpBox.text != "")
            {
                helpBox.text = "";
                helpBox.style.display = DisplayStyle.None;
            }
        }
    }
}
#endif
