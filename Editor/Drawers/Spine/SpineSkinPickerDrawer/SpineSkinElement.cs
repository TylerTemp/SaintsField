using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using Spine;

namespace SaintsField.Editor.Drawers.Spine.SpineSkinPickerDrawer
{
    public class SpineSkinElement: StringDropdownElement
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private ExposedList<Skin> _skinList;

        public void BindSkinList(ExposedList<Skin> skins)
        {
            _skinList = skins;
            RefreshDisplay();
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_skinList == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(CachedValue))
            {
                Label.Clear();
                return;
            }

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (Skin skin in _skinList)
            {
                // ReSharper disable once InvertIf
                if (skin.Name == CachedValue)
                {
                    UIToolkitUtils.SetLabel(Label, new[]
                    {
                        new RichTextDrawer.RichTextChunk
                        {
                            IsIcon = true,
                            Content = SpineSkinUtils.IconPath,
                        },
                        new RichTextDrawer.RichTextChunk
                        {
                            IsIcon = false,
                            Content = skin.Name,
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
