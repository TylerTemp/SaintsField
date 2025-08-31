﻿using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class AboveRichLabelAttribute : FullWidthRichLabelAttribute
    {
        public AboveRichLabelAttribute(string richTextXml="<color=gray><label />", bool isCallback = false, string groupBy = "") : base(richTextXml, isCallback, true, groupBy)
        {
        }
    }
}
