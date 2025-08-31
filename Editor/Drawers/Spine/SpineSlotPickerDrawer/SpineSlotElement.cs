using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;

namespace SaintsField.Editor.Drawers.Spine.SpineSlotPickerDrawer
{
    public class SpineSlotElement: StringDropdownElement
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private IReadOnlyList<SpineSlotUtils.SlotInfo> _slotInfos;

        public void BindSlotInfos(IReadOnlyList<SpineSlotUtils.SlotInfo> cachedSlotInfos)
        {
            _slotInfos = cachedSlotInfos;
            RefreshDisplay();
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_slotInfos == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(CachedValue))
            {
                Label.Clear();
                return;
            }

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (SpineSlotUtils.SlotInfo slotInfo in _slotInfos)
            {
                // ReSharper disable once InvertIf
                if (slotInfo.SlotData.Name == CachedValue)
                {
                    UIToolkitUtils.SetLabel(Label, new[]
                    {
                        new RichTextDrawer.RichTextChunk
                        {
                            IsIcon = true,
                            Content = SpineSlotUtils.IconPath,
                        },
                        new RichTextDrawer.RichTextChunk
                        {
                            IsIcon = false,
                            Content = slotInfo.SlotData.Name,
                        },
                    }, _richTextDrawer);
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
