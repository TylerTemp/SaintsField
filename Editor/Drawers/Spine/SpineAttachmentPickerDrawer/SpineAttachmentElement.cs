#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;

namespace SaintsField.Editor.Drawers.Spine.SpineAttachmentPickerDrawer
{
    public class SpineAttachmentElement: StringDropdownElement
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private SpineAttachmentUtils.AttachmentsResult _attachmentsResult;

        public void BindAttachments(SpineAttachmentUtils.AttachmentsResult attachmentsResult)
        {
            _attachmentsResult = attachmentsResult;
            RefreshDisplay();
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_attachmentsResult.Error != "")
            {
                return;
            }

            if (string.IsNullOrEmpty(CachedValue))
            {
                Label.Clear();
                return;
            }

            foreach (SpineAttachmentPickerAttributeDrawer.AttachmentInfo attachmentInfo in _attachmentsResult.AttachmentInfos)
            {
                if (attachmentInfo.Name == CachedValue)
                {
                    List<RichTextDrawer.RichTextChunk> chunks = new List<RichTextDrawer.RichTextChunk>
                    {
                        new RichTextDrawer.RichTextChunk
                        {
                            IsIcon = false,
                            Content = attachmentInfo.Name,
                        },
                    };
                    if (!string.IsNullOrEmpty(attachmentInfo.Icon))
                    {
                        chunks.Insert(0, new RichTextDrawer.RichTextChunk
                        {
                            IsIcon = true,
                            Content = attachmentInfo.Icon,
                        });
                    }
                    UIToolkitUtils.SetLabel(Label, chunks, _richTextDrawer);
                    return;
                }
            }

            UIToolkitUtils.SetLabel(Label, new[]
            {
                new RichTextDrawer.RichTextChunk
                {
                    IsIcon = false,
                    Content = "?",
                    IconColor = "#FF0000",
                },
                new RichTextDrawer.RichTextChunk
                {
                    IsIcon = false,
                    Content = $" {CachedValue}",
                },
            }, _richTextDrawer);
        }
    }
}
#endif
