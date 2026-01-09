using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.UIToolkitElements.ValueButtons;

namespace SaintsField.Editor.Playa.RendererGroup.TabGroup
{
    public class TabButtonsCalcElement: AbsValueButtonsCalcElement<TabButton>
    {
        protected override AbsValueButton CreateValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks)
        {
            return new ValueButton(chunks);
        }
    }
}
