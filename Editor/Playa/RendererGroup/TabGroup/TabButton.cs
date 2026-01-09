using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements.ValueButtons;
using SaintsField.Editor.Utils;

namespace SaintsField.Editor.Playa.RendererGroup.TabGroup
{
    public class TabButton: AbsValueButton
    {
        public TabButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks): base(chunks)
        {
        }

        protected override bool IsOn(object curValue)
        {
            return Util.GetIsEqual(curValue, Value);
        }
    }
}
