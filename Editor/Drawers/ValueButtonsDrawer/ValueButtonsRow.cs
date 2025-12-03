using System.Collections.Generic;
using SaintsField.Editor.Core;

namespace SaintsField.Editor.Drawers.ValueButtonsDrawer
{
    public class ValueButtonsRow: AbsValueButtonsRow<ValueButton>
    {
        protected override ValueButton MakeValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks)
        {
            return new ValueButton(chunks);
        }
    }
}
