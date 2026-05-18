using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EndTextAttribute: FieldLabelTextAttribute
    {
        public override SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public override string GroupBy { get; }

        public readonly float Padding;

        public readonly string ShowCallback;

        public EndTextAttribute(string richTextXml, bool isCallback=false, float padding=5f, string show=null, string groupBy=""): base(richTextXml, isCallback)
        {
            Padding = padding;
            GroupBy = groupBy;
            ShowCallback = show;
        }
    }
}
