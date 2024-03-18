using System;
using UnityEngine;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RichLabelAttribute: PropertyAttribute, ISaintsAttribute
    {
        public virtual SaintsAttributeType AttributeType => SaintsAttributeType.Label;
        public virtual string GroupBy => "__LABEL_FIELD__";

        // ReSharper disable InconsistentNaming
        public readonly string RichTextXml;
        public readonly bool IsCallback;
        // ReSharper enable InconsistentNaming

        public RichLabelAttribute(string richTextXml, bool isCallback=false)
        {
            RichTextXml = richTextXml;
            IsCallback = isCallback;
        }
    }
}
