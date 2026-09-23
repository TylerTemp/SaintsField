using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements.ValueButtons;

namespace SaintsField.Editor.Drawers.ValueButtonsDrawer
{
    public class ValueButtonsRow: AbsValueButtonsRow<ValueButton>
    {
        protected override ValueButton MakeValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks, bool obsolete)
        {
            ValueButton result = new ValueButton(chunks, obsolete);
            result.SetLabelCenter();
            return result;
        }
    }
}
