#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
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
            return new AddressableSceneField(GetPreferredLabel(property), element)
            {
                name = NameAddressableSceneField(property),
            };
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
                property.stringValue = newValue;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(newValue);
                ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, newValue);
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

                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    metaInfo,
                    worldBound.width,
                    maxHeight,
                    false,
                    (_, curItem) =>
                    {
                        AddressableAssetEntry entry = (AddressableAssetEntry)curItem;
                        string newValue = entry?.address ?? "";
                        property.stringValue = newValue;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback.Invoke(newValue);
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

        // protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
        //     int index,
        //     IReadOnlyList<PropertyAttribute> allAttributes,
        //     VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        // {
        //     if (AddressableAssetSettingsDefaultObject.GetSettings(false) == null)
        //     {
        //         UpdateHelpBox(container.Q<HelpBox>(name: NameHelpBox(property)), AddressableUtil.ErrorNoSettings);
        //         return;
        //     }
        //
        //     ObjectField objectField = container.Q<ObjectField>(name: NameObjectField(property));
        //     string oldValue = (string)objectField.userData;
        //     if (oldValue == property.stringValue)
        //     {
        //         return;
        //     }
        //
        //     UpdateFieldAndErrorMessage(objectField, container.Q<HelpBox>(name: NameHelpBox(property)), property.stringValue, (AddressableSceneAttribute)saintsAttribute);
        // }
        //
        // protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
        //     FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        // {
        //     string value = (string)newValue;
        //     ObjectField objectField = container.Q<ObjectField>(name: NameObjectField(property));
        //     if(value == objectField.userData as string)
        //     {
        //         return;
        //     }
        //
        //     UpdateFieldAndErrorMessage(objectField, container.Q<HelpBox>(name: NameHelpBox(property)), value, (AddressableSceneAttribute)saintsAttribute);
        // }
        //
        // private static void UpdateFieldAndErrorMessage(ObjectField objectField, HelpBox helpBox, string value, AddressableSceneAttribute addressableSceneAttribute)
        // {
        //     (string error, AddressableAssetEntry sceneEntry) = GetSceneEntry(value, addressableSceneAttribute);
        //     if (error != "")
        //     {
        //         UpdateHelpBox(helpBox, error);
        //         return;
        //     }
        //
        //     UpdateHelpBox(helpBox, "");
        //     objectField.userData = value;
        //     objectField.SetValueWithoutNotify(sceneEntry?.MainAsset);
        // }

        // protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
        //     ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
        //     IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        // {
        //     UIToolkitUtils.DropdownButtonField dropdownField =
        //         container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));
        //     UIToolkitUtils.SetLabel(dropdownField.labelElement, richTextChunks, richTextDrawer);
        // }
    }
}
#endif
