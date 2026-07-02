#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.Addressable.AddressableLabelDrawer
{
    public partial class AddressableLabelAttributeDrawer
    {
        private static string NameDropdownField(SerializedProperty property) => $"{property.propertyPath}__AddressableLabel_DropdownField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__AddressableLabel_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container1,
            FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new VisualElement();
            }

            AddressableLabelElement element = new AddressableLabelElement();
            element.BindProperty(property);
            StringDropdownField stringDropdownField = new StringDropdownField(GetPreferredLabel(property), element)
            {
                name = NameDropdownField(property),
            };
            if (!string.IsNullOrEmpty(property.tooltip) && stringDropdownField.labelElement != null)
            {
                stringDropdownField.labelElement.tooltip = property.tooltip;
            }
            return stringDropdownField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new HelpBox($"Type {property.propertyType} is not a string type", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        display = DisplayStyle.None,
                        flexGrow = 1,
                    },
                };
            }

            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                return new HelpBox(ErrorAddressableSettingsNotCreated, HelpBoxMessageType.Error)
                {
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 1,
                    },
                    name = NameHelpBox(property),
                };
            }

            return null;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return;
            }
            StringDropdownField field = container.Q<StringDropdownField>(NameDropdownField(property));

            UIToolkitUtils.AddContextualMenuManipulator(field, property, () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            field.Button.clicked += () => ShowDropdown(property, field, onValueChangedCallback, info, parent);

            // ReSharper disable once InvertIf
            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));

                void CheckHelpBoxDisplay()
                {
                    if (AddressableAssetSettingsDefaultObject.Settings != null)
                    {
                        helpBox.style.display = DisplayStyle.None;
                    }
                }

                SaintsEditorApplicationChanged.OnAnyEvent.AddListener(CheckHelpBoxDisplay);
                field.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(CheckHelpBoxDisplay));
            }
        }

        private static void ShowDropdown(SerializedProperty property, VisualElement root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            (string _, AddressableLabelDropdownInfo dropdownInfo) =
                GetAddressableLabelDropdownInfo(property, false);

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                dropdownInfo.MetaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (curItem, _) =>
                {
                    ApplyAddressableLabelSelection(property, info, parent, dropdownInfo.Settings, (string)curItem,
                        newValue => onValueChangedCallback.Invoke(newValue));
                    return null;
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }
    }
}
#endif
