using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.UIToolkitElements.ValueButtons;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public class FlagButtonsRow: AbsValueButtonsRow<FlagButton>
    {
        private readonly bool _isULong;
        public FlagButtonsRow(bool isULong)
        {
            _isULong = isULong;
        }

        protected override FlagButton MakeValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks)
        {
            return new FlagButton(_isULong, chunks);
        }
    }
}
