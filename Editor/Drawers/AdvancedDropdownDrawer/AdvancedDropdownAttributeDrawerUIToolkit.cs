#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.AdvancedDropdownDrawer
{
    public partial class AdvancedDropdownAttributeDrawer
    {
        // private static string NameContainer(SerializedProperty property) => $"{property.propertyPath}__AdvancedDropdown";
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__AdvancedDropdown_Button";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AdvancedDropdown_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container,
            FieldInfo info,
            object parent)
        {
            AdvancedDropdownMetaInfo initMetaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent, false);

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = NameButton(property);
            dropdownButton.userData = initMetaInfo.CurValues;
            dropdownButton.ButtonLabelElement.text = GetMetaStackDisplay(initMetaInfo);

            dropdownButton.AddToClassList(ClassAllowDisable);

            return dropdownButton;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };

            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property));

            UIToolkitUtils.AddContextualMenuManipulator(dropdownButton.labelElement, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            VisualElement root = container.Q<VisualElement>(NameLabelFieldUIToolkit(property));
            dropdownButton.ButtonElement.clicked += () =>
            {
                AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent, false);
                // Debug.Log(root.worldBound);
                // Debug.Log(Screen.height);
                // float maxHeight = Mathf.Max(400, Screen.height - root.worldBound.y - root.worldBound.height - 200);

                // float maxHeight = Screen.height - root.worldBound.y - root.worldBound.height - 100;
                float maxHeight = Screen.currentResolution.height - root.worldBound.y - root.worldBound.height - 100;
                Rect worldBound = root.worldBound;
                // Debug.Log(worldBound);
                if (maxHeight < 100)
                {
                    // worldBound.x -= 400;
                    worldBound.y -= 100 + worldBound.height;
                    // Debug.Log(worldBound);
                    maxHeight = 100;
                }

                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    metaInfo,
                    root.worldBound.width,
                    maxHeight,
                    false,
                    (newDisplay, curItem) =>
                    {
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, curItem);
                        Util.SignPropertyValue(property, info, parent, curItem);
                        property.serializedObject.ApplyModifiedProperties();

                        dropdownButton.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property)).ButtonLabelElement.text = newDisplay;
                        dropdownButton.userData = curItem;
                        onValueChangedCallback(curItem);
                        // dropdownButton.buttonLabelElement.text = newDisplay;
                    }
                ));

                string curError = metaInfo.Error;
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
                // ReSharper disable once InvertIf
                if (helpBox.text != curError)
                {
                    helpBox.text = curError;
                    helpBox.style.display = curError == ""? DisplayStyle.None : DisplayStyle.Flex;
                }
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property));
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                return;
            }

            string display =
                GetMetaStackDisplay(GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent, false));
            if(dropdownButton.ButtonLabelElement.text != display)
            {
                dropdownButton.ButtonLabelElement.text = display;
            }
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property));
            string display =
                GetMetaStackDisplay(GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent, false));
            if(dropdownButton.ButtonLabelElement.text != display)
            {
                dropdownButton.ButtonLabelElement.text = display;
            }
        }
    }
}
#endif
