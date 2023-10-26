using System;
using UnityEngine;

namespace ExtInspector.Standalone
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RichLabelAttribute: PropertyAttribute, ISaintsAttribute
    {
        public string GroupBy => "__LABEL_FIELD__";

        public SaintsAttributeType AttributeType => SaintsAttributeType.Label;

        public readonly string RichTextXml;
        public RichLabelAttribute(string richTextXml)
        {
            RichTextXml = richTextXml;
        }
    }
}
