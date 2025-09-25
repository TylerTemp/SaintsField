#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.EnumFlagsDrawers;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public partial class TreeDropdownAttributeDrawer
    {
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__TreeDropdown_Button";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__TreeDropdown_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container,
            FieldInfo info,
            object parent)
        {
            AdvancedDropdownMetaInfo initMetaInfo = AdvancedDropdownAttributeDrawer.GetMetaInfo(property, (PathedDropdownAttribute)saintsAttribute, info, parent, false);

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            dropdownButton.style.flexGrow = 1;
            dropdownButton.name = NameButton(property);
            dropdownButton.userData = initMetaInfo.CurValues;

            dropdownButton.AddToClassList(ClassAllowDisable);

            EmptyPrefabOverrideElement emptyPrefabOverrideElement = new EmptyPrefabOverrideElement(property);
            emptyPrefabOverrideElement.Add(dropdownButton);

            return emptyPrefabOverrideElement;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
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

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButtonField = container.Q<UIToolkitUtils.DropdownButtonField>(NameButton(property));
            VisualElement root = container.Q<VisualElement>(NameLabelFieldUIToolkit(property));
            dropdownButtonField.ButtonElement.clicked += () =>
            {
                AdvancedDropdownMetaInfo metaInfo = AdvancedDropdownAttributeDrawer.GetMetaInfo(property, (PathedDropdownAttribute)saintsAttribute, info, parent, false);

                (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

                SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                    metaInfo,
                    root.worldBound.width,
                    maxHeight,
                    false,
                    (curItem, _) =>
                    {
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info,
                            parent, curItem);
                        Util.SignPropertyValue(property, info, parent, curItem);
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback(curItem);
                        return null;
                    }
                );

                // DebugPopupExample.SaintsAdvancedDropdownUIToolkit = sa;
                // var editorWindow = EditorWindow.GetWindow<DebugPopupExample>();
                // editorWindow.Show();

                UnityEditor.PopupWindow.Show(worldBound, sa);

                string curError = metaInfo.Error;
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
                // ReSharper disable once InvertIf
                if (helpBox.text != curError)
                {
                    helpBox.text = curError;
                    helpBox.style.display = curError == ""? DisplayStyle.None : DisplayStyle.Flex;
                }
            };

            dropdownButtonField.TrackPropertyValue(property, UpdateButtonLabel);
            UpdateButtonLabel(property);
            return;

            void UpdateButtonLabel(SerializedProperty p)
            {
                string display =
                    AdvancedDropdownAttributeDrawer.GetMetaStackDisplay(AdvancedDropdownAttributeDrawer.GetMetaInfo(p, (PathedDropdownAttribute)saintsAttribute, info, parent, false));
                if((string)dropdownButtonField.ButtonLabelElement.userData != display)
                {
                    dropdownButtonField.ButtonLabelElement.userData = display;
                    UIToolkitUtils.SetLabel(dropdownButtonField.ButtonLabelElement, RichTextDrawer.ParseRichXml(display, "", null, null, null), _richTextDrawer);
                }
            }
        }

        private readonly struct DrawInfo
        {
            public readonly struct EnumValueInfo
            {
                public readonly bool HasValue;
                public readonly object Value;
                public readonly string Label;

                public EnumValueInfo(object value, string label)
                {
                    HasValue = true;
                    Value = value;
                    Label = label;
                }
            }

            public readonly IReadOnlyList<EnumValueInfo> EnumValues;
            public readonly EnumValueInfo NothingValue;
            public readonly EnumValueInfo EverythingValue;
            public readonly object EverythingBit;
            public readonly bool IsFlags;
            public readonly bool IsULong;

            public DrawInfo(IReadOnlyList<EnumValueInfo> enumValues, EnumValueInfo everythingValue, EnumValueInfo nothingValue, object everythingBit, bool isFlags, bool isULong)
            {
                EnumValues = enumValues;
                EverythingValue = everythingValue;
                NothingValue = nothingValue;
                EverythingBit = everythingBit;
                IsFlags = isFlags;
                IsULong = isULong;
            }
        }


    }
}
#endif
