using System.Diagnostics;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class FullWidthRichLabelAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly bool Above;
        public readonly string RichTextXml;
        public readonly bool IsCallback;

        // ReSharper disable once MemberCanBeProtected.Global
        public FullWidthRichLabelAttribute(string richTextXml, bool isCallback=false, bool above=false, string groupBy="")
        {
            GroupBy = groupBy;

            Above = above;

            (string parsedRich, bool parsedIsCallback) = RuntimeUtil.ParseCallback(richTextXml, isCallback);

            RichTextXml = parsedRich;
            IsCallback = parsedIsCallback;
        }
    }
}
