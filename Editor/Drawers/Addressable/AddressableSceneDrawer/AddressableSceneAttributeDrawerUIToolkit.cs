#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Addressable.AddressableSceneDrawer
{
    public partial class AddressableSceneAttributeDrawer
    {
        private static string NameHelpBox(SerializedProperty property) =>
            $"{property.propertyPath}__AddressableAddress_HelpBox";

        private static string NameAddressableSceneField(SerializedProperty property) => $"{property.propertyPath}__AddressableScene";
        // private static string NameSelectorButton(SerializedProperty property) => $"{property.propertyPath}__AddressableScene_SelectorButton";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container1,
            FieldInfo info, object parent)
        {
            AddressableSceneElement element = new AddressableSceneElement((AddressableSceneAttribute) saintsAttribute);
            element.BindProperty(property);
            AddressableSceneField field = new AddressableSceneField(GetPreferredLabel(property), element)
            {
                name = NameAddressableSceneField(property),
            };
            if (!string.IsNullOrEmpty(property.tooltip) && field.labelElement != null)
            {
                field.labelElement.tooltip = property.tooltip;
            }
            return field;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBoxElement = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                    flexShrink = 1,
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
            AddressableSceneAttribute addressableSceneAttribute = (AddressableSceneAttribute)saintsAttribute;

            AddressableSceneField addressableSceneField = container.Q<AddressableSceneField>(NameAddressableSceneField(property));
            AddressableSceneElement addressableSceneElement = addressableSceneField.Q<AddressableSceneElement>();
            HelpBox helpBoxElement = container.Q<HelpBox>(name: NameHelpBox(property));

            UpdateHelpBox(helpBoxElement, addressableSceneElement.Error);
            addressableSceneElement.ErrorEvent.AddListener(e => UpdateHelpBox(helpBoxElement, e));

            addressableSceneElement.SceneFieldDropChanged.AddListener(newValue =>
            {
                ApplyAddressableSceneSelection(property, info, parent, newValue,
                    changedValue => onValueChangedCallback.Invoke(changedValue));
            });

            addressableSceneElement.Button.clicked += () =>
            {
                (string error, IEnumerable<AddressableAssetEntry> assetGroups) = AddressableUtil.GetAllEntries(addressableSceneAttribute.Group, addressableSceneAttribute.LabelFilters);
                if (error != "")
                {
                    UpdateHelpBox(helpBoxElement, error);
                    return;
                }

                AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(property.stringValue, assetGroups.Where(each => each.MainAsset is SceneAsset), addressableSceneAttribute.SepAsSub, false);
                (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(container.worldBound);

                UnityEditor.PopupWindow.Show(worldBound, new SaintsTreeDropdownUIToolkit(
                    metaInfo,
                    worldBound.width,
                    maxHeight,
                    false,
                    (curItem, _) =>
                    {
                        AddressableAssetEntry entry = (AddressableAssetEntry)curItem;
                        string newValue = entry?.address ?? "";
                        ApplyAddressableSceneSelection(property, info, parent, newValue,
                            changedValue => onValueChangedCallback.Invoke(changedValue));
                        return null;
                    }
                ));
            };

            // UpdateFieldAndErrorMessage(objectField, helpBoxElement, property.stringValue, addressableSceneAttribute);
        }

        private static void UpdateHelpBox(HelpBox helpBox, string error)
        {
            if (helpBox.text == error)
            {
                return;
            }

            helpBox.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
            helpBox.text = error;
        }
    }
}
#endif
