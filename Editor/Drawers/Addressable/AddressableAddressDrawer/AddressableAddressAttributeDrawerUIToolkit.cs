#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Addressable.AddressableAddressDrawer
{
    public partial class AddressableAddressAttributeDrawer
    {
        private static string NameDropdownField(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableAddress_DropdownField";

        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableAddress_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container1,
            FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new VisualElement();
            }

            AddressableAddressElement element = new AddressableAddressElement((AddressableAddressAttribute) saintsAttribute);
            element.BindProperty(property);
            return new StringDropdownField(GetPreferredLabel(property), element)
            {
                name = NameDropdownField(property),
            };
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
                return new HelpBox("Addressable Settings not created.", HelpBoxMessageType.Error)
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

            field.Button.clicked += () => ShowDropdown(property, field, (AddressableAddressAttribute)saintsAttribute, onValueChangedCallback, info, parent);

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

        private static void ShowDropdown(SerializedProperty property, VisualElement root, AddressableAddressAttribute addressableAddressAttribute, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AdvancedDropdownList<string> dropdown = new AdvancedDropdownList<string>();

            string selected = null;
            if (settings == null)
            {
                dropdown.Add("Create Addressable Settings...", null);
            }
            else
            {
                (string _, IEnumerable<AddressableAssetEntry> entries) = AddressableUtil.GetAllEntries(addressableAddressAttribute.Group, addressableAddressAttribute.LabelFilters);
                string[] keys = entries.Select(each => each.address).ToArray();

                foreach (string key in keys)
                {
                    dropdown.Add(new AdvancedDropdownList<string>(key, key));
                    if (property.stringValue == key)
                    {
                        selected = key;
                    }
                }

                if (keys.Length > 0)
                {
                    dropdown.AddSeparator();
                }

                dropdown.Add("Edit Addresses...", null, false, "d_editicon.sml");
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selected is null
                    ? Array.Empty<object>()
                    : new object[] { selected },
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (_, curItem) =>
                {
                    string newValue = (string)curItem;
                    if (newValue is null)
                    {
                        if (settings == null)
                        {
                            AddressableAssetSettingsDefaultObject.GetSettings(true);
                        }
                        else
                        {
                            AddressableUtil.OpenGroupEditor();
                        }
                        return;
                    }

                    property.stringValue = newValue;
                    ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, newValue);
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChangedCallback.Invoke(newValue);
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }
    }
}
#endif
