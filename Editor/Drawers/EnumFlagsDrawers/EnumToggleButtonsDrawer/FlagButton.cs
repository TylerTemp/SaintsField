using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.EnumToggleButtonsDrawer
{
    public class FlagButton: AbsValueButton
    {
        private readonly bool _isULong;

        public FlagButton(bool isULong, IReadOnlyList<RichTextDrawer.RichTextChunk> chunks) : base(chunks)
        {
            _isULong = isULong;
        }

        public override bool IsOn(object curValue)
        {
            object toggleValue = Value;
            return EnumFlagsUtil.IsOnObject(curValue, toggleValue, _isULong);
        }
    }
}
