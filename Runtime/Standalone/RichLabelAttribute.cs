using System;
using UnityEngine;

namespace ExtInspector.Standalone
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RichLabelAttribute: PropertyAttribute
    {
        public readonly string RichTextXml;
        public RichLabelAttribute(string richTextXml)
        {
            RichTextXml = richTextXml;
        }
    }
}
