using System;
using UnityEngine;

namespace SaintsField
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumFlagsAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        public readonly bool AutoExpand;
        public readonly bool DefaultExpanded;

        public EnumFlagsAttribute(bool autoExpand=true, bool defaultExpanded=false)
        {
            AutoExpand = autoExpand;
            DefaultExpanded = defaultExpanded;
        }
    }
}
