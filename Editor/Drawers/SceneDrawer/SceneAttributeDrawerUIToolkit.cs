#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SceneDrawer
{
    public partial class SceneAttributeDrawer
    {
        private static string NameButtonField(SerializedProperty property) => $"{property.propertyPath}__Scene_Button";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__Scene_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton =
                UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = NameButtonField(property);

            dropdownButton.AddToClassList(ClassAllowDisable);

            return dropdownButton;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField buttonLabel =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property));

            UIToolkitUtils.AddContextualMenuManipulator(buttonLabel.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            (int _, string displayName) = GetSelected(property, (SceneAttribute)saintsAttribute);
            buttonLabel.ButtonLabelElement.text = displayName;

            container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property)).ButtonElement.clicked += () =>
                ShowDropdown(property, saintsAttribute, container, parent, onValueChangedCallback);
        }

        private static void ShowDropdown(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, object parent, Action<object> onChange)
        {
            string[] scenes = GetTrimedScenePath(((SceneAttribute)saintsAttribute).FullPath);

            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
            UIToolkitUtils.DropdownButtonField buttonDropdown =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameButtonField(property));

            (int selectedIndex, string _) = GetSelected(property, (SceneAttribute)saintsAttribute);

            for (var index = 0; index < scenes.Length; index++)
            {
                int curIndex = index;
                string curItem = scenes[index];
                string curName = $"{index}: {curItem}";

                genericDropdownMenu.AddItem(curName, index == selectedIndex, () =>
                {
                    if (property.propertyType == SerializedPropertyType.String)
                    {
                        property.stringValue = curItem;
                        property.serializedObject.ApplyModifiedProperties();
                        onChange.Invoke(curItem);
                    }
                    else
                    {
                        property.intValue = curIndex;
                        property.serializedObject.ApplyModifiedProperties();
                        onChange.Invoke(curIndex);
                    }

                    buttonDropdown.ButtonLabelElement.text = curName;
                });
            }

            if (scenes.Length > 0)
            {
                genericDropdownMenu.AddSeparator("");
            }

            genericDropdownMenu.AddItem("Edit Scenes In Build...", false, OpenBuildSettings);

            genericDropdownMenu.DropDown(buttonDropdown.ButtonElement.worldBound, buttonDropdown, true);
        }

        private static (int index, string displayName) GetSelected(SerializedProperty property, SceneAttribute sceneAttribute)
        {
            string[] scenes = GetTrimedScenePath(sceneAttribute.FullPath);
            if (property.propertyType == SerializedPropertyType.String)
            {
                string scene = property.stringValue;
                int index = Array.IndexOf(scenes, scene);
                return (index, index == -1 ? scene : $"{index}: {scene}");
            }
            else
            {
                int index = property.intValue;
                if (index >= scenes.Length)
                {
                    return (-1, $"{index}: ?");
                }

                string scene = scenes[index];
                return (index, $"{index}: {scene}");
            }
        }
    }
}
#endif
