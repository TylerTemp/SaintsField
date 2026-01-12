using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements.ValueButtons;
using SaintsField.Editor.Utils;
using UnityEngine;

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

        protected override void SetOnStyle(bool isFirst, bool isLast)
        {
            // const float gray = 0.15f;
            // const float grayBorder = 0.45f;
            style.backgroundColor = SaintsRendererGroup.FancyBgColor;
            // Color borderColor = new Color(grayBorder, 0.6f, grayBorder, 1f);
            // Color borderColor = SaintsRendererGroup.FancyBorderColor;
            Color borderColor = new Color32(123, 174, 250, 255);
            style.borderTopColor = style.borderLeftColor = style.borderRightColor = borderColor;
            style.borderBottomWidth = 0;
            style.borderLeftWidth = 1;
        }

        protected override void SetOffStyle(bool isFirst, bool isLast)
        {
            base.SetOffStyle(isFirst, isLast);
            style.borderBottomWidth = 1;
        }
    }
}
