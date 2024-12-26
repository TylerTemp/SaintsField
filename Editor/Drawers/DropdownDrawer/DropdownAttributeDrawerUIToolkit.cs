#if UNITY_2021_3_OR_NEWER
using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.DropdownDrawer
{
    public partial class DropdownAttributeDrawer
    {
        #region UIToolkit

        private static string NameDropdownButtonField(SerializedProperty property) => $"{property.propertyPath}__Dropdown_Button";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__Dropdown_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, FieldInfo info, object parent)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;
            MetaInfo metaInfo = GetMetaInfo(property, dropdownAttribute, info, parent);

            string buttonLabel = metaInfo.SelectedIndex == -1? "-": metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
            dropdownButton.style.flexGrow = 1;
            dropdownButton.ButtonLabelElement.text = buttonLabel;
            dropdownButton.name = NameDropdownButtonField(property);
            dropdownButton.userData = metaInfo.SelectedIndex == -1
                ? null
                : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item2;

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

            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute)saintsAttribute;
            container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButtonField(property)).ButtonElement.clicked += () =>
                ShowDropdown(property, saintsAttribute, container, dropdownAttribute.SlashAsSub, info, parent, onValueChangedCallback);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly");
                return;
            }

            Type parentType = parent.GetType();
            FieldInfo field = parentType.GetField(property.name, BindAttr);
            if (field == null)
            {
                return;
            }

            object newValue = field.GetValue(parent);
            object curValue = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButtonField(property)).userData;
            if (Util.GetIsEqual(curValue, newValue))
            {
                return;
            }

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, field, parent);
            string display = metaInfo.SelectedIndex == -1 ? "-" : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;
            // Debug.Log($"change label to {display}");
            Label buttonLabelElement = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButtonField(property)).ButtonLabelElement;
            if(buttonLabelElement.text != display)
            {
                buttonLabelElement.text = display;
            }
        }

        private static void ShowDropdown(SerializedProperty property, ISaintsAttribute saintsAttribute,
            VisualElement container, bool slashAsSub, FieldInfo info, object parent, Action<object> onChange)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;
            MetaInfo metaInfo = GetMetaInfo(property, dropdownAttribute, info, parent);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
            if(helpBox.text != metaInfo.Error)
            {
                helpBox.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = metaInfo.Error;
            }

            if (metaInfo.Error != "")
            {
                return;
            }

            // Button button = container.Q<Button>(NameButtonField(property));
            UIToolkitUtils.DropdownButtonField dropdownButtonField = container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownButtonField(property));

            if (slashAsSub)
            {
                string curDisplay = metaInfo.SelectedIndex == -1 ? "-" : metaInfo.DropdownListValue[metaInfo.SelectedIndex].Item1;
                ShowGenericMenu(metaInfo, curDisplay, dropdownButtonField.ButtonElement.worldBound, (newName, item) =>
                {
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, item);
                    Util.SignPropertyValue(property, info, parent, item);
                    property.serializedObject.ApplyModifiedProperties();
                    onChange(item);
                    dropdownButtonField.ButtonLabelElement.text = newName;
                    // property.serializedObject.ApplyModifiedProperties();
                }, false);
            }
            else
            {
                GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
                // int selectedIndex = (int)buttonLabel.userData;
                // FIXED: can not get correct index
                int selectedIndex = metaInfo.SelectedIndex;
                // Debug.Log($"metaInfo.SelectedIndex={metaInfo.SelectedIndex}");
                foreach (int index in Enumerable.Range(0, metaInfo.DropdownListValue.Count))
                {
                    // int curIndex = index;
                    (string curName, object curItem, bool disabled, bool curIsSeparator) =
                        metaInfo.DropdownListValue[index];
                    if (curIsSeparator)
                    {
                        genericDropdownMenu.AddSeparator(curName);
                    }
                    else if (disabled)
                    {
                        // Debug.Log($"disabled: {curName}");
                        genericDropdownMenu.AddDisabledItem(curName, index == selectedIndex);
                    }
                    else
                    {
                        genericDropdownMenu.AddItem(curName, index == selectedIndex, () =>
                        {
                            ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, curItem);
                            Util.SignPropertyValue(property, info, parent, curItem);
                            property.serializedObject.ApplyModifiedProperties();
                            onChange(curItem);
                            dropdownButtonField.ButtonLabelElement.text = curName;
                            // property.serializedObject.ApplyModifiedProperties();
                        });
                    }
                }

                genericDropdownMenu.DropDown(dropdownButtonField.ButtonElement.worldBound, dropdownButtonField, true);
            }
        }

        #endregion
    }
}

#endif
