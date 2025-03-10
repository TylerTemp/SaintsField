using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField.Spine
{
    public class SpineAttachmentPickerAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly string SkeletonTarget;
        public readonly bool SkinTargetIsCallback;
        public readonly string SkinTarget;
        public readonly bool SlotTargetIsCallback;
        public readonly string SlotTarget;
        public readonly bool CurrentSkinOnly;
        public readonly bool ReturnAttachmentPath;
        public readonly bool PlaceholdersOnly;
        public readonly bool SepAsSub;

        public SpineAttachmentPickerAttribute(string skeletonTarget = null, string skinTarget = null, string slotTarget = null, bool currentSkinOnly = true, bool returnAttachmentPath = false, bool placeholdersOnly = false, bool sepAsSub = true)
        {
            SkeletonTarget = RuntimeUtil.ParseCallback(skeletonTarget).content;
            (SkinTarget, SkinTargetIsCallback) = RuntimeUtil.ParseCallback(skinTarget);
            (SlotTarget, SlotTargetIsCallback) = RuntimeUtil.ParseCallback(slotTarget);
            CurrentSkinOnly = currentSkinOnly;
            ReturnAttachmentPath = returnAttachmentPath;
            PlaceholdersOnly = placeholdersOnly;
            SepAsSub = sepAsSub;
        }
    }
}
