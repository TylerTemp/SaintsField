using System;
using System.Collections.Generic;
using System.Linq;

namespace SaintsField.Editor.Drawers.Spine.SpineAttachmentPickerDrawer
{
    public static class SpineAttachmentUtils
    {
        public readonly struct AttachmentsResult: IEquatable<AttachmentsResult>
        {
            public readonly string Error;
            public readonly string TargetName;

            public readonly IReadOnlyList<SpineAttachmentPickerAttributeDrawer.AttachmentInfo> AttachmentInfos;

            public AttachmentsResult(string error, string targetName, IReadOnlyList<SpineAttachmentPickerAttributeDrawer.AttachmentInfo> attachmentInfos)
            {
                Error = error;
                TargetName = targetName;
                AttachmentInfos = attachmentInfos;
            }

            public bool Equals(AttachmentsResult other)
            {
                if(AttachmentInfos == null)
                {
                    return other.AttachmentInfos == null;
                }

                if (other.AttachmentInfos == null)
                {
                    return AttachmentInfos == null;
                }

                return AttachmentInfos.SequenceEqual(other.AttachmentInfos);
                // return Equals(AttachmentInfos, other.AttachmentInfos);
            }

            public override bool Equals(object obj)
            {
                return obj is AttachmentsResult other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (AttachmentInfos != null ? AttachmentInfos.GetHashCode() : 0);
            }
        }
    }
}
