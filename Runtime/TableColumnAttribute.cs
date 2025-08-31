using System;
using System.Diagnostics;
using SaintsField.Playa;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    // [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TableColumnAttribute: Attribute, IPlayaAttribute
    {
        // public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        // public string GroupBy => "__LABEL_FIELD__";

        public readonly string Title;

        public TableColumnAttribute(string title)
        {
            Title = title;
        }
    }
}
