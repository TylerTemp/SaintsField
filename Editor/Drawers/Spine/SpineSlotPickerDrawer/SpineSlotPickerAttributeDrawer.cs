using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Spine;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Spine.SpineSlotPickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SpineSlotPickerAttribute), true)]
    public partial class SpineSlotPickerAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private static string Validate(bool containsBoundingBoxes, string callback, SerializedProperty property, MemberInfo info, object parent)
        {
            if (string.IsNullOrEmpty(property.stringValue))
            {
                return "";
            }

            (string error, IReadOnlyList<SpineSlotUtils.SlotInfo> slots) = GetSlots(containsBoundingBoxes, callback, property, info, parent);
            if (error != "")
            {
                return error;
            }

            foreach (SpineSlotUtils.SlotInfo slotInfo in slots)
            {
                if (slotInfo.SlotData.Name == property.stringValue)
                {
                    return slotInfo.Disabled
                        ? $"slot `{property.stringValue}` don't contain bounding box attachments"
                        : "";
                }
            }

            return $"{property.stringValue} is not a valid slot: {string.Join(", ", slots.Select(each => each.SlotData.Name))}";
        }



        private static (string error, IReadOnlyList<SpineSlotUtils.SlotInfo> slots) GetSlots(bool containsBoundingBoxes, string callback, SerializedProperty property, MemberInfo info, object parent)
        {
            (string error, SkeletonDataAsset skeletonDataAsset) = SpineUtils.GetSkeletonDataAsset(callback, property, info, parent);
            if (error != "")
            {
                return (error, null);
            }

            if (skeletonDataAsset == null)
            {
                return ($"No SkeletonDataAsset found for {property.propertyPath}{(callback == null? " ": $" {callback}")}", null);
            }

            SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);

            if (skeletonData == null)
            {
                return ($"No skeletonData found for {property.propertyPath}{(callback == null? " ": $" {callback}")}", null);
            }

            IEnumerable<SlotData> orderedSlots = skeletonData.Slots.Items.OrderBy(slotData => slotData.Name);
            List<SpineSlotUtils.SlotInfo> slots = new List<SpineSlotUtils.SlotInfo>();

            foreach (SlotData slotData in orderedSlots) {
                int slotIndex = slotData.Index;
                string name = slotData.Name;
                if (containsBoundingBoxes) {
                    List<Skin.SkinEntry> skinEntries = new List<Skin.SkinEntry>();
                    foreach (Skin skin in skeletonData.Skins) {
                        skin.GetAttachments(slotIndex, skinEntries);
                    }

                    bool hasBoundingBox = false;
                    foreach (Skin.SkinEntry entry in skinEntries) {
                        BoundingBoxAttachment bbAttachment = entry.Attachment as BoundingBoxAttachment;
                        if (bbAttachment != null) {
                            string menuLabel = bbAttachment.IsWeighted() ? name + " (!)" : name;
                            // menu.AddItem(new GUIContent(menuLabel), !property.hasMultipleDifferentValues && name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
                            slots.Add(new SpineSlotUtils.SlotInfo(slotData, false, menuLabel));
                            hasBoundingBox = true;
                            break;
                        }
                    }

                    if (!hasBoundingBox)
                    {
                        slots.Add(new SpineSlotUtils.SlotInfo(slotData, true, name));
                        // menu.AddDisabledItem(new GUIContent(name));
                    }

                } else {
                    slots.Add(new SpineSlotUtils.SlotInfo(slotData, false, name));
                    // menu.AddItem(new GUIContent(name), !property.hasMultipleDifferentValues && name == property.stringValue, HandleSelect, new SpineDrawerValuePair(name, property));
                }

            }

            return ("", slots);
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(string curValue, IReadOnlyList<SpineSlotUtils.SlotInfo> slots, bool isImGui)
        {
            AdvancedDropdownList<SlotData> dropdownListValue =
                new AdvancedDropdownList<SlotData>(isImGui? "Select Slot": "")
                {
                    { "[Empty String]", null },
                };

            dropdownListValue.AddSeparator();

            List<object> curValues = new List<object>();

            foreach (SpineSlotUtils.SlotInfo slotInfo in slots)
            {
                dropdownListValue.Add(slotInfo.Label, slotInfo.SlotData, disabled: slotInfo.Disabled, icon: SpineSlotUtils.IconPath);
                if (slotInfo.SlotData.Name == curValue)
                {
                    curValues.Add(slotInfo.SlotData);
                }
            }

            IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> curSelected;
            if (curValues.Count == 0)
            {
                curSelected = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            }
            else
            {
                (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stacks, string _) =
                    AdvancedDropdownUtil.GetSelected(curValues[0],
                        Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdownListValue);
                curSelected = stacks;
            }
            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = curValues,
                DropdownListValue = dropdownListValue,
                SelectStacks = curSelected,
            };
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            SpineSlotPickerAttribute spineSlotPickerAttribute = (SpineSlotPickerAttribute)propertyAttribute;
            string error = Validate(spineSlotPickerAttribute.ContainsBoundingBoxes, spineSlotPickerAttribute.SkeletonTarget, property, memberInfo, parent);
            return error == ""
                ? null
                : new AutoRunnerFixerResult
                {
                    ExecError = "",
                    Error = error,
                };
        }
    }
}
