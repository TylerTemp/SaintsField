#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.AddressableAssets;
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

        private static string NameObjectField(SerializedProperty property) => $"{property.propertyPath}__AddressableScene_ObjectField";
        private static string NameSelectorButton(SerializedProperty property) => $"{property.propertyPath}__AddressableScene_SelectorButton";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container1,
            FieldInfo info, object parent)
        {
            ObjectField objectField = new ObjectField(GetPreferredLabel(property))
            {
                objectType = typeof(SceneAsset),
                allowSceneObjects = false,
                name = NameObjectField(property),
                userData = "",
            };

            StyleSheet uss = Util.LoadResource<StyleSheet>("UIToolkit/PropertyFieldHideSelector.uss");
            objectField.styleSheets.Add(uss);

            objectField.AddToClassList(ClassAllowDisable);
            objectField.AddToClassList(ObjectField.alignedFieldUssClassName);

            return objectField;
        }

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            Button selectorButton = new Button
            {
                // text = "●",
                style =
                {
                    backgroundImage = Util.LoadResource<Texture2D>("classic-dropdown.png"),
                    width = SingleLineHeight,
                    marginLeft = 0,
                    marginRight = 0,
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(14, 14),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                },
                name = NameSelectorButton(property),
            };
            return selectorButton;
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

            HelpBox helpBoxElement = container.Q<HelpBox>(name: NameHelpBox(property));
            ObjectField objectField = container.Q<ObjectField>(name: NameObjectField(property));
            objectField.RegisterValueChangedCallback(newObj =>
            {
                (string error, AddressableAssetEntry sceneEntry) = GetSceneEntryFromSceneAsset(newObj.newValue, addressableSceneAttribute);
                if (error != "")
                {
                    UpdateHelpBox(helpBoxElement, error);
                    return;
                }

                UpdateHelpBox(helpBoxElement, "");
                string newValue = sceneEntry == null ? "" : sceneEntry.address;
                property.stringValue = newValue;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(newValue);
            });

            Button selectorButton = container.Q<Button>(name: NameSelectorButton(property));
            selectorButton.clicked += () =>
            {
                (string error, IEnumerable<AddressableAssetEntry> assetGroups) = AddressableUtil.GetAllEntries(addressableSceneAttribute.Group, addressableSceneAttribute.LabelFilters);
                if (error != "")
                {
                    UpdateHelpBox(helpBoxElement, error);
                    return;
                }

                AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(property.stringValue, assetGroups.Where(each => each.MainAsset is SceneAsset), addressableSceneAttribute.SepAsSub, false);
                Rect worldBound = new Rect(objectField.worldBound)
                {
                    width = objectField.worldBound.width + selectorButton.worldBound.width,
                };
                float maxHeight = Screen.currentResolution.height - worldBound.y - worldBound.height - 100;
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

            UpdateFieldAndErrorMessage(objectField, helpBoxElement, property.stringValue, addressableSceneAttribute);
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

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            if (AddressableAssetSettingsDefaultObject.GetSettings(false) == null)
            {
                UpdateHelpBox(container.Q<HelpBox>(name: NameHelpBox(property)), AddressableUtil.ErrorNoSettings);
                return;
            }

            ObjectField objectField = container.Q<ObjectField>(name: NameObjectField(property));
            string oldValue = (string)objectField.userData;
            if (oldValue == property.stringValue)
            {
                return;
            }

            UpdateFieldAndErrorMessage(objectField, container.Q<HelpBox>(name: NameHelpBox(property)), property.stringValue, (AddressableSceneAttribute)saintsAttribute);
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            string value = (string)newValue;
            ObjectField objectField = container.Q<ObjectField>(name: NameObjectField(property));
            if(value == objectField.userData as string)
            {
                return;
            }

            UpdateFieldAndErrorMessage(objectField, container.Q<HelpBox>(name: NameHelpBox(property)), value, (AddressableSceneAttribute)saintsAttribute);
        }

        private static void UpdateFieldAndErrorMessage(ObjectField objectField, HelpBox helpBox, string value, AddressableSceneAttribute addressableSceneAttribute)
        {
            (string error, AddressableAssetEntry sceneEntry) = GetSceneEntry(value, addressableSceneAttribute);
            if (error != "")
            {
                UpdateHelpBox(helpBox, error);
                return;
            }

            UpdateHelpBox(helpBox, "");
            objectField.userData = value;
            objectField.SetValueWithoutNotify(sceneEntry?.MainAsset);
        }

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
