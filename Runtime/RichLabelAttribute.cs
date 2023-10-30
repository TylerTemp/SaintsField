using System;
using UnityEngine;

namespace ExtInspector
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RichLabelAttribute: PropertyAttribute, ISaintsAttribute
    {
        public string GroupBy => "__LABEL_FIELD__";

        public SaintsAttributeType AttributeType => SaintsAttributeType.Label;

        public readonly string RichTextXml;
        public readonly bool IsCallback;

        public RichLabelAttribute(string richTextXml, bool isCallback=false)
        {
            RichTextXml = richTextXml;
            IsCallback = isCallback;
        }
    }
}
