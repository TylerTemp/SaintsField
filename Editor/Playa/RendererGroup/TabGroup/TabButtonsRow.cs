using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements.ValueButtons;

namespace SaintsField.Editor.Playa.RendererGroup.TabGroup
{
    public class TabButtonsRow: AbsValueButtonsRow<TabButton>
    {
        protected override TabButton MakeValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks)
        {
            TabButton result =  new TabButton(chunks);
            result.SetLabelCenter();
            return result;
        }
    }
}
