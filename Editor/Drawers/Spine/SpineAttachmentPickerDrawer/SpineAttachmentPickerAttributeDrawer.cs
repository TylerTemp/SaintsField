using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Spine;
using Spine;
using Spine.Unity;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Spine.SpineAttachmentPickerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SpineAttachmentPickerAttribute), true)]
    public partial class SpineAttachmentPickerAttributeDrawer: SaintsPropertyDrawer
    {
        private const string IconAttachment = "Spine/icon-attachment.png";
        private const string IconImage = "Spine/icon-image.png";
        private const string IconWeights = "Spine/icon-weights.png";
        private const string IconMesh = "Spine/icon-mesh.png";
        private const string IconBoundingBox = "Spine/icon-boundingBox.png";
        private const string IconPoint = "Spine/icon-point.png";
        private const string IconPath = "Spine/icon-path.png";
        private const string IconClipping = "Spine/icon-clipping.png";

        public struct AttachmentInfo: IEquatable<AttachmentInfo>
        {
            public string Path;
            public string Name;
            public bool Disabled;
            public string Icon;

            public bool Equals(AttachmentInfo other)
            {
                return Path == other.Path && Name == other.Name;
            }

            public override bool Equals(object obj)
            {
                return obj is AttachmentInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Path, Name);
            }
        }



        private static string Validate(SpineAttachmentPickerAttribute spineAttachmentPickerAttribute, SerializedProperty property, MemberInfo info, object parent)
        {
            if (string.IsNullOrEmpty(property.stringValue))
            {
                return "";
            }

            SpineAttachmentUtils.AttachmentsResult attachmentsResult = GetAttachments(spineAttachmentPickerAttribute, property, info, parent);
            if (attachmentsResult.Error != "")
            {
                return attachmentsResult.Error;
            }

            return attachmentsResult.AttachmentInfos.Any(each => (spineAttachmentPickerAttribute.ReturnAttachmentPath? each.Path: each.Name) == property.stringValue)
                ? ""
                : $"{property.stringValue} is not a valid attachment: {string.Join(", ", attachmentsResult.AttachmentInfos.Select(each => each.Name))}";
        }

        private struct AttachInfo : IEquatable<AttachInfo>
        {
            public string Name;
            public Attachment Attachment;

            public bool Equals(AttachInfo other)
            {
                return Name == other.Name;
            }

            public override bool Equals(object obj)
            {
                return obj is AttachInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (Name != null ? Name.GetHashCode() : 0);
            }
        }

        private static SpineAttachmentUtils.AttachmentsResult GetAttachments(SpineAttachmentPickerAttribute spineAttachmentPickerAttribute, SerializedProperty property, MemberInfo info, object parent)
        {
            (string error, SkeletonDataAsset skeletonDataAsset) = SpineUtils.GetSkeletonDataAsset(spineAttachmentPickerAttribute.SkeletonTarget, property, info, parent);
            if (error != "")
            {
                return new SpineAttachmentUtils.AttachmentsResult(error, null, null);
            }

            if (skeletonDataAsset == null)
            {
                return new SpineAttachmentUtils.AttachmentsResult($"No SkeletonDataAsset found for {property.propertyPath}{(spineAttachmentPickerAttribute.SkeletonTarget == null ? " " : $" {spineAttachmentPickerAttribute.SkeletonTarget}")}", null, null);
            }

            SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);

            if (skeletonData == null)
            {
                return new SpineAttachmentUtils.AttachmentsResult(
                    $"No skeletonData found for {property.propertyPath}{(spineAttachmentPickerAttribute.SkeletonTarget == null ? " " : $" {spineAttachmentPickerAttribute.SkeletonTarget}")}",
                    null, null);
            }

            List<Skin> validSkins = new List<Skin>();

            if (spineAttachmentPickerAttribute.CurrentSkinOnly) {
                string currentSkinName;
                if (spineAttachmentPickerAttribute.SkinTargetIsCallback)
                {
                    (string error, string result) skinFilter = Util.GetOf<string>(spineAttachmentPickerAttribute.SkinTarget, null, property, info, parent);
                    if (skinFilter.error != "")
                    {
                        return new SpineAttachmentUtils.AttachmentsResult(skinFilter.error, null, null);
                    }
                    currentSkinName = skinFilter.result;
                }
                else
                {
                    currentSkinName = spineAttachmentPickerAttribute.SkinTarget;
                }

                Skin currentSkin = string.IsNullOrEmpty(currentSkinName)
                    ? skeletonData.Skins.Items[0]
                    : skeletonData.FindSkin(currentSkinName);

                validSkins.Add(currentSkin ?? skeletonData.Skins.Items[0]);
            }
            else
            {
                validSkins.AddRange(skeletonData.Skins.Where(skin => skin != null));
            }


            List<AttachInfo> attachmentNames = new List<AttachInfo>();
            List<AttachInfo> placeholderNames = new List<AttachInfo>();
            string prefix = "";

            string targetName = skeletonDataAsset.name;
            Skin defaultSkin = skeletonData.Skins.Items[0];

            string currentSlotName;
            if (spineAttachmentPickerAttribute.SlotTargetIsCallback)
            {
                (string error, string result) slotFilter = Util.GetOf<string>(spineAttachmentPickerAttribute.SlotTarget, null, property, info, parent);
                if (slotFilter.error != "")
                {
                    return new SpineAttachmentUtils.AttachmentsResult(slotFilter.error, null, null);
                }
                currentSlotName = slotFilter.result;
            }
            else
            {
                currentSlotName = spineAttachmentPickerAttribute.SkinTarget;
            }

            string slotMatch = string.IsNullOrEmpty(currentSlotName)? "": currentSlotName.ToLower();

            List<AttachmentInfo> attachmentInfos = new List<AttachmentInfo>();
            foreach (Skin skin in validSkins) {
                string skinPrefix = skin.Name + "/";

                if (validSkins.Count > 1)
                {
                    prefix = skinPrefix;
                }

                for (int slotIndex = 0; slotIndex < skeletonData.Slots.Count; slotIndex++) {
                    if (slotMatch.Length > 0 && !skeletonData.Slots.Items[slotIndex].Name.Equals(slotMatch, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    attachmentNames.Clear();
                    placeholderNames.Clear();

                    List<Skin.SkinEntry> skinEntries = new List<Skin.SkinEntry>();
                    skin.GetAttachments(slotIndex, skinEntries);
                    attachmentNames.AddRange(skinEntries.Select(entry => new AttachInfo
                    {
                        Name = entry.Name,
                        Attachment = entry.Attachment,
                    }));

                    if (skin != defaultSkin) {
                        placeholderNames.AddRange(skinEntries.Select(entry => new AttachInfo
                        {
                            Name = entry.Name,
                            Attachment = entry.Attachment,
                        }));
                        skinEntries.Clear();
                        defaultSkin.GetAttachments(slotIndex, skinEntries);
                        attachmentNames.AddRange(skinEntries.Select(entry => new AttachInfo
                        {
                            Name = entry.Name,
                            Attachment = entry.Attachment,
                        }));
                    }

                    foreach (AttachInfo attachInfo in attachmentNames)
                    {
                        string menuPath = prefix + skeletonData.Slots.Items[slotIndex].Name + "/" + attachInfo.Name;
                        string name = attachInfo.Name;

                        if (spineAttachmentPickerAttribute.ReturnAttachmentPath)
                        {
                            name = skin.Name + "/" + skeletonData.Slots.Items[slotIndex].Name + "/" + attachInfo.Name;
                        }

                        bool disabled = spineAttachmentPickerAttribute.PlaceholdersOnly &&
                                        !placeholderNames.Contains(attachInfo);

                        attachmentInfos.Add(new AttachmentInfo
                        {
                            Path = menuPath,
                            Name = name,
                            Disabled = disabled,
                            Icon = GetIconPathFromAttachment(attachInfo.Attachment),
                        });
                    }

                }
            }

            return new SpineAttachmentUtils.AttachmentsResult(error, targetName, attachmentInfos);
        }

        private static string GetIconPathFromAttachment(Attachment attachment)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (attachment)
            {
                case RegionAttachment:
                    return IconImage;
                case MeshAttachment meshAttachment:
                    return meshAttachment.IsWeighted() ? IconWeights : IconMesh;
                case BoundingBoxAttachment:
                    return IconBoundingBox;
                case PointAttachment:
                    return IconPoint;
                case PathAttachment:
                    return IconPath;
                case ClippingAttachment:
                    return IconClipping;
                default:
                    return IconAttachment;
            }
        }

        private static AdvancedDropdownMetaInfo GetMetaInfo(string curValue, SpineAttachmentUtils.AttachmentsResult attachmentsResult, bool subAsSep)
        {
            AdvancedDropdownList<string> dropdownListValue =
                new AdvancedDropdownList<string>(attachmentsResult.TargetName)
                {
                    { "[Empty String]", "" },
                };

            dropdownListValue.AddSeparator();

            object[] curValues = { curValue };

            foreach (AttachmentInfo attachmentInfo in attachmentsResult.AttachmentInfos)
            {
                if(subAsSep)
                {
                    dropdownListValue.Add(attachmentInfo.Path, attachmentInfo.Name, icon: attachmentInfo.Icon, disabled: attachmentInfo.Disabled);
                }
                else
                {
                    dropdownListValue.Add(new AdvancedDropdownList<string>(attachmentInfo.Path, attachmentInfo.Name, icon: attachmentInfo.Icon, disabled: attachmentInfo.Disabled));
                }
            }

            // if (curValues.Length == 0)
            // {
            //     curSelected = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>();
            // }
            // else
            // {
            (IReadOnlyList<AdvancedDropdownAttributeDrawer.SelectStack> stacks, string _) =
                AdvancedDropdownUtil.GetSelected(curValues[0],
                    Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(), dropdownListValue);

            // }
            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                CurValues = curValues,
                DropdownListValue = dropdownListValue,
                SelectStacks = stacks,
            };
        }
    }
}
