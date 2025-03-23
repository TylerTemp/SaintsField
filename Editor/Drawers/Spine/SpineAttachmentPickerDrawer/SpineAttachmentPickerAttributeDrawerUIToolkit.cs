#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Spine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Spine.SpineAttachmentPickerDrawer
{
    public partial class SpineAttachmentPickerAttributeDrawer
    {
        private static string NameDropdownField(SerializedProperty property) => $"{property.propertyPath}__SpineAttachment_SelectorButton";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__SpineAttachment_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownField = UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            dropdownField.name = NameDropdownField(property);
            SetDropdownLabel(dropdownField.ButtonLabelElement, property.stringValue);

            return dropdownField;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                },
                name = NameHelpBox(property),
            };
        }

        // private Texture2D _iconTexture2D;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SpineAttachmentPickerAttribute spineAttachmentPickerAttribute = (SpineAttachmentPickerAttribute) saintsAttribute;

            UIToolkitUtils.DropdownButtonField dropdownField = container.Q<UIToolkitUtils.DropdownButtonField>(name: NameDropdownField(property));
            Debug.Assert(dropdownField != null);

            SpineAttachmentPickerAttribute spineSkinPickerAttribute = (SpineAttachmentPickerAttribute)saintsAttribute;

            HelpBox helpBox = container.Q<HelpBox>(name: NameHelpBox(property));
            ValidateUIToolkit(helpBox, spineSkinPickerAttribute, property, info, parent);

            dropdownField.TrackPropertyValue(property, _ =>
            {
                SetDropdownLabel(dropdownField.ButtonLabelElement, property.stringValue);
                ValidateUIToolkit(helpBox, spineSkinPickerAttribute, property, info, parent);
            });

            dropdownField.ButtonElement.clicked += () =>
            {
                AttachmentsResult attachmentsResult = GetAttachments(spineSkinPickerAttribute, property, info, parent);
                if (attachmentsResult.Error != "")
                {
                    UpdateHelpBox(container.Q<HelpBox>(NameHelpBox(property)), attachmentsResult.Error);
                }

                AdvancedDropdownMetaInfo dropdownMetaInfo = GetMetaInfo(property.stringValue, attachmentsResult, spineAttachmentPickerAttribute.SepAsSub);

                float maxHeight = Screen.currentResolution.height - dropdownField.worldBound.y - dropdownField.worldBound.height - 100;
                // Rect worldBound = dropdownButton.worldBound;
                Rect worldBound = dropdownField.worldBound;
                if (maxHeight < 100)
                {
                    worldBound.y -= 100 + worldBound.height;
                    maxHeight = 100;
                }

                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    dropdownMetaInfo,
                    worldBound.width,
                    maxHeight,
                    false,
                    (_, curItem) =>
                    {
                        string newValue = (string)curItem;
                        if (property.stringValue != newValue)
                        {
                            property.stringValue = newValue;
                            property.serializedObject.ApplyModifiedProperties();
                            onValueChangedCallback.Invoke(newValue);
                        }
                    }
                ));
            };
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        {
            UIToolkitUtils.DropdownButtonField dropdownField =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));
            UIToolkitUtils.SetLabel(dropdownField.labelElement, richTextChunks, richTextDrawer);
        }

        private static void ValidateUIToolkit(HelpBox helpBox, SpineAttachmentPickerAttribute spineAttachmentPickerAttribute, SerializedProperty property, MemberInfo info, object parent)
        {
            string error = Validate(spineAttachmentPickerAttribute, property, info, parent);
            UpdateHelpBox(helpBox, error);
        }

        private RichTextDrawer _textDrawer;

        private void SetDropdownLabel(Label label, string value)
        {
            _textDrawer ??= new RichTextDrawer();

            IEnumerable<RichTextDrawer.RichTextChunk> chunksOrNull;

            if (string.IsNullOrEmpty(value))
            {
                chunksOrNull = new[]
                {
                    new RichTextDrawer.RichTextChunk
                    {
                        IsIcon = false,
                        Content = "-",
                    },
                };
            }
            else
            {
                chunksOrNull = new[]
                {
                    new RichTextDrawer.RichTextChunk
                    {
                        IsIcon = true,
                        Content = IconAttachment,
                    },
                    new RichTextDrawer.RichTextChunk
                    {
                        IsIcon = false,
                        Content = value,
                    },
                };
            }

            UIToolkitUtils.SetLabel(label, chunksOrNull, _textDrawer);
        }

        private static void UpdateHelpBox(HelpBox helpBox, string error)
        {
            if (helpBox.text == error)
            {
                return;
            }

            helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            helpBox.text = error;
        }
    }
}
#endif
