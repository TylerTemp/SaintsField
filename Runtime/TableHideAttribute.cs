using System;
using System.Diagnostics;
using SaintsField.Playa;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class TableHideAttribute: Attribute, IPlayaAttribute
    {
        // public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        // public string GroupBy => "__LABEL_FIELD__";
    }
}
