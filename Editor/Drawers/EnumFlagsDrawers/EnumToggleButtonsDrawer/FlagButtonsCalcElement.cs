using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.UIToolkitElements.ValueButtons;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public class FlagButtonsCalcElement: AbsValueButtonsCalcElement<FlagButton>
    {
        private readonly bool _isULong;

        public FlagButtonsCalcElement(bool isULong)
        {
            _isULong = isULong;
        }

        protected override AbsValueButton CreateValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks)
        {
            return new FlagButton(_isULong, chunks);
        }
    }
}
