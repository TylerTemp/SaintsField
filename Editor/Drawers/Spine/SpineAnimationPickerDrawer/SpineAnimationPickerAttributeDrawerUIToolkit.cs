#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.ShaderDrawers;
using SaintsField.Editor.Utils;
using SaintsField.Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Spine.SpineAnimationPickerDrawer
{
    public partial class SpineAnimationPickerAttributeDrawer
    {
        private static string DropdownButtonName(SerializedProperty property) => $"{property.propertyPath}__SpineAnimationPicker_DropdownButton";
        private static string HelpBoxName(SerializedProperty property) => $"{property.propertyPath}__SpineAnimationPicker_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
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

            SpineAnimationPickerAttribute spineAnimationPicker = (SpineAnimationPickerAttribute) saintsAttribute;

            UpdateDisplay(container, spineAnimationPicker, property, info, parent);

            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(DropdownButtonName(property));
            dropdownButton.ButtonElement.clicked += () =>
            {
                (string error, Shader shader) = ShaderUtils.GetShader(spineAnimationPicker.TargetName, spineAnimationPicker.Index, property, info, parent);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    UpdateDisplay(container, spineAnimationPicker, property, info, parent);
                    return;
                }

                ShaderInfo[] shaderInfos = GetShaderInfo(shader, spineAnimationPicker.PropertyType).ToArray();
                (bool foundShaderInfo, ShaderInfo selectedShaderInfo) = GetSelectedShaderInfo(property, shaderInfos);
                AdvancedDropdownMetaInfo dropdownMetaInfo = GetMetaInfo(foundShaderInfo, selectedShaderInfo, shaderInfos, false);

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
                        ShaderInfo shaderInfo = (ShaderInfo) curItem;
                        // ReSharper disable once ConvertIfStatementToSwitchStatement
                        if (property.propertyType == SerializedPropertyType.String)
                        {
                            property.stringValue = shaderInfo.PropertyName;
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback(shaderInfo.PropertyName);
                        }
                        else if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            property.intValue = shaderInfo.PropertyID;
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback(shaderInfo.PropertyID);
                        }
                    }
                ));
            };
        }

        private static void UpdateDisplay(VisualElement container, SpineAnimationPickerAttribute spineAnimationPickerAttribute, SerializedProperty property, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(DropdownButtonName(property));
            HelpBox helpBox = container.Q<HelpBox>(HelpBoxName(property));

            (string error, SkeletonRenderer skeletonRenderer) = SpineUtils.GetSkeletonRenderer(spineAnimationPickerAttribute.SkeletonTarget, property, info, parent);
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

            var animations = skeletonRenderer

            (bool foundShaderInfo, ShaderInfo selectedShaderInfo) = GetSelectedShaderInfo(property, GetShaderInfo(skeletonRenderer, spineAnimationPickerAttribute.PropertyType));

            if(!foundShaderInfo)
            {
                // dropdownButton.SetEnabled(true);
                string notFoundError;
                if (property.propertyType == SerializedPropertyType.String)
                {
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

                    notFoundError = $"{stringValue} not found in shader";
                }
                else
                {
                    notFoundError = $"{property.intValue} not found in shader";
                }
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

            // dropdownButton.SetEnabled(true);
            string label = selectedShaderInfo.ToString();
            if (dropdownButton.ButtonLabelElement.text != label)
            {
                dropdownButton.ButtonLabelElement.text = label;
            }
        }
    }
}
#endif
