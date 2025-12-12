using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements.ValueButtons;

namespace SaintsField.Editor.Drawers.ValueButtonsDrawer
{
    public class ValueButtonsCalcElement: AbsValueButtonsCalcElement<ValueButton>
    {
        protected override AbsValueButton CreateValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks)
        {
            return new ValueButton(chunks);
        }
    }
}
