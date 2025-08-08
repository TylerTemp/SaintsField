using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Spine;
using Spine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_2021_3_OR_NEWER
namespace SaintsField.Editor.Drawers.Spine.SpineSlotPickerDrawer
{
    public partial class SpineSlotPickerAttributeDrawer
    {
        private static string NameDropdownField(SerializedProperty property) =>
            $"{property.propertyPath}__SpineSlot_SelectorButton";

        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__SpineSlot_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return new Label(GetPreferredLabel(property));
            }

            SpineSlotElement element = new SpineSlotElement();
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

        private IReadOnlyList<SpineSlotUtils.SlotInfo> _cachedSlotInfos = new List<SpineSlotUtils.SlotInfo>();

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return;
            }

            SpineSlotPickerAttribute spineSlotPickerAttribute = (SpineSlotPickerAttribute)saintsAttribute;

            HelpBox helpBox = container.Q<HelpBox>(name: NameHelpBox(property));

            StringDropdownField stringDropdownField = container.Q<StringDropdownField>(NameDropdownField(property));
            SpineSlotElement element = stringDropdownField.Q<SpineSlotElement>();
            UIToolkitUtils.AddContextualMenuManipulator(stringDropdownField, property,
                () => Util.PropertyChangedCallback(property, info, onValueChangedCallback));

            stringDropdownField.Button.clicked += () => MakeDropdown(GetSlotInfosRefresh, property,
                stringDropdownField, onValueChangedCallback, info, parent);

            GetSlotInfosRefresh();

            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(GetSlotInfosRefreshListener);
            stringDropdownField.RegisterCallback<DetachFromPanelEvent>(_ =>
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(GetSlotInfosRefreshListener));
            return;

            IReadOnlyList<SpineSlotUtils.SlotInfo> GetSlotInfosRefresh()
            {
                (string error, IReadOnlyList<SpineSlotUtils.SlotInfo> slots) = GetSlots(
                    spineSlotPickerAttribute.ContainsBoundingBoxes, spineSlotPickerAttribute.SkeletonTarget, property,
                    info, parent);
                if (helpBox.text != error)
                {
                    helpBox.text = error;
                    helpBox.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
                }

                if (error == "")
                {
                    if (!slots.SequenceEqual(_cachedSlotInfos))
                    {
                        element.BindSlotInfos(_cachedSlotInfos = slots);
                    }
                }

                return _cachedSlotInfos;
            }

            void GetSlotInfosRefreshListener() => GetSlotInfosRefresh();
        }

        private static void MakeDropdown(Func<IReadOnlyList<SpineSlotUtils.SlotInfo>> getSlotInfosRefresh, SerializedProperty property, StringDropdownField root, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            AdvancedDropdownMetaInfo metaInfo = GetMetaInfo(property.stringValue, getSlotInfosRefresh(), false);

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                true,
                (_, curItem) =>
                {
                    SlotData newValue = (SlotData)curItem;
                    string newString = newValue?.Name ?? "";
                    // ReSharper disable once InvertIf
                    if (property.stringValue != newString)
                    {
                        property.stringValue = newString;
                        property.serializedObject.ApplyModifiedProperties();
                        onValueChangedCallback.Invoke(newString);
                    }
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }
    }
}
#endif
