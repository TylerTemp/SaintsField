using System;
using UnityEngine;

namespace ExtInspector.Standalone
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RichLabelAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Label;
        public string DrawerClass => "ExtInspector.Editor.Standalone.RichLabelAttributeDrawer";

        public readonly string RichTextXml;
        public RichLabelAttribute(string richTextXml)
        {
            RichTextXml = richTextXml;
        }
    }
}
