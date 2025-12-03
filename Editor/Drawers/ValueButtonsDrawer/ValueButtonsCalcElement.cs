using System.Collections.Generic;
using SaintsField.Editor.Core;

namespace SaintsField.Editor.Drawers.ValueButtonsDrawer
{
    public class ValueButtonsCalcElement: AbsValueButtonsCalcElement<ValueButton>
    {
        public override AbsValueButton CreateValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks)
        {
            return new ValueButton(chunks);
        }
    }
}
