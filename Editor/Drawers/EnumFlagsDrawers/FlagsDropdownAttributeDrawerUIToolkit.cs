#if UNITY_2021_3_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
{
    public partial class FlagsDropdownAttributeDrawer
    {
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__FlagsDropdown";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__FlagsDropdown_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container,
            FieldInfo info,
            object parent)
        {
            EnumFlagsMetaInfo metaInfo = EnumFlagsUtil.GetMetaInfo(info);

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(property.displayName);
            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = NameButton(property);
            dropdownButton.userData = metaInfo;
            dropdownButton.ButtonLabelElement.text = GetSelectedNames(metaInfo.BitValueToName, property.intValue);

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
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property));
            VisualElement root = container.Q<VisualElement>(NameLabelFieldUIToolkit(property));
            EnumFlagsMetaInfo metaInfo = (EnumFlagsMetaInfo)dropdownButton.userData;

            dropdownButton.ButtonElement.clicked += () =>
            {
                AdvancedDropdownMetaInfo dropdownMetaInfo = GetMetaInfo(property.intValue, metaInfo.AllCheckedInt, metaInfo.BitValueToName);

                // AdvancedDropdownAttributeDrawer.MetaInfo metaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent);
                float maxHeight = Screen.height - root.worldBound.y - root.worldBound.height - 100;
                Rect worldBound = root.worldBound;
                if (maxHeight < 100)
                {
                    worldBound.y -= 300 + worldBound.height;
                    maxHeight = 300;
                }

                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    dropdownMetaInfo,
                    root.worldBound.width,
                    maxHeight,
                    true,
                    (_, curItem) =>
                    {
                        int selectedValue = (int)curItem;
                        int newMask = selectedValue == 0
                            ? 0
                            : EnumFlagsUtil.ToggleBit(property.intValue, selectedValue);
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, newMask);
                        property.intValue = newMask;
                        property.serializedObject.ApplyModifiedProperties();

                        dropdownButton.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property)).ButtonLabelElement.text = GetSelectedNames(metaInfo.BitValueToName, newMask);
                        dropdownButton.userData = curItem;
                        onValueChangedCallback(curItem);
                    }
                ));

                string curError = dropdownMetaInfo.Error;
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
                // ReSharper disable once InvertIf
                if (helpBox.text != curError)
                {
                    helpBox.text = curError;
                    helpBox.style.display = curError == ""? DisplayStyle.None : DisplayStyle.Flex;
                }
            };
        }
    }
}
#endif
