#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
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
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new Label(GetPreferredLabel(property));
            }

            SpineAttachmentElement element = new SpineAttachmentElement();
            element.BindProperty(property);
            return new StringDropdownField(GetPreferredLabel(property), element)
            {
                name = NameDropdownField(property),
            };
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new HelpBox($"Type {property.propertyType} is not a string type.", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        flexGrow = 1,
                    },
                };
            }

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

        private SpineAttachmentUtils.AttachmentsResult _cachedAttachmentsResult;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return;
            }

            SpineAttachmentPickerAttribute spineAttachmentPickerAttribute = (SpineAttachmentPickerAttribute)saintsAttribute;
            HelpBox helpBox = container.Q<HelpBox>(name: NameHelpBox(property));

            StringDropdownField stringDropdownField = container.Q<StringDropdownField>(NameDropdownField(property));
            SpineAttachmentElement element = stringDropdownField.Q<SpineAttachmentElement>();
            UIToolkitUtils.AddContextualMenuManipulator(stringDropdownField, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            stringDropdownField.Button.clicked += () => MakeDropdown(GetAttachmentRefresh, spineAttachmentPickerAttribute, property,
                stringDropdownField, onValueChangedCallback, info, parent);

            GetAttachmentRefresh();

            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(GetSlotInfosRefreshListener);
            stringDropdownField.RegisterCallback<DetachFromPanelEvent>(_ =>
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(GetSlotInfosRefreshListener));
            return;

            SpineAttachmentUtils.AttachmentsResult GetAttachmentRefresh()
            {
                SpineAttachmentUtils.AttachmentsResult attachmentsResult = GetAttachments(spineAttachmentPickerAttribute, property, info, parent);
                if (helpBox.text != attachmentsResult.Error)
                {
                    helpBox.text = attachmentsResult.Error;
                    helpBox.style.display = string.IsNullOrEmpty(attachmentsResult.Error) ? DisplayStyle.None : DisplayStyle.Flex;
                }

                if (attachmentsResult.Error == "")
                {
                    if (!_cachedAttachmentsResult.Equals(attachmentsResult))
                    {
                        element.BindAttachments(_cachedAttachmentsResult = attachmentsResult);
                    }
                }

                return _cachedAttachmentsResult;
            }

            void GetSlotInfosRefreshListener() => GetAttachmentRefresh();
        }

        private static void MakeDropdown(Func<SpineAttachmentUtils.AttachmentsResult> getAttachmentRefresh, SpineAttachmentPickerAttribute spineAttachmentPickerAttribute, SerializedProperty property, StringDropdownField root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            SpineAttachmentUtils.AttachmentsResult attachmentsResult = getAttachmentRefresh();

            if (attachmentsResult.Error != "")
            {
                return;
            }

            AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(property.stringValue, getAttachmentRefresh(), spineAttachmentPickerAttribute.SepAsSub);
            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                true,
                (_, curItem) =>
                {
                    string newValue = (string)curItem;
                    // ReSharper disable once InvertIf
                    if (property.stringValue != newValue)
                    {
                        property.stringValue = newValue;
                        property.serializedObject.ApplyModifiedProperties();
                        ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, newValue);
                        onValueChangedCallback.Invoke(newValue);
                    }
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        {
            UIToolkitUtils.DropdownButtonField dropdownField =
                container.Q<UIToolkitUtils.DropdownButtonField>(NameDropdownField(property));
            UIToolkitUtils.SetLabel(dropdownField.labelElement, richTextChunks, richTextDrawer);
        }
    }
}
#endif
