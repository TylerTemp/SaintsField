using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class FieldLabelTextAttribute: PropertyAttribute, ISaintsAttribute
    {
        public virtual SaintsAttributeType AttributeType => SaintsAttributeType.Label;
        public virtual string GroupBy => "__LABEL_FIELD__";

        // ReSharper disable InconsistentNaming
        public readonly string RichTextXml;
        public readonly bool IsCallback;
        // ReSharper enable InconsistentNaming

        public FieldLabelTextAttribute(string richTextXml, bool isCallback=false)
        {
            (string parsedContent, bool parsedIsCallback) = RuntimeUtil.ParseCallback(richTextXml, isCallback);

            RichTextXml = parsedContent;
            IsCallback = parsedIsCallback;
        }
    }
}
