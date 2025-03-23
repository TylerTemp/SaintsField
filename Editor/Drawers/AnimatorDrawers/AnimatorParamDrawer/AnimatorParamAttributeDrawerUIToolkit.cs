#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AnimatorDrawers.AnimatorParamDrawer
{
    public partial class AnimatorParamAttributeDrawer
    {
        private static string NameDropdownField(SerializedProperty property) =>
            $"{property.propertyPath}__AnimatorParam_DropdownField";

        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__AnimatorParam_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            FieldInfo info, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            UIToolkitUtils.DropdownButtonField dropdownButton =
                UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            dropdownButton.name = NameDropdownField(property);
            dropdownButton.userData = metaInfo;

            dropdownButton.AddToClassList(ClassAllowDisable);
            return dropdownButton;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBoxElement = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 0,
                },
                name = NameHelpBox(property),
            };

            helpBoxElement.AddToClassList(ClassAllowDisable);
            return helpBoxElement;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownField =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));
            dropdownField.ButtonElement.clicked += () =>
                ShowDropdown(property, saintsAttribute, container, info, parent, onValueChangedCallback);
        }

        private static void ShowDropdown(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent, Action<object> onValueChangedCallback)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            UIToolkitUtils.DropdownButtonField dropdownField =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));

            bool isString = property.propertyType == SerializedPropertyType.String;
            int selectedIndex = isString
                ? Util.ListIndexOfAction(metaInfo.AnimatorParameters, eachName => ParamNameEquals(eachName, property))
                : Util.ListIndexOfAction(metaInfo.AnimatorParameters, eachHash => ParamHashEquals(eachHash, property));

            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
            foreach ((AnimatorControllerParameter value, int index) in metaInfo.AnimatorParameters.WithIndex())
            {
                AnimatorControllerParameter curItem = value;
                string curName = GetParameterLabel(curItem);

                genericDropdownMenu.AddItem(curName, index == selectedIndex, () =>
                {
                    if (isString)
                    {
                        property.stringValue = curItem.name;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback(curItem.name);
                    }
                    else
                    {
                        property.intValue = curItem.nameHash;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback(curItem.nameHash);
                    }

                    dropdownField.ButtonLabelElement.text = curName;
                });
            }

            if (metaInfo.Animator != null)
            {
                if (metaInfo.AnimatorParameters.Count > 0)
                {
                    genericDropdownMenu.AddSeparator("");
                }

                genericDropdownMenu.AddItem($"Edit {metaInfo.Animator.runtimeAnimatorController.name}...", false,
                    () => OpenAnimator(metaInfo.Animator.runtimeAnimatorController));
            }

            genericDropdownMenu.DropDown(dropdownField.ButtonElement.worldBound, dropdownField.ButtonElement, true);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);

            UIToolkitUtils.DropdownButtonField dropdownField =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));
            HelpBox helpBoxElement = container.Q<HelpBox>(NameHelpBox(property));
            if (helpBoxElement.text != metaInfo.Error)
            {
                helpBoxElement.text = metaInfo.Error;
                helpBoxElement.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }

            string label;
            if (property.propertyType == SerializedPropertyType.String)
            {
                if (string.IsNullOrEmpty(property.stringValue))
                {
                    label = "-";
                }
                else
                {
                    label = $"{property.stringValue} [?]";
                    foreach (AnimatorControllerParameter animatorControllerParameter in metaInfo.AnimatorParameters)
                    {
                        // ReSharper disable once InvertIf
                        if (ParamNameEquals(animatorControllerParameter, property))
                        {
                            label = GetParameterLabel(animatorControllerParameter);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (property.intValue == 0)
                {
                    label = "-";
                }
                else
                {
                    label = $"{property.intValue} [?]";
                    foreach (AnimatorControllerParameter animatorControllerParameter in metaInfo.AnimatorParameters)
                    {
                        // ReSharper disable once InvertIf
                        if (ParamHashEquals(animatorControllerParameter, property))
                        {
                            label = GetParameterLabel(animatorControllerParameter);
                            break;
                        }
                    }
                }
            }

            if (dropdownField.ButtonLabelElement.text != label)
            {
                dropdownField.ButtonLabelElement.text = label;
            }
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        {
            UIToolkitUtils.DropdownButtonField dropdownField =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));
            UIToolkitUtils.SetLabel(dropdownField.labelElement, richTextChunks, richTextDrawer);
        }

        private static string GetParameterLabel(AnimatorControllerParameter each) => $"{each.name} [{each.type}]";
    }
}
#endif
